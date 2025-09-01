using QMC.Common.Component;
using QMC.Common.Motion.Ajin;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;

namespace QMC.Common.Motions.CKD
{
    public class CKDMotorDriver : BaseComponent, IDisposable
    {
        #region Define
        public enum PDOProcessImage
        {
            TxPdoInputSignal1,
            TxPdoInputSignal2,
            TxPdoInputData1,
            TxPdoInputData2,
            TxPdoInputData3,
            TxPdoInputData4,
            TxPdoInputData5,
            TxPdoInputCommand1,
            TxPdoInputCommand2,
            TxPdoInputCommand3,
            RxPdoOutputSignal1,
            RxPdoOutputSignal2,
            RxPdoOutputData1,
            RxPdoOutputData2,
            RxPdoOutputData3,
            RxPdoOutputData4,
            RxPdoOutputData5,
            RxPdoOutputCommand1,
            RxPdoOutputCommand2,
            RxPdoOutputCommand3,
        }

        public enum OutputSignal1Mapping
        {
            ProgramNo1,
            programNo2,
            ProgramNo3,
            ProgramNo4,
            ProgramNoSetting2ndDigit,
            ProgramNoSetting1stDigit,
            Reset,
            HomePositionInstruction,
            Start,
            ServoOn,
            Command10,
            Command11,
            ForcedStop,
            BrakeRelease,
            JogOperationCW,
            JogOperationCCW,
            Unavailable16,
            Unavailable17,
            Unavailable18,
            TableOperation,
            Unavailable20,
            Unavailable21,
            Unavailable22,
            Unavailable23,
            Unavailable24,
            Unavailable25,
            Unavailable26,
            Unavailable27,
            Unavailable28,
            Unavailable29,
            Unavailable30,
            Unavailable31,
        }

        public enum OutputSignal2Mapping
        {
            MonitorOutputExecutionRequest,
            InstructionCodeExecutionRequest,
            Unavailable2,
            Unavailable3,
            Unavailable4,
            Unavailable5,
            Unavailable6,
            Unavailable7,
            Unavailable8,
            Unavailable9,
            Unavailable10,
            Unavailable11,
            Unavailable12,
            Unavailable13,
            Unavailable14,
            Unavailable15,
            Unavailable16,
            Unavailable17,
            Unavailable18,
            Unavailable19,
            Unavailable20,
            Unavailable21,
            Unavailable22,
            Unavailable23,
            Unavailable24,
            Unavailable25,
            Unavailable26,
            Unavailable27,
            Unavailable28,
            Unavailable29,
            Unavailable30,
            Unavailable31,
        }

        public enum InputSignal1Mapping
        {
            MCode0,
            MCode1,
            MCode2,
            MCode3,
            MCode4,
            MCode5,
            MCode6,
            MCode7,
            InPosition,
            PositionCompletion,
            StartInputWait,
            Alarm1,
            Alarm2,
            HomePosition,
            ServoState,
            Ready,
            SegmentPositionStrobe,
            MCodeStrobe,
            Unavailable18,
            Unavailable19,
            Unavailable20,
            Unavailable21,
            Unavailable22,
            Unavailable23,
            Unavailable24,
            Unavailable25,
            Unavailable26,
            Unavailable27,
            Unavailable28,
            Unavailable29,
            Unavailable30,
            Unavailable31,
        }

        public enum InputSignal2Mapping
        {
            Monitoring,
            InstructionCodeExecutionCompletion,
            Unavailable2,
            Unavailable3,
            Unavailable4,
            Unavailable5,
            Unavailable6,
            Unavailable7,
            Unavailable8,
            Unavailable9,
            Unavailable10,
            Unavailable11,
            Unavailable12,
            Unavailable13,
            Unavailable14,
            Unavailable15,
            Unavailable16,
            Unavailable17,
            Unavailable18,
            Unavailable19,
            Unavailable20,
            Unavailable21,
            Unavailable22,
            Unavailable23,
            Unavailable24,
            Unavailable25,
            Unavailable26,
            Unavailable27,
            Unavailable28,
            Unavailable29,
            Unavailable30,
            Unavailable31,
        }

        public struct SignalMappingPos
        {
            public int ByteIndex;
            public int BitIndex;

            public SignalMappingPos(int byteIndex, int bitIndex)
            {
                ByteIndex = byteIndex;
                BitIndex = bitIndex;
            }
        }

        public struct TxPdoInputData
        {
            public byte[] InputSignal1;
            public byte[] InputSignal2;
            public byte[] InputData1;
            public byte[] InputData2;
            public byte[] InputData3;
            public byte[] InputData4;
            public byte[] InputData5;
            public byte[] InputCommand1;
            public byte[] InputCommand2;
            public byte[] InputCommand3;

            public TxPdoInputData(int byteSize)
            {
                InputSignal1 = new byte[byteSize];
                InputSignal2 = new byte[byteSize];
                InputData1 = new byte[byteSize];
                InputData2 = new byte[byteSize];
                InputData3 = new byte[byteSize];
                InputData4 = new byte[byteSize];
                InputData5 = new byte[byteSize];
                InputCommand1 = new byte[byteSize];
                InputCommand2 = new byte[byteSize];
                InputCommand3 = new byte[byteSize];
            }

            public void Clear()
            {
                Array.Clear(InputSignal1, 0, InputSignal1.Length);
                Array.Clear(InputSignal2, 0, InputSignal2.Length);
                Array.Clear(InputData1, 0, InputData1.Length);
                Array.Clear(InputData2, 0, InputData2.Length);
                Array.Clear(InputData3, 0, InputData3.Length);
                Array.Clear(InputData4, 0, InputData4.Length);
                Array.Clear(InputData5, 0, InputData5.Length);
                Array.Clear(InputCommand1, 0, InputCommand1.Length);
                Array.Clear(InputCommand2, 0, InputCommand2.Length);
                Array.Clear(InputCommand3, 0, InputCommand3.Length);
            }
        }

        public struct RxPdoOutputData
        {
            public byte[] OutputSignal1;
            public byte[] OutputSignal2;
            public byte[] OutputData1;
            public byte[] OutputData2;
            public byte[] OutputData3;
            public byte[] OutputData4;
            public byte[] OutputData5;
            public byte[] OutputCommand1;
            public byte[] OutputCommand2;
            public byte[] OutputCommand3;

