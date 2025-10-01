using NationalInstruments.DAQmx;
using QMC.Common.Component;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;

namespace QMC.Common.StrainGage
{
    public class StrainGageMonitor : BaseComponent, IDisposable
    {
        private const int MonitoringIntervalMs = 20;

        #region Fields
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
        public StrainGageMonitor(string name) : base(name)
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
        public void Clear()
        {
            items.Clear();
        }
        public bool Add(StrainGage strainGage)
        {
            if (strainGage == null)
                return false;
            if (items.Any(item => item.strainGage == strainGage))
                return false;

            items.Add((strainGage, strainGage.Config.ReadChannelName));
            return true;
        }
        public void Start()
        {
            lock (gate)
            {
                if (monitoringTask != null && !monitoringTask.IsCompleted)
                    return;
                cts = new CancellationTokenSource();

                Thread.CurrentThread.Name = "StrainGageMonitor";
                monitoringTask = System.Threading.Tasks.Task.Run(() => RunMonitoring(cts.Token), cts.Token);
            }
        }
        public void Stop()
        {
            lock (gate)
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
                NationalInstruments.DAQmx.Task readerTask = null;
                try
                {
                    // Create DAQmx Task
                    readerTask = new NationalInstruments.DAQmx.Task();

                    // Create Voltage Channels
                    int idIndex = 0;
                    foreach (var item in items)
                    {
                        var gage = item.strainGage;
                        readerTask.AIChannels.CreateVoltageChannel(gage.Config.ReadChannelName
                            , $"id{idIndex++}"
                            , AITerminalConfiguration.Rse
                            , gage.Config.MinVoltage
                            , gage.Config.MaxVoltage
                            , AIVoltageUnits.Volts);
                    }

                    // Create Reader & Start Read Sample
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
                catch (Exception)
                {
                    return;
                }
                finally
                {
                    if (readerTask != null)
                    {
                        readerTask.Stop();
                        readerTask.Dispose();
                    }
                }
            }
            else
            {
                Random random = new Random();
                int step = 0;

                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        step++;

                        for (int i = 0; i < items.Count; i++)
                        {
                            if (items[i].strainGage.Config.IsSimulation)
                            {
                                // calculate simulated voltage for each strain gage if needed
                                double noiseLevel = 0.1;
                                double sine = Math.Sin(Math.PI * step * 0.05) + 1;
                                double noise = (2 * random.NextDouble() - 1) * noiseLevel;

                                double simulatedVoltage = sine + noise;
                                simulatedVoltage = simulatedVoltage * (items[i].strainGage.Config.MaxVoltage - items[i].strainGage.Config.MinVoltage) / 5.0;

                                items[i].strainGage.UpdateVoltage(simulatedVoltage);
                            }
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
