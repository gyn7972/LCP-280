//using NationalInstruments.DAQmx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;

namespace QMC.Common.StrainGage
{
    public class StrainGageMonitor : IDisposable
    {
        private const int MonitoringIntervalMs = 1;

        #region Fields
        //NationalInstruments.DAQmx.Task readerTask = new NationalInstruments.DAQmx.Task();
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

        private Random _rand = new Random();
        private int _simStep = 0;

        private async System.Threading.Tasks.Task RunMonitoring(CancellationToken token)
        {
            var reader = new AnalogMultiChannelReader(readerTask.Stream);

            double amplitude = 1.0; // 사인파 진폭
            double frequency = 0.5; // Hz (1초에 0.5회)
            double noiseLevel = 1;  // 노이즈 진폭

            while (!token.IsCancellationRequested)
            {
                try
                {
                    double[] data = reader.ReadSingleSample();
                    for (int i = 0; i < items.Count; i++)
                    {
                        if (!items[i].strainGage.Config.IsSimulation)
                        {
                            items[i].strainGage.UpdateVoltage(data[i]);
                        }
                        else
                        {
                            _simStep++;
                            double t = _simStep * 0.001; // 0.05초(50ms) 간격 기준, 필요시 타이머 간격에 맞게 조정
                            double sine = amplitude * Math.Sin(2 * Math.PI * frequency * t);
                            double noise = (2 * _rand.NextDouble() - 1) * noiseLevel; // -noiseLevel ~ +noiseLevel
                            double simulatedVoltage = sine + noise;

                            items[i].strainGage.UpdateVoltage(simulatedVoltage);
                        }
                    }

                    // Event
                    OnVoltageUpdated?.Invoke(this, EventArgs.Empty);
                }
                catch {}
                await System.Threading.Tasks.Task.Delay(MonitoringIntervalMs, token);
            }
        }
        #endregion
    }
}