            public RxPdoOutputData(int byteSize)
            {
                OutputSignal1 = new byte[byteSize];
                OutputSignal2 = new byte[byteSize];
                OutputData1 = new byte[byteSize];
                OutputData2 = new byte[byteSize];
                OutputData3 = new byte[byteSize];
                OutputData4 = new byte[byteSize];
                OutputData5 = new byte[byteSize];
                OutputCommand1 = new byte[byteSize];
                OutputCommand2 = new byte[byteSize];
                OutputCommand3 = new byte[byteSize];
            }

            public void Clear()
            {
                Array.Clear(OutputSignal1, 0, OutputSignal1.Length);
                Array.Clear(OutputSignal2, 0, OutputSignal2.Length);
                Array.Clear(OutputData1, 0, OutputData1.Length);
                Array.Clear(OutputData2, 0, OutputData2.Length);
                Array.Clear(OutputData3, 0, OutputData3.Length);
                Array.Clear(OutputData4, 0, OutputData4.Length);
                Array.Clear(OutputData5, 0, OutputData5.Length);
                Array.Clear(OutputCommand1, 0, OutputCommand1.Length);
                Array.Clear(OutputCommand2, 0, OutputCommand2.Length);
                Array.Clear(OutputCommand3, 0, OutputCommand3.Length);
            }
        }

