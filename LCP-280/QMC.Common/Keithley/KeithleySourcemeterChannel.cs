using QMC.Common.Component;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common.Keithley
{
    public class KeithleySourcemeterChannel
    {
        #region Defines
        public enum CommandAction
        {
            MeasureI,
            MeasureV,
            PulseSweepIAndTrigger,
            PulseSweepVAndTrigger,
        }
        public class ChannelCommand
        {
            public string Name { get; set; }
            public CommandAction Action { get; set; }

            // Source
            public double SourceValue { get; set; }
            public double SourceTime { get; set; }
            public double SourceLimit { get; set; }
            public double SourceRange { get; set; }

            // Measure
            public double MeasureRange { get; set; }
            public double MeasureTime { get; set; }

            // Pulse Sweep
            public double PulseWidth { get; set; }
            public double PulsePeriod { get; set; }
            public int PulseCount { get; set; }
        }
        #endregion

        #region Field
        private List<ChannelCommand> commands = new List<ChannelCommand>();
        private List<string> bufferDatas = new List<string>();
        #endregion

        #region Property
        public string Name { get; set; }
        public KeithleySourcemeter Owner { get; private set; }
        public string[] BufferDatas => bufferDatas.ToArray();
        #endregion

        #region Constructor
        public KeithleySourcemeterChannel(string name, KeithleySourcemeter owner)
        {
            Name = name;
            Owner = owner;
        }
        #endregion

        #region Method
        public bool Init()
        {
            try
            {
                string[] cmdStrs = new string[]
                {
                    $"initChannel({Name})",
                };

                KeithleyInstrumentCommunicator comm = Owner.Communicator;
                foreach (var cmd in cmdStrs)
                {
                    if (!comm.Write(cmd))
                        throw new Exception($"[{Name}] Failed to send Command: {cmd}");
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return false;
            }
        }
        public bool ApplyConfig()
        {
            try
            {
                KeithelySourcemeterConfig config = Owner.Config;
                string[] cmdStrs = new string[]
                {
                    $"{Name}.sense = {(int)config.SenseMode}",
                    $"{Name}.source.sink = {(int)config.SourceSink}",
                    $"{Name}.source.settling = {(int)config.SourceSettling}",
                    $"{Name}.source.offmode = {(int)config.SourceOffmode}",
                };

                KeithleyInstrumentCommunicator comm = Owner.Communicator;
                foreach (var cmd in cmdStrs)
                {
                    if (!comm.Write(cmd))
                        throw new Exception($"[{Name}] Failed to send Command: {cmd}");
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return false;
            }
        }
        public bool ChannelReset()
        {
            try
            {
                string[] cmdStrs = new string[]
                {
                    $"{Name}.reset()",
                };

                KeithleyInstrumentCommunicator comm = Owner.Communicator;
                foreach (var cmd in cmdStrs)
                {
                    if (!comm.Write(cmd))
                        throw new Exception($"[{Name}] Failed to send Command: {cmd}");
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return false;
            }
        }
        public bool ChannelOn(bool on)
        {
            try
            {
                int value = on ? 1 : 0;
                string[] cmdStrs = new string[]
                {
                    $"{Name}.source.output = {value}",
                };

                KeithleyInstrumentCommunicator comm = Owner.Communicator;
                foreach (var cmd in cmdStrs)
                {
                    if (!comm.Write(cmd))
                        throw new Exception($"[{Name}] Failed to send Command: {cmd}");
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return false;
            }
        }
        #endregion

        #region Command Method
        public void ClearCommands()
        {
            commands.Clear();
        }
        public void AddCommand(ChannelCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            commands.Add(command);
        }
        public bool RunCommands()
        {
            bufferDatas.Clear();
            try
            {
                var cmdStrs = new List<string>();
                var args = new List<string>();

                cmdStrs.Add($"startmeasure({Name})");
                foreach (var cmd in commands)
                {
                    switch (cmd.Action)
                    {
                        case CommandAction.MeasureI:
                            {
                                args.Clear();
                                args.Add(Name);
                                args.Add(cmd.SourceValue.ToString());
                                args.Add(cmd.SourceRange.ToString());
                                args.Add(cmd.SourceTime.ToString());
                                args.Add(cmd.SourceLimit.ToString());
                                args.Add(cmd.MeasureRange.ToString());
                                args.Add(cmd.MeasureTime.ToString());
                                cmdStrs.Add("vi(" + string.Join(",", args) + ")");
                            }
                            break;
                        case CommandAction.MeasureV:
                            {
                                args.Clear();
                                args.Add(Name);
                                args.Add(cmd.SourceValue.ToString());
                                args.Add(cmd.SourceRange.ToString());
                                args.Add(cmd.SourceTime.ToString());
                                args.Add(cmd.SourceLimit.ToString());
                                args.Add(cmd.MeasureRange.ToString());
                                args.Add(cmd.MeasureTime.ToString());
                                cmdStrs.Add("iv(" + string.Join(",", args) + ")");
                            }
                            break;
                        case CommandAction.PulseSweepIAndTrigger:
                            {
                                args.Clear();
                                args.Add(Name);
                                args.Add(cmd.SourceValue.ToString());
                                args.Add(cmd.PulseWidth.ToString());
                                args.Add(cmd.PulsePeriod.ToString());
                                args.Add(cmd.PulseCount.ToString());
                                cmdStrs.Add("iPulseAndTrigger(" + string.Join(",", args) + ")");
                            }
                            break;
                        case CommandAction.PulseSweepVAndTrigger:
                            {
                                args.Clear();
                                args.Add(Name);
                                args.Add(cmd.SourceValue.ToString());
                                args.Add(cmd.PulseWidth.ToString());
                                args.Add(cmd.PulsePeriod.ToString());
                                args.Add(cmd.PulseCount.ToString());
                                cmdStrs.Add("vPulseAndTrigger(" + string.Join(",", args) + ")");
                            }
                            break;
                    }
                }
                cmdStrs.Add($"endmeasure({Name})");

                KeithleyInstrumentCommunicator comm = Owner.Communicator;
                foreach (var cmd in cmdStrs)
                {
                    if (!comm.Write(cmd))
                        throw new Exception($"[{Name}] Failed to send Command: {cmd}");
                }
                return true;
            }
            catch (Exception ex)
            {
                // Error handling
                Log.Write(ex);
                return false;
            }
        }
        public bool WaitComplete()
        {
            try
            {
                string response = "";

                KeithleyInstrumentCommunicator comm = Owner.Communicator;
                //if (!comm.Query("*OPC?", ref response))
                //    return false;

                ////response = response.Trim();
                //response = response.Replace("\n", "");
                //response = response.Replace("\r", "");
                //return (response == "1");
                if (comm.Read(ref response))
                {
                    return (response == "measure_end");
                }
            }
            catch (Exception ex)
            {
                // Error handling
                Log.Write(ex);
                return false;
            }
            return false;
        }
        public bool ReadBufferData()
        {
            try
            {
                string bufferName = $"{Name}.nvbuffer1";
                string bufferReadCommand = $"printbuffer(1, {bufferName}.n, {bufferName}.readings)";
                
                string bufferData = "";

                KeithleyInstrumentCommunicator comm = Owner.Communicator;
                if (!comm.Query(bufferReadCommand, ref bufferData))
                    throw new Exception($"[{Name}] Failed to read buffer.");

                bufferData = bufferData.Trim();
                string[] datas = bufferData.Split(new char[] { ',', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                bufferDatas.Clear();
                foreach (var d in datas)
                    bufferDatas.Add(d.Trim());
            }
            catch (Exception ex)
            {
                // Error handling
                bufferDatas.Clear();

                Log.Write(ex);
                return false;
            }
            return true;
        }
        #endregion

        #region Simulation Method
        public void SimulateBufferData()
        {
            bufferDatas.Clear();

            int measureCount = 0;
            foreach (var cmd in commands)
            {
                switch (cmd.Action)
                {
                    case CommandAction.MeasureI:
                    case CommandAction.MeasureV:
                        measureCount++;
                        break;
                }
            }

            Random rand = new Random();
            for (int i = 0; i < measureCount; i++)
            {
                double simulatedValue = Math.Round(rand.NextDouble() * 10, 3); // Simulate a value between 0 and 10
                bufferDatas.Add(simulatedValue.ToString());
            }
        }
        #endregion
    }
}
