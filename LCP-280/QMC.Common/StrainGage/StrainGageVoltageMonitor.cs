//using NationalInstruments.DAQmx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QMC.Common.StrainGage
{
    public class StrainGageVoltageMonitor : IDisposable
    {
        private const int MonitoringIntervalMs = 50;

        #region Fields
        //NationalInstruments.DAQmx.Task readerTask = new NationalInstruments.DAQmx.Task();
        List<(StrainGage strainGage, string channelName)> items = new List<(StrainGage strainGage, string channelName)>();

        private CancellationTokenSource cts;
        private System.Threading.Tasks.Task monitoringTask;
        private readonly object gate = new object();

        private bool _disposed = false;
        #endregion

        #region Properties
        IReadOnlyList<(StrainGage strainGage, string channelName)> Items => items.AsReadOnly();
        #endregion

        #region Constructor
        public StrainGageVoltageMonitor()
        {
        }
        #endregion

        #region Event
        public event EventHandler OnVoltageUpdated;
        #endregion

        #region IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // 관리되는 리소스 해제
                    Stop();
                }

                // 관리되지 않는 리소스 해제
                _disposed = true;
            }
        }
        #endregion

        #region Methods
        public bool Add(StrainGage strainGage)
        {
            if (strainGage == null)
                return false;
            if (items.Any(item => item.strainGage == strainGage))
                return false;

            try
            {
                //readerTask.AIChannels.CreateVoltageChannel(strainGage.Config.ReadChannelName
                //    , $"channel{items.Count}"
                //    , AITerminalConfiguration.Rse
                //    , strainGage.Config.MinVoltage
                //    , strainGage.Config.MaxVoltage
                //    , AIVoltageUnits.Volts);

                items.Add((strainGage, strainGage.Config.ReadChannelName));
                return true;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return false;
            }
        }
        public void Start()
        {
            lock (gate)
            {
                if (monitoringTask != null && !monitoringTask.IsCompleted)
                    return;
                cts = new CancellationTokenSource();
                monitoringTask = System.Threading.Tasks.Task.Run(() => RunMonitoring(cts.Token), cts.Token);
            }
        }
        public void Stop()
        {
            lock (gate)
            {
                cts?.Cancel();
            }
            try { monitoringTask?.Wait(); } catch { /* ignore */ }
        }
        private async System.Threading.Tasks.Task RunMonitoring(CancellationToken token)
        {
            //var reader = new AnalogMultiChannelReader(readerTask.Stream);
            //Random rdm = new Random();

            //while (!token.IsCancellationRequested)
            //{
            //    try
            //    {
            //        double[] data = reader.ReadSingleSample();
            //        for (int i = 0; i < items.Count; i++)
            //        {
            //            items[i].strainGage.UpdateVoltage(data[i]);
            //        }

            //        //double[] data = new double[items.Count];
            //        //for (int i = 0; i < items.Count; i++)
            //        //{
            //        //    data[i] = rdm.NextDouble() * 0.4 + 0.1;
            //        //    items[i].strainGage.UpdateVoltage(data[i]);
            //        //}

            //        // Event
            //        OnVoltageUpdated?.Invoke(this, EventArgs.Empty);
            //    }
            //    catch {}
            //    await System.Threading.Tasks.Task.Delay(MonitoringIntervalMs, token);
            //}
        }
        #endregion
    }
}