        // PDO Mapping
        private Dictionary<PDOProcessImage, uint> bitOffset = new Dictionary<PDOProcessImage, uint>
        {
            // TxPDO (Input) : Master <- Driver
            { PDOProcessImage.TxPdoInputSignal1,    StartBitOffset + 32 * 0 },
            { PDOProcessImage.TxPdoInputSignal2,    StartBitOffset + 32 * 1 },
            { PDOProcessImage.TxPdoInputData1,      StartBitOffset + 32 * 2 },
            { PDOProcessImage.TxPdoInputData2,      StartBitOffset + 32 * 3 },
            { PDOProcessImage.TxPdoInputData3,      StartBitOffset + 32 * 4 },
            { PDOProcessImage.TxPdoInputData4,      StartBitOffset + 32 * 5 },
            { PDOProcessImage.TxPdoInputData5,      StartBitOffset + 32 * 6 },
            { PDOProcessImage.TxPdoInputCommand1,   StartBitOffset + 32 * 7 },
            { PDOProcessImage.TxPdoInputCommand2,   StartBitOffset + 32 * 8 },
            { PDOProcessImage.TxPdoInputCommand3,   StartBitOffset + 32 * 9 },

            // RxPDO (Output) : Master -> Driver
            { PDOProcessImage.RxPdoOutputSignal1,   StartBitOffset + 32 * 0 },
            { PDOProcessImage.RxPdoOutputSignal2,   StartBitOffset + 32 * 1 },
            { PDOProcessImage.RxPdoOutputData1,     StartBitOffset + 32 * 2 },
            { PDOProcessImage.RxPdoOutputData2,     StartBitOffset + 32 * 3 },
            { PDOProcessImage.RxPdoOutputData3,     StartBitOffset + 32 * 4 },
            { PDOProcessImage.RxPdoOutputData4,     StartBitOffset + 32 * 5 },
            { PDOProcessImage.RxPdoOutputData5,     StartBitOffset + 32 * 6 },
            { PDOProcessImage.RxPdoOutputCommand1,  StartBitOffset + 32 * 7 },
            { PDOProcessImage.RxPdoOutputCommand2,  StartBitOffset + 32 * 8 },
            { PDOProcessImage.RxPdoOutputCommand3,  StartBitOffset + 32 * 9 },
        };
        private Dictionary<OutputSignal1Mapping, SignalMappingPos> outputSignal1MappingPos = new Dictionary<OutputSignal1Mapping, SignalMappingPos>
        {
            { OutputSignal1Mapping.ProgramNo1,                 new SignalMappingPos(0, 0) },
            { OutputSignal1Mapping.programNo2,                 new SignalMappingPos(0, 1) },
            { OutputSignal1Mapping.ProgramNo3,                 new SignalMappingPos(0, 2) },
            { OutputSignal1Mapping.ProgramNo4,                 new SignalMappingPos(0, 3) },
            { OutputSignal1Mapping.ProgramNoSetting2ndDigit,   new SignalMappingPos(0, 4) },
            { OutputSignal1Mapping.ProgramNoSetting1stDigit,   new SignalMappingPos(0, 5) },
            { OutputSignal1Mapping.Reset,                      new SignalMappingPos(0, 6) },
            { OutputSignal1Mapping.HomePositionInstruction,    new SignalMappingPos(0, 7) },
            { OutputSignal1Mapping.Start,                      new SignalMappingPos(1, 0) },
            { OutputSignal1Mapping.ServoOn,                    new SignalMappingPos(1, 1) },
            { OutputSignal1Mapping.Command10,                  new SignalMappingPos(1, 2) },
            { OutputSignal1Mapping.Command11,                  new SignalMappingPos(1, 3) },
            { OutputSignal1Mapping.ForcedStop,                 new SignalMappingPos(1, 4) },
            { OutputSignal1Mapping.BrakeRelease,               new SignalMappingPos(1, 5) },
            { OutputSignal1Mapping.JogOperationCW,             new SignalMappingPos(1, 6) },
            { OutputSignal1Mapping.JogOperationCCW,            new SignalMappingPos(1, 7) },
            { OutputSignal1Mapping.Unavailable16,              new SignalMappingPos(2, 0) },
            { OutputSignal1Mapping.Unavailable17,              new SignalMappingPos(2, 1) },
            { OutputSignal1Mapping.Unavailable18,              new SignalMappingPos(2, 2) },
            { OutputSignal1Mapping.TableOperation,             new SignalMappingPos(2, 3) },
            { OutputSignal1Mapping.Unavailable20,              new SignalMappingPos(2, 4) },
            { OutputSignal1Mapping.Unavailable21,              new SignalMappingPos(2, 5) },
            { OutputSignal1Mapping.Unavailable22,              new SignalMappingPos(2, 6) },
            { OutputSignal1Mapping.Unavailable23,              new SignalMappingPos(2, 7) },
            { OutputSignal1Mapping.Unavailable24,              new SignalMappingPos(3, 0) },
            { OutputSignal1Mapping.Unavailable25,              new SignalMappingPos(3, 1) },
            { OutputSignal1Mapping.Unavailable26,              new SignalMappingPos(3, 2) },
            { OutputSignal1Mapping.Unavailable27,              new SignalMappingPos(3, 3) },
            { OutputSignal1Mapping.Unavailable28,              new SignalMappingPos(3, 4) },
            { OutputSignal1Mapping.Unavailable29,              new SignalMappingPos(3, 5) },
            { OutputSignal1Mapping.Unavailable30,              new SignalMappingPos(3, 6) },
            { OutputSignal1Mapping.Unavailable31,              new SignalMappingPos(3, 7) },
        };
        private Dictionary<OutputSignal2Mapping, SignalMappingPos> outputSignal2MappingPos = new Dictionary<OutputSignal2Mapping, SignalMappingPos>
        {
            { OutputSignal2Mapping.MonitorOutputExecutionRequest,      new SignalMappingPos(0, 0) },
            { OutputSignal2Mapping.InstructionCodeExecutionRequest,    new SignalMappingPos(0, 1) },
            { OutputSignal2Mapping.Unavailable2,                       new SignalMappingPos(0, 2) },
            { OutputSignal2Mapping.Unavailable3,                       new SignalMappingPos(0, 3) },
            { OutputSignal2Mapping.Unavailable4,                       new SignalMappingPos(0, 4) },
            { OutputSignal2Mapping.Unavailable5,                       new SignalMappingPos(0, 5) },
            { OutputSignal2Mapping.Unavailable6,                       new SignalMappingPos(0, 6) },
            { OutputSignal2Mapping.Unavailable7,                       new SignalMappingPos(0, 7) },
            { OutputSignal2Mapping.Unavailable8,                       new SignalMappingPos(1, 0) },
            { OutputSignal2Mapping.Unavailable9,                       new SignalMappingPos(1, 1) },
            { OutputSignal2Mapping.Unavailable10,                      new SignalMappingPos(1, 2) },
            { OutputSignal2Mapping.Unavailable11,                      new SignalMappingPos(1, 3) },
            { OutputSignal2Mapping.Unavailable12,                      new SignalMappingPos(1, 4) },
            { OutputSignal2Mapping.Unavailable13,                      new SignalMappingPos(1, 5) },
            { OutputSignal2Mapping.Unavailable14,                      new SignalMappingPos(1, 6) },
            { OutputSignal2Mapping.Unavailable15,                      new SignalMappingPos(1, 7) },
            { OutputSignal2Mapping.Unavailable16,                      new SignalMappingPos(2, 0) },
            { OutputSignal2Mapping.Unavailable17,                      new SignalMappingPos(2, 1) },
            { OutputSignal2Mapping.Unavailable18,                      new SignalMappingPos(2, 2) },
            { OutputSignal2Mapping.Unavailable19,                      new SignalMappingPos(2, 3) },
            { OutputSignal2Mapping.Unavailable20,                      new SignalMappingPos(2, 4) },
            { OutputSignal2Mapping.Unavailable21,                      new SignalMappingPos(2, 5) },
            { OutputSignal2Mapping.Unavailable22,                      new SignalMappingPos(2, 6) },
            { OutputSignal2Mapping.Unavailable23,                      new SignalMappingPos(2, 7) },
            { OutputSignal2Mapping.Unavailable24,                      new SignalMappingPos(3, 0) },
            { OutputSignal2Mapping.Unavailable25,                      new SignalMappingPos(3, 1) },
            { OutputSignal2Mapping.Unavailable26,                      new SignalMappingPos(3, 2) },
            { OutputSignal2Mapping.Unavailable27,                      new SignalMappingPos(3, 3) },
            { OutputSignal2Mapping.Unavailable28,                      new SignalMappingPos(3, 4) },
            { OutputSignal2Mapping.Unavailable29,                      new SignalMappingPos(3, 5) },
            { OutputSignal2Mapping.Unavailable30,                      new SignalMappingPos(3, 6) },
            { OutputSignal2Mapping.Unavailable31,                      new SignalMappingPos(3, 7) },
        };
        private Dictionary<InputSignal1Mapping, SignalMappingPos> inputSignal1MappingPos = new Dictionary<InputSignal1Mapping, SignalMappingPos>
        {
            { InputSignal1Mapping.MCode0,                  new SignalMappingPos(0, 0) },
            { InputSignal1Mapping.MCode1,                  new SignalMappingPos(0, 1) },
            { InputSignal1Mapping.MCode2,                  new SignalMappingPos(0, 2) },
            { InputSignal1Mapping.MCode3,                  new SignalMappingPos(0, 3) },
            { InputSignal1Mapping.MCode4,                  new SignalMappingPos(0, 4) },
            { InputSignal1Mapping.MCode5,                  new SignalMappingPos(0, 5) },
            { InputSignal1Mapping.MCode6,                  new SignalMappingPos(0, 6) },
            { InputSignal1Mapping.MCode7,                  new SignalMappingPos(0, 7) },
            { InputSignal1Mapping.InPosition,              new SignalMappingPos(1, 0) },
            { InputSignal1Mapping.PositionCompletion,      new SignalMappingPos(1, 1) },
            { InputSignal1Mapping.StartInputWait,          new SignalMappingPos(1, 2) },
            { InputSignal1Mapping.Alarm1,                  new SignalMappingPos(1, 3) },
            { InputSignal1Mapping.Alarm2,                  new SignalMappingPos(1, 4) },
            { InputSignal1Mapping.HomePosition,            new SignalMappingPos(1, 5) },
            { InputSignal1Mapping.ServoState,              new SignalMappingPos(1, 6) },
            { InputSignal1Mapping.Ready,                   new SignalMappingPos(1, 7) },
            { InputSignal1Mapping.SegmentPositionStrobe,   new SignalMappingPos(2, 0) },
            { InputSignal1Mapping.MCodeStrobe,             new SignalMappingPos(2, 1) },
            { InputSignal1Mapping.Unavailable18,           new SignalMappingPos(2, 2) },
            { InputSignal1Mapping.Unavailable19,           new SignalMappingPos(2, 3) },
            { InputSignal1Mapping.Unavailable20,           new SignalMappingPos(2, 4) },
            { InputSignal1Mapping.Unavailable21,           new SignalMappingPos(2, 5) },
            { InputSignal1Mapping.Unavailable22,           new SignalMappingPos(2, 6) },
            { InputSignal1Mapping.Unavailable23,           new SignalMappingPos(2, 7) },
            { InputSignal1Mapping.Unavailable24,           new SignalMappingPos(3, 0) },
            { InputSignal1Mapping.Unavailable25,           new SignalMappingPos(3, 1) },
            { InputSignal1Mapping.Unavailable26,           new SignalMappingPos(3, 2) },
            { InputSignal1Mapping.Unavailable27,           new SignalMappingPos(3, 3) },
            { InputSignal1Mapping.Unavailable28,           new SignalMappingPos(3, 4) },
            { InputSignal1Mapping.Unavailable29,           new SignalMappingPos(3, 5) },
            { InputSignal1Mapping.Unavailable30,           new SignalMappingPos(3, 6) },
            { InputSignal1Mapping.Unavailable31,           new SignalMappingPos(3, 7) },
        };
        private Dictionary<InputSignal2Mapping, SignalMappingPos> inputSignal2MappingPos = new Dictionary<InputSignal2Mapping, SignalMappingPos>
        {
            { InputSignal2Mapping.Monitoring,                          new SignalMappingPos(0, 0) },
            { InputSignal2Mapping.InstructionCodeExecutionCompletion,  new SignalMappingPos(0, 1) },
            { InputSignal2Mapping.Unavailable2,                        new SignalMappingPos(0, 2) },
            { InputSignal2Mapping.Unavailable3,                        new SignalMappingPos(0, 3) },
            { InputSignal2Mapping.Unavailable4,                        new SignalMappingPos(0, 4) },
            { InputSignal2Mapping.Unavailable5,                        new SignalMappingPos(0, 5) },
            { InputSignal2Mapping.Unavailable6,                        new SignalMappingPos(0, 6) },
            { InputSignal2Mapping.Unavailable7,                        new SignalMappingPos(0, 7) },
            { InputSignal2Mapping.Unavailable8,                        new SignalMappingPos(1, 0) },
            { InputSignal2Mapping.Unavailable9,                        new SignalMappingPos(1, 1) },
            { InputSignal2Mapping.Unavailable10,                       new SignalMappingPos(1, 2) },
            { InputSignal2Mapping.Unavailable11,                       new SignalMappingPos(1, 3) },
            { InputSignal2Mapping.Unavailable12,                       new SignalMappingPos(1, 4) },
            { InputSignal2Mapping.Unavailable13,                       new SignalMappingPos(1, 5) },
            { InputSignal2Mapping.Unavailable14,                       new SignalMappingPos(1, 6) },
            { InputSignal2Mapping.Unavailable15,                       new SignalMappingPos(1, 7) },
            { InputSignal2Mapping.Unavailable16,                       new SignalMappingPos(2, 0) },
            { InputSignal2Mapping.Unavailable17,                       new SignalMappingPos(2, 1) },
            { InputSignal2Mapping.Unavailable18,                       new SignalMappingPos(2, 2) },
            { InputSignal2Mapping.Unavailable19,                       new SignalMappingPos(2, 3) },
            { InputSignal2Mapping.Unavailable20,                       new SignalMappingPos(2, 4) },
            { InputSignal2Mapping.Unavailable21,                       new SignalMappingPos(2, 5) },
            { InputSignal2Mapping.Unavailable22,                       new SignalMappingPos(2, 6) },
            { InputSignal2Mapping.Unavailable23,                       new SignalMappingPos(2, 7) },
            { InputSignal2Mapping.Unavailable24,                       new SignalMappingPos(3, 0) },
            { InputSignal2Mapping.Unavailable25,                       new SignalMappingPos(3, 1) },
            { InputSignal2Mapping.Unavailable26,                       new SignalMappingPos(3, 2) },
            { InputSignal2Mapping.Unavailable27,                       new SignalMappingPos(3, 3) },
            { InputSignal2Mapping.Unavailable28,                       new SignalMappingPos(3, 4) },
            { InputSignal2Mapping.Unavailable29,                       new SignalMappingPos(3, 5) },
            { InputSignal2Mapping.Unavailable30,                       new SignalMappingPos(3, 6) },
            { InputSignal2Mapping.Unavailable31,                       new SignalMappingPos(3, 7) },
        };
        #endregion

