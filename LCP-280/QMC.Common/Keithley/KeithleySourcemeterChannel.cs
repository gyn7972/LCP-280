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
            MeasureVAndPulseTrigger,
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
        private StringBuilder stringBuilder = new StringBuilder();
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
        public bool ApplyConfig()
        {
            try
            {
                KeithelySourcemeterConfig config = Owner.Config;

                stringBuilder.Clear();
                stringBuilder.AppendLine($"{Name}.sense = {(int)config.SenseMode}");
                stringBuilder.AppendLine($"{Name}.source.sink = {(int)config.SourceSink}");
                stringBuilder.AppendLine($"{Name}.source.settling = {(int)config.SourceSettling}");
                stringBuilder.AppendLine($"{Name}.source.offmode = {(int)config.SourceOffmode}");

                return Owner.Communicator.Write(stringBuilder.ToString());
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
                stringBuilder.Clear();
                stringBuilder.AppendLine($"{Name}.reset()");

                return Owner.Communicator.Write(stringBuilder.ToString());
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
                stringBuilder.Clear();
                int value = on ? 1 : 0;
                stringBuilder.AppendLine($"{Name}.source.output = {value}");

                return Owner.Communicator.Write(stringBuilder.ToString());
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
        public bool RunMeasureCommands()
        {
            bufferDatas.Clear();
            try
            {
                stringBuilder.Clear();

                // begin run command
                stringBuilder.AppendLine("startmeasure()");

                var args = new List<string>();

                foreach (var cmd in commands)
                {
                    switch (cmd.Action)
                    {
                        case CommandAction.MeasureI:
                            args.Add(Name);
                            args.Add(cmd.SourceValue.ToString());
                            args.Add(cmd.SourceRange.ToString());
                            args.Add(cmd.MeasureRange.ToString());
                            args.Add(cmd.MeasureTime.ToString());
                            args.Add(cmd.SourceLimit.ToString());
                            args.Add(cmd.SourceTime.ToString());
                            stringBuilder.AppendLine("vi(" + string.Join(",", args) + ")");
                            break;
                        case CommandAction.MeasureV:
                            args.Add(Name);
                            args.Add(cmd.SourceValue.ToString());
                            args.Add(cmd.SourceRange.ToString());
                            args.Add(cmd.MeasureRange.ToString());
                            args.Add(cmd.MeasureTime.ToString());
                            args.Add(cmd.SourceLimit.ToString());
                            args.Add(cmd.SourceTime.ToString());
                            stringBuilder.AppendLine("iv(" + string.Join(",", args) + ")");
                            break;
                        case CommandAction.MeasureVAndPulseTrigger:
                            args.Add(Name);
                            args.Add(cmd.SourceValue.ToString());
                            args.Add(cmd.SourceRange.ToString());
                            args.Add(cmd.MeasureRange.ToString());
                            args.Add(cmd.MeasureTime.ToString());
                            args.Add(cmd.SourceLimit.ToString());
                            args.Add(cmd.SourceTime.ToString());
                            args.Add(cmd.PulsePeriod.ToString());
                            args.Add(cmd.PulseWidth.ToString());
                            args.Add(cmd.PulseCount.ToString());
                            stringBuilder.AppendLine("VF_Pulse(" + string.Join(",", args) + ")");
                            break;
                    }
                }

                // end run command
                stringBuilder.AppendLine("endmeasure()");

                return Owner.Communicator.Write(stringBuilder.ToString());
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
                if (!Owner.Communicator.Query("*OPC?", ref response))
                    return false;

                return (response == "1");
            }
            catch (Exception ex)
            {
                // Error handling
                Log.Write(ex);
                return false;
            }
        }
        public bool ReadBufferData()
        {
            try
            {
                string bufferName = $"{Name}.nvbuffer1";
                string bufferReadCommand = $"printbuffer(1, {bufferName}.n, {bufferName}.readings";
                
                string bufferData = "";
                if (!Owner.Communicator.Query(bufferReadCommand, ref bufferData))
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
            Random rand = new Random();
            for (int i = 0; i < commands.Count; i++)
            {
                double simulatedValue = Math.Round(rand.NextDouble() * 10, 3); // Simulate a value between 0 and 10
                bufferDatas.Add(simulatedValue.ToString());
            }
        }
        #endregion
    }
}
