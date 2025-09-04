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
            public CommandAction action { get; set; }

            // Source
            public double sourceValue { get; set; }
            public double sourceTime { get; set; }
            public double sourceLimit { get; set; }
            public double sourceRange { get; set; }

            // Measure
            public double measureRange { get; set; }
            public double measureTime { get; set; }

            // Pulse Sweep
            public double pulseWidth { get; set; }
            public double pulsePeriod { get; set; }
            public int pulseCount { get; set; }
        }
        #endregion

        #region Field
        private List<ChannelCommand> commands;
        private StringBuilder stringBuilder;
        #endregion

        #region Property
        public string Name { get; set; }
        public KeithleySourcemeter Owner { get; private set; }
        #endregion

        #region Constructor
        public KeithleySourcemeterChannel(string name, KeithleySourcemeter owner)
        {
            Name = name;
            Owner = owner;

            commands = new List<ChannelCommand>();
            stringBuilder = new StringBuilder();
        }
        #endregion

        #region Method
        public int ApplyConfig()
        {
            try
            {
                KeithelySourcemeterConfig config = Owner.Config;

                stringBuilder.Clear();
                stringBuilder.AppendLine($"{Name}.sense = {(int)config.SenseMode}");
                stringBuilder.AppendLine($"{Name}.source.sink = {(int)config.SourceSink}");
                stringBuilder.AppendLine($"{Name}.source.settling = {(int)config.SourceSettling}");
                stringBuilder.AppendLine($"{Name}.source.offmode = {(int)config.SourceOffmode}");

                if (!Owner.Communicator.Write(stringBuilder.ToString()))
                    throw new Exception($"[{Name}] Failed to apply config.");
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return -1;
            }
            return 0;
        }
        public int ChannelReset()
        {
            try
            {
                stringBuilder.Clear();
                stringBuilder.AppendLine($"{Name}.reset()");

                if (!Owner.Communicator.Write(stringBuilder.ToString()))
                    throw new Exception($"[{Name}] Failed to apply config.");
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return -1;
            }
            return 0;
        }
        public int ChannelOn(bool on)
        {
            try
            {
                stringBuilder.Clear();
                int value = on ? 1 : 0;
                stringBuilder.AppendLine($"{Name}.source.output = {value}");

                if (!Owner.Communicator.Write(stringBuilder.ToString()))
                    throw new Exception($"[{Name}] Failed to apply config.");
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return -1;
            }
            return 0;
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
        public int RunMeasureCommands()
        {
            try
            {
                stringBuilder.Clear();

                // begin run command
                stringBuilder.AppendLine("startmeasure()");

                var args = new List<string>();

                foreach (var cmd in commands)
                {
                    switch (cmd.action)
                    {
                        case CommandAction.MeasureI:
                            args.Add(Name);
                            args.Add(cmd.sourceValue.ToString());
                            args.Add(cmd.sourceRange.ToString());
                            args.Add(cmd.measureRange.ToString());
                            args.Add(cmd.measureTime.ToString());
                            args.Add(cmd.sourceLimit.ToString());
                            args.Add(cmd.sourceTime.ToString());
                            stringBuilder.AppendLine("vi(" + string.Join(",", args) + ")");
                            break;

                        case CommandAction.MeasureV:
                            args.Add(Name);
                            args.Add(cmd.sourceValue.ToString());
                            args.Add(cmd.sourceRange.ToString());
                            args.Add(cmd.measureRange.ToString());
                            args.Add(cmd.measureTime.ToString());
                            args.Add(cmd.sourceLimit.ToString());
                            args.Add(cmd.sourceTime.ToString());
                            stringBuilder.AppendLine("iv(" + string.Join(",", args) + ")");
                            break;

                        case CommandAction.MeasureVAndPulseTrigger:
                            args.Add(Name);
                            args.Add(cmd.sourceValue.ToString());
                            args.Add(cmd.sourceRange.ToString());
                            args.Add(cmd.measureRange.ToString());
                            args.Add(cmd.measureTime.ToString());
                            args.Add(cmd.sourceLimit.ToString());
                            args.Add(cmd.sourceTime.ToString());
                            args.Add(cmd.pulsePeriod.ToString());
                            args.Add(cmd.pulseWidth.ToString());
                            args.Add(cmd.pulseCount.ToString());
                            stringBuilder.AppendLine("VF_Pulse(" + string.Join(",", args) + ")");
                            break;
                    }
                }

                // end run command
                stringBuilder.AppendLine("endmeasure()");

                if (!Owner.Communicator.Write(stringBuilder.ToString()))
                    throw new Exception($"[{Name}] Failed to send messsage: {stringBuilder.ToString()}");
            }
            catch (Exception ex)
            {
                // Error handling
                Log.Write(ex);
                return -1;
            }
            return 0;
        }
        public bool WaitComplete()
        {
            try
            {
                string response = "";
                if (!Owner.Communicator.Query("*OPC?", ref response))
                    throw new Exception($"[{Name}] Failed to query opc.");

                if (response == "1")
                    return true;
            }
            catch (Exception ex)
            {
                // Error handling
                Log.Write(ex);
                return false;
            }
            return false;
        }
        public int ReadBufferString(ref string buffer)
        {
            try
            {
                string bufferName = $"{Name}.nvbuffer1";
                string bufferReadCommand = $"printbuffer(1, {bufferName}.n, {bufferName}.readings";
                string bufferData = "";

                if (!Owner.Communicator.Query(bufferReadCommand, ref bufferData))
                    throw new Exception($"[{Name}] Failed to read buffer.");
            }
            catch (Exception ex)
            {
                // Error handling
                Log.Write(ex);
                return -1;
            }
            return 0;
        }
        #endregion
    }
}