        private const int BoardNo = 0;
        private const uint StartBitOffset = 1320;
        private const int PdoWriteDelay = 25; // ms
        private const int ReadPeriod = 20; // ms

        #region Field
        private TxPdoInputData txPdoData;
        private RxPdoOutputData rxPdoData;

        private CancellationTokenSource cts;
        private Task readInputTask;
        private readonly object gate = new object();
        #endregion

        #region Property
        public TxPdoInputData TxPdoData
        {
            get { return txPdoData; }
        }
        public RxPdoOutputData RxPdoData
        {
            get { return rxPdoData; }
        }
        #endregion

        #region Constructor
        public CKDMotorDriver(string name) : base(name)
        {
            txPdoData = new TxPdoInputData(4);
            rxPdoData = new RxPdoOutputData(4);
        }
        public void Dispose()
        {
            StopReadInputDataMonitoring();
        }
        #endregion

        #region Event
        public event EventHandler OnMotorStateUpdated;
        #endregion

        #region Base Component Method
        protected override void InitAlarm()
        {
            base.InitAlarm();
        }
        public override int Initialize()
        {
            int ret = 0;
            if ((ret = ReadAllRxPdoOutputData()) != 0)
                return ret;
  
            return ret;
        }
        #endregion

        #region DD Motor Control
        public int RunProgram(int programNo)
        {
            int ret = 0;
            if ((ret = SendSetProgramNoCommand(programNo)) != 0)
                return ret;
            if ((ret = SendProgramStartCommand()) != 0)
                return ret;
            return ret;
        }
        public int MovePitchCCW_8Div()
        {
            // 1) 프로그램 번호 설정
            int programNo = 0;

            int ret = 0;
            if ((ret = RunProgram(programNo)) != 0)
                return ret;
            return ret;
        }
        public int MovePitchCW_8Div()
        {
            // 1) 프로그램 번호 설정
            int programNo = 1;

            int ret = 0;
            if ((ret = RunProgram(programNo)) != 0)
                return ret;
            return ret;
        }
        public int MovePitchCCW_16Div()
        {
            // 1) 프로그램 번호 설정
            int programNo = 2;

            int ret = 0;
            if ((ret = RunProgram(programNo)) != 0)
                return ret;
            return ret;
        }
        public int MovePitchCW_16Div()
        {
            // 1) 프로그램 번호 설정
            int programNo = 3;

            int ret = 0;
            if ((ret = RunProgram(programNo)) != 0)
                return ret;
            return ret;
        }
        public int MovePitchCCW_32Div()
        {
            // 1) 프로그램 번호 설정
            int programNo = 4;

            int ret = 0;
            if ((ret = RunProgram(programNo)) != 0)
                return ret;
            return ret;
        }
        public int MovePitchCW_32Div()
        {
            // 1) 프로그램 번호 설정
            int programNo = 3;

            int ret = 0;
            if ((ret = RunProgram(programNo)) != 0)
                return ret;
            return ret;
        }
        public int Servo(bool on)
        {
            int ret = 0;
            if ((ret = SendServoOnOffCommand(on)) != 0)
                return ret;
            return ret;
        }
        public int Home()
        {
            int ret = 0;
            if ((ret = SendHomeSearchCommand()) != 0)
                return ret;
            return ret;
        }
        public int AlarmReset()
        {
            int ret = 0;
            if ((ret = SendAlarmResetCommand()) != 0)
                return ret;
            return ret;
        }
        public int EmergencyStop()
        {
            int ret = 0;
            if ((ret = SendForcedStopCommand(true)) != 0)
                return ret;
            return ret;
        }
        public int ClearEmergency()
        {
            int ret = 0;
            if ((ret = SendForcedStopCommand(false)) != 0)
                return ret;
            if ((ret = SendAlarmResetCommand()) != 0)
                return ret;
            return ret;
        }
        public int BrakeRelease(bool release)
        {
            int ret = 0;
            if ((ret = SendBrakeReleaseCommand(release)) != 0)
                return ret;
            return ret;
        }
        #endregion

