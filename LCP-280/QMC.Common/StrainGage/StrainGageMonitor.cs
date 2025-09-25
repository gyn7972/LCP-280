using NationalInstruments.DAQmx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QMC.Common.StrainGage
{
    public class StrainGageMonitor : IDisposable
    {
        private const int MonitoringIntervalMs = 5;

        #region Fields
        NationalInstruments.DAQmx.Task readerTask = new NationalInstruments.DAQmx.Task();
        List<(StrainGage strainGage, string channelName)> items = new List<(StrainGage strainGage, string channelName)>();

        private CancellationTokenSource cts;
        private System.Threading.Tasks.Task monitoringTask;
        private readonly object gate = new object();

        private bool _disposed = false;
        #endregion

        #region Properties
        public IReadOnlyList<(StrainGage strainGage, string channelName)> Items => items.AsReadOnly();
        #endregion

        #region Constructor
        public StrainGageMonitor()
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
        public bool Clear()
        {
            Stop();
            try
            {
                readerTask.Stop();
                readerTask.Dispose();
                readerTask = new NationalInstruments.DAQmx.Task();

                items.Clear();
                return true;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return false;
            }
        }
        public bool Add(StrainGage strainGage)
        {
            if (strainGage == null)
                return false;
            if (items.Any(item => item.strainGage == strainGage))
                return false;

            try
            {
                if (!strainGage.Config.IsSimulation)
                {
                    readerTask.AIChannels.CreateVoltageChannel(strainGage.Config.ReadChannelName
                    , $"id{items.Count}"
                    , AITerminalConfiguration.Rse
                    , strainGage.Config.MinVoltage
                    , strainGage.Config.MaxVoltage
                    , AIVoltageUnits.Volts);
                }

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
            //lock (gate)
            {
                if (monitoringTask != null && !monitoringTask.IsCompleted)
                    return;
                cts = new CancellationTokenSource();
                monitoringTask = System.Threading.Tasks.Task.Run(() => RunMonitoring(cts.Token), cts.Token);
            }
        }
        public void Stop()
        {
            //lock (gate)
            {
                cts?.Cancel();
                monitoringTask?.Wait(500);
                cts?.Dispose();
                cts = null;
            }
            //try { monitoringTask?.Wait(); } catch { /* ignore */ }
        }

        private async System.Threading.Tasks.Task RunMonitoring(CancellationToken token)
        {
            bool IsSimulation = false;
            foreach (var item in items)
            {
                if (item.strainGage.Config.IsSimulation)
                {
                    IsSimulation = true;
                    break;
                }
            }

            if (!IsSimulation)
            {
                var reader = new AnalogMultiChannelReader(readerTask.Stream);
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        double[] data = reader.ReadSingleSample();
                        for (int i = 0; i < items.Count; i++)
                        {
                            items[i].strainGage.UpdateVoltage(data[i]);
                        }

                        // Event
                        OnVoltageUpdated?.Invoke(this, EventArgs.Empty);
                        await System.Threading.Tasks.Task.Delay(MonitoringIntervalMs, token);
                    }
                    catch { }
                }
            }
            else
            {
                double amplitude = 1.0; // 사인파 진폭
                double noiseLevel = 0.2;  // 노이즈 진폭

                Random _rand = new Random();
                int _simStep = 0;

                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        _simStep++;
                        double sine = amplitude * Math.Sin(2 * Math.PI * _simStep * 0.02);
                        double noise = (2 * _rand.NextDouble() - 1) * noiseLevel; // -noiseLevel ~ +noiseLevel
                        double simulatedVoltage = sine + noise;

                        for (int i = 0; i < items.Count; i++)
                        {
                            if (items[i].strainGage.Config.IsSimulation)
                                items[i].strainGage.UpdateVoltage(simulatedVoltage);
                            else
                                items[i].strainGage.UpdateVoltage(0);
                        }

                        // Event
                        OnVoltageUpdated?.Invoke(this, EventArgs.Empty);
                        await System.Threading.Tasks.Task.Delay(MonitoringIntervalMs, token);
                    }
                    catch { }
                }
            }      
        }
        #endregion
    }
}