        #region DD Motor Get Stautus
        public int GetMCode()
        {
            SignalMappingPos[] mappingPos = new SignalMappingPos[8]
            {
                inputSignal1MappingPos[InputSignal1Mapping.MCode0],
                inputSignal1MappingPos[InputSignal1Mapping.MCode1],
                inputSignal1MappingPos[InputSignal1Mapping.MCode2],
                inputSignal1MappingPos[InputSignal1Mapping.MCode3],
                inputSignal1MappingPos[InputSignal1Mapping.MCode4],
                inputSignal1MappingPos[InputSignal1Mapping.MCode5],
                inputSignal1MappingPos[InputSignal1Mapping.MCode6],
                inputSignal1MappingPos[InputSignal1Mapping.MCode7],
            };

            int mcode = 0;
            for (int i = 0; i < mappingPos.Length; i++)
            {
                if (GetBit(txPdoData.InputSignal1[mappingPos[i].ByteIndex], mappingPos[i].BitIndex))
                    mcode |= (1 << i);
            }
            return mcode;
        }
        public bool IsInPosition()
        {
            SignalMappingPos mappingPos = inputSignal1MappingPos[InputSignal1Mapping.InPosition];
            return GetBit(txPdoData.InputSignal1[mappingPos.ByteIndex], mappingPos.BitIndex);
        }
        public bool IsPositionComplete()
        {
            SignalMappingPos mappingPos = inputSignal1MappingPos[InputSignal1Mapping.PositionCompletion];
            return GetBit(txPdoData.InputSignal1[mappingPos.ByteIndex], mappingPos.BitIndex);
        }
        public bool IsRunWait()
        {
            SignalMappingPos mappingPos = inputSignal1MappingPos[InputSignal1Mapping.StartInputWait];
            return GetBit(txPdoData.InputSignal1[mappingPos.ByteIndex], mappingPos.BitIndex);
        }
        public bool IsAlarm()
        {
            SignalMappingPos[] mappingPos = new SignalMappingPos[2]
            {
                inputSignal1MappingPos[InputSignal1Mapping.Alarm1],
                inputSignal1MappingPos[InputSignal1Mapping.Alarm2],
            };

            // 두 알람 비트 중 하나라도 1이면 알람 상태
            for (int i = 0; i < mappingPos.Length; i++)
            {
                if (!GetBit(txPdoData.InputSignal1[mappingPos[i].ByteIndex], mappingPos[i].BitIndex))
                {
                    return true;
                }
            }
            return false;
        }
        public bool IsHomePosition()
        {
            SignalMappingPos mappingPos = inputSignal1MappingPos[InputSignal1Mapping.HomePosition];
            return GetBit(txPdoData.InputSignal1[mappingPos.ByteIndex], mappingPos.BitIndex);
        }
        public bool IsServoOn()
        {
            SignalMappingPos mappingPos = inputSignal1MappingPos[InputSignal1Mapping.ServoState];
            return GetBit(txPdoData.InputSignal1[mappingPos.ByteIndex], mappingPos.BitIndex);
        }
        public bool IsReady()
        {
            SignalMappingPos mappingPos = inputSignal1MappingPos[InputSignal1Mapping.Ready];
            return GetBit(txPdoData.InputSignal1[mappingPos.ByteIndex], mappingPos.BitIndex);
        }
        public int GetPositionDegree()
        {
            int value = BitConverter.ToInt32(txPdoData.InputData1, 0);
            return value;
        }
        public int GetPositionPulse()
        {
            int value = BitConverter.ToInt32(txPdoData.InputData2, 0);
            return value;
        }
        public int GetErrorPulse()
        {
            int value = BitConverter.ToInt32(txPdoData.InputData3, 0);
            return value;
        }
        public int GetVelocity()
        {
            int value = BitConverter.ToInt32(txPdoData.InputData4, 0);
            return value;
        }
        public int GetProgramNo()
        {
            int value = BitConverter.ToInt32(txPdoData.InputData5, 0);
            return value;
        }
        #endregion

        #region Read RxPdo Input Data
        public void StartReadInputDataMonitoring()
        {
            lock (gate)
            {
                if (readInputTask != null && !readInputTask.IsCompleted)
                    return;
                cts = new CancellationTokenSource();
                readInputTask = Task.Run(() => RunReadInputDataMonitoring(cts.Token), cts.Token);
            }
        }
        public void StopReadInputDataMonitoring()
        {
            lock (gate)
            {
                cts?.Cancel();
            }
            try { readInputTask?.Wait(); } catch { /* ignore */ }
        }
        private async Task RunReadInputDataMonitoring(CancellationToken ct)
        {
            var sw = new StopWatch();
            RequestMonitorExecution(true);
            while (!ct.IsCancellationRequested)
            {
                sw.Restart();
                try
                {
                    ReadAllTxPdoInputData();
                    OnMotorStateUpdated?.Invoke(this, EventArgs.Empty);
                }
                catch (Exception ex)
                {
                }

                var wait = ReadPeriod;
                try { await Task.Delay(wait, ct); } catch { /* canceled */ }
            }
            RequestMonitorExecution(false);
        }
        #endregion

        #region Bit Operation
        private void SetBit(ref byte data, int index, bool value)
        {
            if (value)
                data |= (byte)(1 << index);
            else
                data &= (byte)~(1 << index);
        }
        private bool GetBit(byte data, int index)
        {
            return (data & (1 << index)) != 0;
        }
        #endregion

        #region PDO Mapping (Output) [Master > Driver]

        #region Output Signal 1

        private int SendOutputSignal1SetBit(OutputSignal1Mapping signal, bool bit)
        {
            int ret = 0;
            try
            {
                SignalMappingPos mappingPos = outputSignal1MappingPos[signal];
                SetBit(ref rxPdoData.OutputSignal1[mappingPos.ByteIndex], mappingPos.BitIndex, bit);
                if ((ret = AXDEV.ECatWritePdoOutputEx(BoardNo, bitOffset[PDOProcessImage.RxPdoOutputSignal1], 32, rxPdoData.OutputSignal1)) != 0)
                    return ret;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                ret = -1;
            }
            return ret;
        }
        private int SendOutputSignal1BitArray(OutputSignal1Mapping[] signals, bool[] bits)
        {
            if (signals.Length != bits.Length)
                return -1;
            int ret = 0;
            try
            {
                for (int i = 0; i < signals.Length; i++)
                {
                    SignalMappingPos mappingPos = outputSignal1MappingPos[signals[i]];
                    SetBit(ref rxPdoData.OutputSignal1[mappingPos.ByteIndex], mappingPos.BitIndex, bits[i]);
                }
                if ((ret = AXDEV.ECatWritePdoOutputEx(BoardNo, bitOffset[PDOProcessImage.RxPdoOutputSignal1], 32, rxPdoData.OutputSignal1)) != 0)
                    return ret;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                ret = -1;
            }
            return ret;
        }
        private int SendOutputSignal1RisingEdge(OutputSignal1Mapping signal)
        {
            int ret = 0;
            try
            {
                SignalMappingPos mappingPos = outputSignal1MappingPos[signal];
                // Set 0
                SetBit(ref rxPdoData.OutputSignal1[mappingPos.ByteIndex], mappingPos.BitIndex, false);
                if ((ret = AXDEV.ECatWritePdoOutputEx(BoardNo, bitOffset[PDOProcessImage.RxPdoOutputSignal1], 32, rxPdoData.OutputSignal1)) != 0)
                    return ret;
                Thread.Sleep(PdoWriteDelay);
                // Set 1
                SetBit(ref rxPdoData.OutputSignal1[mappingPos.ByteIndex], mappingPos.BitIndex, true);
                if ((ret = AXDEV.ECatWritePdoOutputEx(BoardNo, bitOffset[PDOProcessImage.RxPdoOutputSignal1], 32, rxPdoData.OutputSignal1)) != 0)
                    return ret;
                Thread.Sleep(PdoWriteDelay);
                // Bit Reset
                SetBit(ref rxPdoData.OutputSignal1[mappingPos.ByteIndex], mappingPos.BitIndex, false);
                if ((ret = AXDEV.ECatWritePdoOutputEx(BoardNo, bitOffset[PDOProcessImage.RxPdoOutputSignal1], 32, rxPdoData.OutputSignal1)) != 0)
                    return ret;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                ret = -1;
            }
            return ret;
        }

        private int SendSetProgramNoCommand(int programNo)
        {
            if (programNo < 0 || programNo > 99)
                return -1;

            int ret = 0;
            try
            {
                int secondDigit = programNo / 10;
                int firstDigit = programNo % 10;
                bool[] bits = new bool[4];

                OutputSignal1Mapping[] programNoSetPos = new OutputSignal1Mapping[4]
                {
                    OutputSignal1Mapping.ProgramNo1,
                    OutputSignal1Mapping.programNo2,
                    OutputSignal1Mapping.ProgramNo3,
                    OutputSignal1Mapping.ProgramNo4,
                };

                // 2번째 자릿수 데이터 비트를 먼저 설정한다.
                for (int i = 0; i < programNoSetPos.Length; i++)
                    bits[i] = GetBit((byte)secondDigit, i);
                if ((ret = SendOutputSignal1BitArray(programNoSetPos, bits)) != 0)
                    return ret;

                // 2번째 자릿수 설정 신호를 보낸다.
                if ((ret = SendOutputSignal1RisingEdge(OutputSignal1Mapping.ProgramNoSetting2ndDigit)) != 0)
                    return ret;

                // 1번째 자릿수 데이터 비트를 설정한다.
                for (int i = 0; i < programNoSetPos.Length; i++)
                    bits[i] = GetBit((byte)firstDigit, i);
                if ((ret = SendOutputSignal1BitArray(programNoSetPos, bits)) != 0)
                    return ret;

                // 1번째 자릿수 설정 신호를 보낸다.
                if ((ret = SendOutputSignal1RisingEdge(OutputSignal1Mapping.ProgramNoSetting1stDigit)) != 0)
                    return ret;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                ret = -1;
            }
            return ret;
        }
        private int SendAlarmResetCommand()
        {
            int ret = 0;
            try
            {
                if ((ret = SendOutputSignal1RisingEdge(OutputSignal1Mapping.Reset)) != 0)
                    return ret;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                ret = -1;
            }
            return ret;
        }
        private int SendHomeSearchCommand()
        {
            int ret = 0;
            try
            {
                if ((ret = SendOutputSignal1RisingEdge(OutputSignal1Mapping.HomePositionInstruction)) != 0)
                    return ret;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                ret = -1;
            }
            return ret;
        }
        private int SendProgramStartCommand()
        {
            int ret = 0;
            try
            {
                if ((ret = SendOutputSignal1RisingEdge(OutputSignal1Mapping.Start)) != 0)
                    return ret;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                ret = -1;
            }
            return ret;
        }
        private int SendServoOnOffCommand(bool on)
        {
            int ret = 0;
            try
            {
                if ((ret = SendOutputSignal1SetBit(OutputSignal1Mapping.ServoOn, on)) != 0)
                    return ret;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                ret = -1;
            }
            return ret;
        }
        private int SendForcedStopCommand(bool on)
        {
            int ret = 0;
            try
            {
                // Negative
                if ((ret = SendOutputSignal1SetBit(OutputSignal1Mapping.ForcedStop, !on)) != 0)
                    return ret;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                ret = -1;
            }
            return ret;
        }
        private int SendBrakeReleaseCommand(bool on)
        {
            int ret = 0;
            try
            {
                if ((ret = SendOutputSignal1SetBit(OutputSignal1Mapping.BrakeRelease, on)) != 0)
                    return ret;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                ret = -1;
            }
            return ret;
        }
        #endregion

        #region Output Signal 2
        private int SendOutputSignal2SetBit(OutputSignal2Mapping signal, bool bit)
        {
            int ret = 0;
            try
            {
                SignalMappingPos mappingPos = outputSignal2MappingPos[signal];
                SetBit(ref rxPdoData.OutputSignal2[mappingPos.ByteIndex], mappingPos.BitIndex, bit);
                if ((ret = AXDEV.ECatWritePdoOutputEx(BoardNo, bitOffset[PDOProcessImage.RxPdoOutputSignal2], 32, rxPdoData.OutputSignal2)) != 0)
                    return ret;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                ret = -1;
            }
            return ret;
        }
        private int SendOutputSignal2BitArray(OutputSignal2Mapping[] signals, bool[] bits)
        {
            if (signals.Length != bits.Length)
                return -1;
            int ret = 0;
            try
            {
                for (int i = 0; i < signals.Length; i++)
                {
                    SignalMappingPos mappingPos = outputSignal2MappingPos[signals[i]];
                    SetBit(ref rxPdoData.OutputSignal2[mappingPos.ByteIndex], mappingPos.BitIndex, bits[i]);
                }
                if ((ret = AXDEV.ECatWritePdoOutputEx(BoardNo, bitOffset[PDOProcessImage.RxPdoOutputSignal2], 32, rxPdoData.OutputSignal2)) != 0)
                    return ret;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                ret = -1;
            }
            return ret;
        }
        private int SendOutputSignal2RisingEdge(OutputSignal2Mapping signal)
        {
            int ret = 0;
            try
            {
                SignalMappingPos mappingPos = outputSignal2MappingPos[signal];
                // Set 0
                SetBit(ref rxPdoData.OutputSignal2[mappingPos.ByteIndex], mappingPos.BitIndex, false);
                if ((ret = AXDEV.ECatWritePdoOutputEx(BoardNo, bitOffset[PDOProcessImage.RxPdoOutputSignal2], 32, rxPdoData.OutputSignal2)) != 0)
                    return ret;
                Thread.Sleep(PdoWriteDelay);
                // Set 1
                SetBit(ref rxPdoData.OutputSignal2[mappingPos.ByteIndex], mappingPos.BitIndex, true);
                if ((ret = AXDEV.ECatWritePdoOutputEx(BoardNo, bitOffset[PDOProcessImage.RxPdoOutputSignal2], 32, rxPdoData.OutputSignal2)) != 0)
                    return ret;
                Thread.Sleep(PdoWriteDelay);
                // Bit Reset
                SetBit(ref rxPdoData.OutputSignal2[mappingPos.ByteIndex], mappingPos.BitIndex, false);
                if ((ret = AXDEV.ECatWritePdoOutputEx(BoardNo, bitOffset[PDOProcessImage.RxPdoOutputSignal2], 32, rxPdoData.OutputSignal2)) != 0)
                    return ret;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                ret = -1;
            }
            return ret;
        }

        private int RequestMonitorExecution(bool on)
        {
            int ret = 0;
            try
            {
                if ((ret = SendOutputSignal2SetBit(OutputSignal2Mapping.MonitorOutputExecutionRequest, on)) != 0)
                    return ret;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                ret = -1;
            }
            return ret;
        }
        private int RequestInstructionCodeExecution()
        {
            int ret = 0;
            try
            {
                if ((ret = SendOutputSignal2RisingEdge(OutputSignal2Mapping.InstructionCodeExecutionRequest)) != 0)
                    return ret;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                ret = -1;
            }
            return ret;
        }
        #endregion

        #region Output Data
        private int GetOutputMonitorCode1(ref uint code)
        {
            int ret = 0;
            try
            {
                // Read PDO
                ret = AXDEV.ECatReadPdoOutputEx(BoardNo, bitOffset[PDOProcessImage.RxPdoOutputData1], 32, rxPdoData.OutputData1);
                if (ret != 0)
                    return ret;
                // Get Monitor Code
                code = BitConverter.ToUInt32(rxPdoData.OutputData1, 0);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                ret = -1;
            }
            return ret;
        }
        private int GetOutputMonitorCode2(ref uint code)
        {
            int ret = 0;
            try
            {
                // Read PDO
                ret = AXDEV.ECatReadPdoOutputEx(BoardNo, bitOffset[PDOProcessImage.RxPdoOutputData2], 32, rxPdoData.OutputData2);
                if (ret != 0)
                    return ret;
                // Get Monitor Code
                code = BitConverter.ToUInt32(rxPdoData.OutputData2, 0);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                ret = -1;
            }
            return ret;
        }
        private int GetOutputMonitorCode3(ref uint code)
        {
            int ret = 0;
            try
            {
                // Read PDO
                ret = AXDEV.ECatReadPdoOutputEx(BoardNo, bitOffset[PDOProcessImage.RxPdoOutputData3], 32, rxPdoData.OutputData3);
                if (ret != 0)
                    return ret;
                // Get Monitor Code
                code = BitConverter.ToUInt32(rxPdoData.OutputData3, 0);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                ret = -1;
            }
            return ret;
        }
        private int GetOutputMonitorCode4(ref uint code)
        {
            int ret = 0;
            try
            {
                // Read PDO
                ret = AXDEV.ECatReadPdoOutputEx(BoardNo, bitOffset[PDOProcessImage.RxPdoOutputData4], 32, rxPdoData.OutputData4);
                if (ret != 0)
                    return ret;
                // Get Monitor Code
                code = BitConverter.ToUInt32(rxPdoData.OutputData4, 0);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                ret = -1;
            }
            return ret;
        }
        private int GetOutputMonitorCode5(ref uint code)
        {
            int ret = 0;
            try
            {
                // Read PDO
                ret = AXDEV.ECatReadPdoOutputEx(BoardNo, bitOffset[PDOProcessImage.RxPdoOutputData5], 32, rxPdoData.OutputData5);
                if (ret != 0)
                    return ret;
                // Get Monitor Code
                code = BitConverter.ToUInt32(rxPdoData.OutputData5, 0);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                ret = -1;
            }
            return ret;
        }
        #endregion

        #region Output Command
        private int GetOutputCommandCode(ref uint commandCode)
        {
            int ret = 0;
            try
            {
                // Read PDO
                ret = AXDEV.ECatReadPdoOutputEx(BoardNo, bitOffset[PDOProcessImage.RxPdoOutputCommand1], 32, rxPdoData.OutputCommand1);
                if (ret != 0)
                    return ret;
                // Get Command Code
                commandCode = BitConverter.ToUInt32(rxPdoData.OutputCommand1, 0);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                ret = -1;
            }
            return ret;
        }
        #endregion

        private int ReadAllRxPdoOutputData()
        {
            int ret = 0;
            try
            {
                // Read All PDO
                ret = AXDEV.ECatReadPdoOutputEx(BoardNo, bitOffset[PDOProcessImage.RxPdoOutputSignal1], 32, rxPdoData.OutputSignal1);
                if (ret != 0)
                    return ret;
                ret = AXDEV.ECatReadPdoOutputEx(BoardNo, bitOffset[PDOProcessImage.RxPdoOutputSignal2], 32, rxPdoData.OutputSignal2);
                if (ret != 0)
                    return ret;
                ret = AXDEV.ECatReadPdoOutputEx(BoardNo, bitOffset[PDOProcessImage.RxPdoOutputData1], 32, rxPdoData.OutputData1);
                if (ret != 0)
                    return ret;
                ret = AXDEV.ECatReadPdoOutputEx(BoardNo, bitOffset[PDOProcessImage.RxPdoOutputData2], 32, rxPdoData.OutputData2);
                if (ret != 0)
                    return ret;
                ret = AXDEV.ECatReadPdoOutputEx(BoardNo, bitOffset[PDOProcessImage.RxPdoOutputData3], 32, rxPdoData.OutputData3);
                if (ret != 0)
                    return ret;
                ret = AXDEV.ECatReadPdoOutputEx(BoardNo, bitOffset[PDOProcessImage.RxPdoOutputData4], 32, rxPdoData.OutputData4);
                if (ret != 0)
                    return ret;
                ret = AXDEV.ECatReadPdoOutputEx(BoardNo, bitOffset[PDOProcessImage.RxPdoOutputData5], 32, rxPdoData.OutputData5);
                if (ret != 0)
                    return ret;
                ret = AXDEV.ECatReadPdoOutputEx(BoardNo, bitOffset[PDOProcessImage.RxPdoOutputCommand1], 32, rxPdoData.OutputCommand1);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                ret = -1;
            }
            return ret;
        }
        #endregion

        #region PDO Mapping (Input) [Driver > Master]
        private int ReadAllTxPdoInputData()
        {
            int ret = 0;
            try
            {
                // Read All PDO
                ret = AXDEV.ECatReadPdoInputEx(BoardNo, bitOffset[PDOProcessImage.TxPdoInputSignal1], 32, txPdoData.InputSignal1);
                if (ret != 0)
                    return ret;
                ret = AXDEV.ECatReadPdoInputEx(BoardNo, bitOffset[PDOProcessImage.TxPdoInputSignal2], 32, txPdoData.InputSignal2);
                if (ret != 0)
                    return ret;
                ret = AXDEV.ECatReadPdoInputEx(BoardNo, bitOffset[PDOProcessImage.TxPdoInputData1], 32, txPdoData.InputData1);
                if (ret != 0)
                    return ret;
                ret = AXDEV.ECatReadPdoInputEx(BoardNo, bitOffset[PDOProcessImage.TxPdoInputData2], 32, txPdoData.InputData2);
                if (ret != 0)
                    return ret;
                ret = AXDEV.ECatReadPdoInputEx(BoardNo, bitOffset[PDOProcessImage.TxPdoInputData3], 32, txPdoData.InputData3);
                if (ret != 0)
                    return ret;
                ret = AXDEV.ECatReadPdoInputEx(BoardNo, bitOffset[PDOProcessImage.TxPdoInputData4], 32, txPdoData.InputData4);
                if (ret != 0)
                    return ret;
                ret = AXDEV.ECatReadPdoInputEx(BoardNo, bitOffset[PDOProcessImage.TxPdoInputData5], 32, txPdoData.InputData5);
                if (ret != 0)
                    return ret;
                ret = AXDEV.ECatReadPdoInputEx(BoardNo, bitOffset[PDOProcessImage.TxPdoInputCommand1], 32, txPdoData.InputCommand1);
            }
            catch (Exception ex)
            {
                //Log.Write(ex);
                ret = -1;
            }
            return ret;
        }
        #endregion
    }
}
