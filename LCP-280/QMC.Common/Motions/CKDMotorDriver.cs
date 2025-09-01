using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using QMC.Common.Motion.Ajin;

namespace QMC.Common.Motions.CKD
{
    public enum RotaryPosition
    {
        Degree_0,
        Degree_45,
        Degree_90,
        Degree_135,
        Degree_180,
        Degree_225,
        Degree_270,
        Degree_315,
    }

    /// <summary>
    /// CKD DD Motor Driver를 제어하기 위한 클래스입니다.
    /// Ajin EtherCAT Master Board에서 PDO I/O 통신으로 제어합니다.
    /// </summary>
    public class CKDMotorDriver
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

            public string GetInputSignal1String()
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < InputSignal1.Length; i++)
                {
                    sb.Append(Convert.ToString(InputSignal1[i], 2).PadLeft(8, '0'));
                }
                return sb.ToString();
            }
            public string GetInputSignal2String()
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < InputSignal2.Length; i++)
                {
                    sb.Append(Convert.ToString(InputSignal2[i], 2).PadLeft(8, '0'));
                }
                return sb.ToString();
            }
            public string GetInputData1String()
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < InputData1.Length; i++)
                {
                    sb.Append(Convert.ToString(InputData1[i], 2).PadLeft(8, '0'));
                }
                return sb.ToString();
            }
            public string GetInputData2String()
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < InputData2.Length; i++)
                {
                    sb.Append(Convert.ToString(InputData2[i], 2).PadLeft(8, '0'));
                }
                return sb.ToString();
            }
            public string GetInputData3String()
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < InputData3.Length; i++)
                {
                    sb.Append(Convert.ToString(InputData3[i], 2).PadLeft(8, '0'));
                }
                return sb.ToString();
            }
            public string GetInputData4String()
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < InputData4.Length; i++)
                {
                    sb.Append(Convert.ToString(InputData4[i], 2).PadLeft(8, '0'));
                }
                return sb.ToString();
            }
            public string GetInputData5String()
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < InputData5.Length; i++)
                {
                    sb.Append(Convert.ToString(InputData5[i], 2).PadLeft(8, '0'));
                }
                return sb.ToString();
            }
            public string GetInputCommand1String()
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < InputCommand1.Length; i++)
                {
                    sb.Append(Convert.ToString(InputCommand1[i], 2).PadLeft(8, '0'));
                }
                return sb.ToString();
            }
            public string GetInputCommand2String()
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < InputCommand2.Length; i++)
                {
                    sb.Append(Convert.ToString(InputCommand2[i], 2).PadLeft(8, '0'));
                }
                return sb.ToString();
            }
            public string GetInputCommand3String()
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < InputCommand3.Length; i++)
                {
                    sb.Append(Convert.ToString(InputCommand3[i], 2).PadLeft(8, '0'));
                }
                return sb.ToString();
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

            public string GetOutputSignal1String()
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < OutputSignal1.Length; i++)
                {
                    sb.Append(Convert.ToString(OutputSignal1[i], 2).PadLeft(8, '0'));
                }
                return sb.ToString();
            }
            public string GetOutputSignal2String()
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < OutputSignal2.Length; i++)
                {
                    sb.Append(Convert.ToString(OutputSignal2[i], 2).PadLeft(8, '0'));
                }
                return sb.ToString();
            }
            public string GetOutputData1String()
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < OutputData1.Length; i++)
                {
                    sb.Append(Convert.ToString(OutputData1[i], 2).PadLeft(8, '0'));
                }
                return sb.ToString();
            }
            public string GetOutputData2String()
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < OutputData2.Length; i++)
                {
                    sb.Append(Convert.ToString(OutputData2[i], 2).PadLeft(8, '0'));
                }
                return sb.ToString();
            }
            public string GetOutputData3String()
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < OutputData3.Length; i++)
                {
                    sb.Append(Convert.ToString(OutputData3[i], 2).PadLeft(8, '0'));
                }
                return sb.ToString();
            }
            public string GetOutputData4String()
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < OutputData4.Length; i++)
                {
                    sb.Append(Convert.ToString(OutputData4[i], 2).PadLeft(8, '0'));
                }
                return sb.ToString();
            }
            public string GetOutputData5String()
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < OutputData5.Length; i++)
                {
                    sb.Append(Convert.ToString(OutputData5[i], 2).PadLeft(8, '0'));
                }
                return sb.ToString();
            }
            public string GetOutputCommand1String()
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < OutputCommand1.Length; i++)
                {
                    sb.Append(Convert.ToString(OutputCommand1[i], 2).PadLeft(8, '0'));
                }
                return sb.ToString();
            }
            public string GetOutputCommand2String()
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < OutputCommand2.Length; i++)
                {
                    sb.Append(Convert.ToString(OutputCommand2[i], 2).PadLeft(8, '0'));
                }
                return sb.ToString();
            }
            public string GetOutputCommand3String()
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < OutputCommand3.Length; i++)
                {
                    sb.Append(Convert.ToString(OutputCommand3[i], 2).PadLeft(8, '0'));
                }
                return sb.ToString();
            }
        }
        #endregion

        private const int BoardNo = 0;
        private const uint StartBitOffset = 1320;
        private const int PdoWriteDelay = 25; // ms

        #region Field
        private TxPdoInputData txPdoData;
        private RxPdoOutputData rxPdoData;
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
        public CKDMotorDriver()
        {
            txPdoData = new TxPdoInputData(4);
            rxPdoData = new RxPdoOutputData(4);
        }
        #endregion

        public int Home()
        {
            return HomeSearch();
        }

        public bool IsHomeDone()
        {
            bool isHomeDone = false;
            if (GetHomeState(ref isHomeDone) == 0)
                return isHomeDone;
            else
                return false;
        }

        public int MoveAbs(RotaryPosition position)
        {
            // 1) 프로그램 번호 설정
            int programNo = 0;

            int ret = 0;
            if ((ret = RunProgram(programNo)) != 0)
                return ret;
            return ret;
        }

        public int MovePitchCW()
        {
            // 1) 프로그램 번호 설정
            int programNo = 0;

            int ret = 0;
            if ((ret = RunProgram(programNo)) != 0)
                return ret;
            return ret;
        }

        public int MovePitchCCW()
        {
            // 1) 프로그램 번호 설정
            int programNo = 0;

            int ret = 0;
            if ((ret = RunProgram(programNo)) != 0)
                return ret;
            return ret;
        }

        public bool IsMoveDone()
        {
            bool isMoveDone = false;
            if (GetPositionCompletion(ref isMoveDone) == 0)
                return isMoveDone;
            else
                return false;
        }

        public int EmergencyStop()
        {
            int ret = 0;
            if ((ret = ForcedStop(true)) != 0)
                return ret;

            return ret;
        }

        public int ClearEmergency()
        {
            int ret = 0;
            if ((ret = ForcedStop(false)) != 0)
                return ret;
            return ret;
        }

        public int Servo(bool on)
        {
            int ret = 0;
            if ((ret = ServoOnOff(on)) != 0)
                return ret;
            return ret;
        }

        public int ClearAlarm()
        {
            int ret = 0;
            if ((ret = ResetDriver()) != 0)
                return ret;
            return ret;
        }

        private int RunProgram(int programNo)
        {
            int ret = 0;
            if ((ret = SelectProgramNo(programNo)) != 0)
                return ret;
            if ((ret = ApplyProgramNo()) != 0)
                return ret;
            if ((ret = ProgramStart()) != 0)
                return ret;
            return ret;
        }

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
        /// <summary>
        /// 실행할 프로그램 번호를 입력합니다. (0~15)
        /// </summary>
        private int SelectProgramNo(int programNo)
        {
            if (programNo < 0 || programNo > 16)
                return -1;

            int ret = 0;
            try
            {
                // Index
                SignalMappingPos[] mappingPos = new SignalMappingPos[4]
                {
                    outputSignal1MappingPos[OutputSignal1Mapping.ProgramNo1],
                    outputSignal1MappingPos[OutputSignal1Mapping.programNo2],
                    outputSignal1MappingPos[OutputSignal1Mapping.ProgramNo3],
                    outputSignal1MappingPos[OutputSignal1Mapping.ProgramNo4],
                };

                // Set Program Number Bit
                byte data = (byte)(programNo & 0x0f);
                for (int i = 0; i < mappingPos.Length; i ++)
                    SetBit(ref rxPdoData.OutputSignal1[mappingPos[i].ByteIndex], mappingPos[i].BitIndex, GetBit(data, i));
                ret = AXDEV.ECatWritePdoOutputEx(BoardNo, bitOffset[PDOProcessImage.RxPdoOutputData1], 32, rxPdoData.OutputSignal1);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                ret = -1;
            }
            return ret;
        }
        /// <summary>
        /// 설정한 프로그램 번호를 적용합니다.
        /// </summary>
        private int ApplyProgramNo()
        {
            int ret = 0;
            try
            {
                // Index
                SignalMappingPos[] mappingPos = new SignalMappingPos[2]
                {
                    outputSignal1MappingPos[OutputSignal1Mapping.ProgramNoSetting2ndDigit],
                    outputSignal1MappingPos[OutputSignal1Mapping.ProgramNoSetting1stDigit],
                };

                // Set 0
                for (int i = 0; i < mappingPos.Length; i++)
                    SetBit(ref rxPdoData.OutputSignal1[mappingPos[i].ByteIndex], mappingPos[i].BitIndex, false);
                ret = AXDEV.ECatWritePdoOutputEx(BoardNo, bitOffset[PDOProcessImage.RxPdoOutputSignal1], 32, rxPdoData.OutputSignal1);
                Thread.Sleep(PdoWriteDelay);

                // Set 1 (Rising Edge)
                for (int i = 0; i < mappingPos.Length; i++)
                    SetBit(ref rxPdoData.OutputSignal1[mappingPos[i].ByteIndex], mappingPos[i].BitIndex, true);
                ret = AXDEV.ECatWritePdoOutputEx(BoardNo, bitOffset[PDOProcessImage.RxPdoOutputSignal1], 32, rxPdoData.OutputSignal1);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                ret = -1;
            }
            return ret;
        }
        /// <summary>
        /// 드라이버에 리셋을 입력합니다.
        /// </summary>
        private int ResetDriver()
        {
            int ret = 0;
            try
            {
                // Index
                SignalMappingPos[] mappingPos = new SignalMappingPos[1]
                {
                    outputSignal1MappingPos[OutputSignal1Mapping.Reset],
                };

                // Set 0
                for (int i = 0; i < mappingPos.Length; i++)
                    SetBit(ref rxPdoData.OutputSignal1[mappingPos[i].ByteIndex], mappingPos[i].BitIndex, false);
                ret = AXDEV.ECatWritePdoOutputEx(BoardNo, bitOffset[PDOProcessImage.RxPdoOutputSignal1], 32, rxPdoData.OutputSignal1);
                Thread.Sleep(PdoWriteDelay);

                // Set 1 (Rising Edge)
                for (int i = 0; i < mappingPos.Length; i++)
                    SetBit(ref rxPdoData.OutputSignal1[mappingPos[i].ByteIndex], mappingPos[i].BitIndex, true);
                ret = AXDEV.ECatWritePdoOutputEx(BoardNo, bitOffset[PDOProcessImage.RxPdoOutputSignal1], 32, rxPdoData.OutputSignal1);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                ret = -1;
            }
            return ret;
        }
        /// <summary>
        /// 드라이버에 원점 복귀 지령을 입력합니다.
        /// </summary>
        private int HomeSearch()
        {
            int ret = 0;
            try
            {
                // Index
                SignalMappingPos[] mappingPos = new SignalMappingPos[1]
                {
                    outputSignal1MappingPos[OutputSignal1Mapping.HomePositionInstruction],
                };

                // Set 0
                for (int i = 0; i < mappingPos.Length; i++)
                    SetBit(ref rxPdoData.OutputSignal1[mappingPos[i].ByteIndex], mappingPos[i].BitIndex, false);
                ret = AXDEV.ECatWritePdoOutputEx(BoardNo, bitOffset[PDOProcessImage.RxPdoOutputSignal1], 32, rxPdoData.OutputSignal1);
                Thread.Sleep(PdoWriteDelay);

                // Set 1 (Rising Edge)
                for (int i = 0; i < mappingPos.Length; i++)
                    SetBit(ref rxPdoData.OutputSignal1[mappingPos[i].ByteIndex], mappingPos[i].BitIndex, true);
                ret = AXDEV.ECatWritePdoOutputEx(BoardNo, bitOffset[PDOProcessImage.RxPdoOutputSignal1], 32, rxPdoData.OutputSignal1);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                ret = -1;
            }
            return ret;
        }
        /// <summary>
        /// 드라이버에 설정한 프로그램을 실행하는 기동 지령을 입력합니다.
        /// </summary>
        private int ProgramStart()
        {
            int ret = 0;
            try
            {
                // Index
                SignalMappingPos[] mappingPos = new SignalMappingPos[1]
                {
                    outputSignal1MappingPos[OutputSignal1Mapping.Start],
                };

                // Set 0
                for (int i = 0; i < mappingPos.Length; i++)
                    SetBit(ref rxPdoData.OutputSignal1[mappingPos[i].ByteIndex], mappingPos[i].BitIndex, false);
                ret = AXDEV.ECatWritePdoOutputEx(BoardNo, bitOffset[PDOProcessImage.RxPdoOutputSignal1], 32, rxPdoData.OutputSignal1);
                Thread.Sleep(PdoWriteDelay);

                // Set 1 (Rising Edge)
                for (int i = 0; i < mappingPos.Length; i++)
                    SetBit(ref rxPdoData.OutputSignal1[mappingPos[i].ByteIndex], mappingPos[i].BitIndex, true);
                ret = AXDEV.ECatWritePdoOutputEx(BoardNo, bitOffset[PDOProcessImage.RxPdoOutputSignal1], 32, rxPdoData.OutputSignal1);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                ret = -1;
            }
            return ret;
        }
        /// <summary>
        /// 드라이버에 서보 온/오프 지령을 입력합니다.
        /// </summary>
        private int ServoOnOff(bool on)
        {
            int ret = 0;
            try
            {
                // Index
                SignalMappingPos[] mappingPos = new SignalMappingPos[1]
                {
                    outputSignal1MappingPos[OutputSignal1Mapping.ServoOn],
                };

                // Set 0
                for (int i = 0; i < mappingPos.Length; i++)
                    SetBit(ref rxPdoData.OutputSignal1[mappingPos[i].ByteIndex], mappingPos[i].BitIndex, on);
                ret = AXDEV.ECatWritePdoOutputEx(BoardNo, bitOffset[PDOProcessImage.RxPdoOutputSignal1], 32, rxPdoData.OutputSignal1);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                ret = -1;
            }
            return ret;
        }
        /// <summary>
        /// 드라이버에 강제 정지 (Software Emergency Stop) 지령을 입력합니다.
        /// </summary>
        private int ForcedStop(bool on)
        {
            int ret = 0;
            try
            {
                // Index
                SignalMappingPos[] mappingPos = new SignalMappingPos[1]
                {
                    outputSignal1MappingPos[OutputSignal1Mapping.ForcedStop],
                };

                // Set
                for (int i = 0; i < mappingPos.Length; i++)
                    SetBit(ref rxPdoData.OutputSignal1[mappingPos[i].ByteIndex], mappingPos[i].BitIndex, !on);
                ret = AXDEV.ECatWritePdoOutputEx(BoardNo, bitOffset[PDOProcessImage.RxPdoOutputSignal1], 32, rxPdoData.OutputSignal1);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                ret = -1;
            }
            return ret;
        }
        /// <summary>
        /// 드라이버에 브레이크 해제 지령을 입력합니다.
        /// </summary>
        private int BrakeRelease(bool on)
        {
            int ret = 0;
            try
            {
                // Index
                SignalMappingPos[] mappingPos = new SignalMappingPos[1]
                {
                    outputSignal1MappingPos[OutputSignal1Mapping.BrakeRelease],
                };

                // Set
                for (int i = 0; i < mappingPos.Length; i++)
                    SetBit(ref rxPdoData.OutputSignal1[mappingPos[i].ByteIndex], mappingPos[i].BitIndex, on);
                ret = AXDEV.ECatWritePdoOutputEx(BoardNo, bitOffset[PDOProcessImage.RxPdoOutputSignal1], 32, rxPdoData.OutputSignal1);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                ret = -1;
            }
            return ret;
        }
        /// <summary>
        /// 드라이버에 조그 운전 지령을 입력합니다. (CW) [+] 네트워크 운전 모드일 때만 유효
        /// </summary>
        private int JogCW()
        {
            int ret = 0;
            try
            {
                // Index
                SignalMappingPos[] mappingPos = new SignalMappingPos[1]
                {
                    outputSignal1MappingPos[OutputSignal1Mapping.JogOperationCW],
                };

                // Set
                for (int i = 0; i < mappingPos.Length; i++)
                    SetBit(ref rxPdoData.OutputSignal1[mappingPos[i].ByteIndex], mappingPos[i].BitIndex, true);
                ret = AXDEV.ECatWritePdoOutputEx(BoardNo, bitOffset[PDOProcessImage.RxPdoOutputSignal1], 32, rxPdoData.OutputSignal1);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                ret = -1;
            }
            return ret;
        }
        /// <summary>
        /// 드라이버에 조그 운전 지령을 입력합니다. (CCW) [+] 네트워크 운전 모드일 때만 유효
        /// </summary>
        private int JogCCW()
        {
            int ret = 0;
            try
            {
                // Index
                SignalMappingPos[] mappingPos = new SignalMappingPos[1]
                {
                    outputSignal1MappingPos[OutputSignal1Mapping.JogOperationCCW],
                };

                // Set
                for (int i = 0; i < mappingPos.Length; i++)
                    SetBit(ref rxPdoData.OutputSignal1[mappingPos[i].ByteIndex], mappingPos[i].BitIndex, true);
                ret = AXDEV.ECatWritePdoOutputEx(BoardNo, bitOffset[PDOProcessImage.RxPdoOutputSignal1], 32, rxPdoData.OutputSignal1);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                ret = -1;
            }
            return ret;
        }
        /// <summary>
        /// 드라이버에 조그 운전 지령을 해제합니다. [+] 네트워크 운전 모드일 때만 유효
        /// </summary>
        private int JogStop()
        {
            int ret = 0;
            try
            {
                // Index
                SignalMappingPos[] mappingPos = new SignalMappingPos[2]
                {
                    outputSignal1MappingPos[OutputSignal1Mapping.JogOperationCW],
                    outputSignal1MappingPos[OutputSignal1Mapping.JogOperationCCW],
                };
                // Set
                for (int i = 0; i < mappingPos.Length; i++)
                    SetBit(ref rxPdoData.OutputSignal1[mappingPos[i].ByteIndex], mappingPos[i].BitIndex, false);
                ret = AXDEV.ECatWritePdoOutputEx(BoardNo, bitOffset[PDOProcessImage.RxPdoOutputSignal1], 32, rxPdoData.OutputSignal1);
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
        private int RequestMonitorExecution(bool on)
        {
            int ret = 0;
            try
            {
                // Index
                SignalMappingPos[] mappingPos = new SignalMappingPos[1]
                {
                    outputSignal2MappingPos[OutputSignal2Mapping.MonitorOutputExecutionRequest],
                };
                // Set
                for (int i = 0; i < mappingPos.Length; i++)
                    SetBit(ref rxPdoData.OutputSignal2[mappingPos[i].ByteIndex], mappingPos[i].BitIndex, on);
                ret = AXDEV.ECatWritePdoOutputEx(BoardNo, bitOffset[PDOProcessImage.RxPdoOutputSignal2], 32, rxPdoData.OutputSignal2);
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
                // Index
                SignalMappingPos[] mappingPos = new SignalMappingPos[1]
                {
                    outputSignal2MappingPos[OutputSignal2Mapping.InstructionCodeExecutionRequest],
                };
                // Set 0
                for (int i = 0; i < mappingPos.Length; i++)
                    SetBit(ref rxPdoData.OutputSignal2[mappingPos[i].ByteIndex], mappingPos[i].BitIndex, false);
                ret = AXDEV.ECatWritePdoOutputEx(BoardNo, bitOffset[PDOProcessImage.RxPdoOutputSignal2], 32, rxPdoData.OutputSignal2);
                Thread.Sleep(PdoWriteDelay);

                // Set 1 (Rising Edge)
                for (int i = 0; i < mappingPos.Length; i++)
                    SetBit(ref rxPdoData.OutputSignal2[mappingPos[i].ByteIndex], mappingPos[i].BitIndex, true);
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

        #endregion

        #region PDO Mapping (Input) [Driver > Master]

        #region Input Signal 1
        private int GetMCode(ref byte code)
        {
            int ret = 0;
            try
            {
                // Read PDO
                ret = AXDEV.ECatReadPdoInputEx(BoardNo, bitOffset[PDOProcessImage.TxPdoInputSignal1], 32, txPdoData.InputSignal1);
                if (ret != 0)
                    return ret;
                // Index
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
                // Get M-Code
                code = 0;
                for (int i = 0; i < mappingPos.Length; i++)
                    code |= (byte)((GetBit(txPdoData.InputSignal1[mappingPos[i].ByteIndex], mappingPos[i].BitIndex) ? 1 : 0) << i);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                ret = -1;
            }
            return ret;
        }
        private int GetInPosition(ref bool inPosition)
        {
            int ret = 0;
            try
            {
                // Read PDO
                ret = AXDEV.ECatReadPdoInputEx(BoardNo, bitOffset[PDOProcessImage.TxPdoInputSignal1], 32, txPdoData.InputSignal1);
                if (ret != 0)
                    return ret;
                // Index
                SignalMappingPos[] mappingPos = new SignalMappingPos[1]
                {
                    inputSignal1MappingPos[InputSignal1Mapping.InPosition],
                };
                // Get In Position
                inPosition = GetBit(txPdoData.InputSignal1[mappingPos[0].ByteIndex], mappingPos[0].BitIndex);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                ret = -1;
            }
            return ret;
        }
        private int GetPositionCompletion(ref bool positionCompletion)
        {
            int ret = 0;
            try
            {
                // Read PDO
                ret = AXDEV.ECatReadPdoInputEx(BoardNo, bitOffset[PDOProcessImage.TxPdoInputSignal1], 32, txPdoData.InputSignal1);
                if (ret != 0)
                    return ret;
                // Index
                SignalMappingPos[] mappingPos = new SignalMappingPos[1]
                {
                    inputSignal1MappingPos[InputSignal1Mapping.PositionCompletion],
                };
                // Get Position Completion
                positionCompletion = GetBit(txPdoData.InputSignal1[mappingPos[0].ByteIndex], mappingPos[0].BitIndex);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                ret = -1;
            }
            return ret;
        }
        private int GetStartInputWait(ref bool startInputWait)
        {
            int ret = 0;
            try
            {
                // Read PDO
                ret = AXDEV.ECatReadPdoInputEx(BoardNo, bitOffset[PDOProcessImage.TxPdoInputSignal1], 32, txPdoData.InputSignal1);
                if (ret != 0)
                    return ret;
                // Index
                SignalMappingPos[] mappingPos = new SignalMappingPos[1]
                {
                    inputSignal1MappingPos[InputSignal1Mapping.StartInputWait],
                };
                // Get Start Input Wait
                startInputWait = GetBit(txPdoData.InputSignal1[mappingPos[0].ByteIndex], mappingPos[0].BitIndex);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                ret = -1;
            }
            return ret;
        }
        private int GetAlarm(ref bool alarm1, ref bool alarm2)
        {
            int ret = 0;
            try
            {
                // Read PDO
                ret = AXDEV.ECatReadPdoInputEx(BoardNo, bitOffset[PDOProcessImage.TxPdoInputSignal1], 32, txPdoData.InputSignal1);
                if (ret != 0)
                    return ret;
                // Index
                SignalMappingPos[] mappingPos = new SignalMappingPos[2]
                {
                    inputSignal1MappingPos[InputSignal1Mapping.Alarm1],
                    inputSignal1MappingPos[InputSignal1Mapping.Alarm2],
                };
                // Get Alarm
                alarm1 = GetBit(txPdoData.InputSignal1[mappingPos[0].ByteIndex], mappingPos[0].BitIndex);
                alarm2 = GetBit(txPdoData.InputSignal1[mappingPos[1].ByteIndex], mappingPos[1].BitIndex);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                ret = -1;
            }
            return ret;
        }
        private int GetHomeState(ref bool homeState)
        {
            int ret = 0;
            try
            {
                // Read PDO
                ret = AXDEV.ECatReadPdoInputEx(BoardNo, bitOffset[PDOProcessImage.TxPdoInputSignal1], 32, txPdoData.InputSignal1);
                if (ret != 0)
                    return ret;
                // Index
                SignalMappingPos[] mappingPos = new SignalMappingPos[1]
                {
                    inputSignal1MappingPos[InputSignal1Mapping.HomePosition],
                };
                // Get Home Position
                homeState = GetBit(txPdoData.InputSignal1[mappingPos[0].ByteIndex], mappingPos[0].BitIndex);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                ret = -1;
            }
            return ret;
        }
        private int GetServoState(ref bool servoState)
        {
            int ret = 0;
            try
            {
                // Read PDO
                ret = AXDEV.ECatReadPdoInputEx(BoardNo, bitOffset[PDOProcessImage.TxPdoInputSignal1], 32, txPdoData.InputSignal1);
                if (ret != 0)
                    return ret;
                // Index
                SignalMappingPos[] mappingPos = new SignalMappingPos[1]
                {
                    inputSignal1MappingPos[InputSignal1Mapping.ServoState],
                };
                // Get Servo State
                servoState = GetBit(txPdoData.InputSignal1[mappingPos[0].ByteIndex], mappingPos[0].BitIndex);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                ret = -1;
            }
            return ret;
        }
        private int GetReady(ref bool ready)
        {
            int ret = 0;
            try
            {
                // Read PDO
                ret = AXDEV.ECatReadPdoInputEx(BoardNo, bitOffset[PDOProcessImage.TxPdoInputSignal1], 32, txPdoData.InputSignal1);
                if (ret != 0)
                    return ret;
                // Index
                SignalMappingPos[] mappingPos = new SignalMappingPos[1]
                {
                    inputSignal1MappingPos[InputSignal1Mapping.Ready],
                };
                // Get Ready
                ready = GetBit(txPdoData.InputSignal1[mappingPos[0].ByteIndex], mappingPos[0].BitIndex);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                ret = -1;
            }
            return ret;
        }
        private int GetSegmentPositionStrobe(ref bool segmentPositionStrobe)
        {
            int ret = 0;
            try
            {
                // Read PDO
                ret = AXDEV.ECatReadPdoInputEx(BoardNo, bitOffset[PDOProcessImage.TxPdoInputSignal1], 32, txPdoData.InputSignal1);
                if (ret != 0)
                    return ret;
                // Index
                SignalMappingPos[] mappingPos = new SignalMappingPos[1]
                {
                    inputSignal1MappingPos[InputSignal1Mapping.SegmentPositionStrobe],
                };
                // Get Segment Position Strobe
                segmentPositionStrobe = GetBit(txPdoData.InputSignal1[mappingPos[0].ByteIndex], mappingPos[0].BitIndex);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                ret = -1;
            }
            return ret;
        }
        private int GetMCodeStrobe(ref bool mCodeStrobe)
        {
            int ret = 0;
            try
            {
                // Read PDO
                ret = AXDEV.ECatReadPdoInputEx(BoardNo, bitOffset[PDOProcessImage.TxPdoInputSignal1], 32, txPdoData.InputSignal1);
                if (ret != 0)
                    return ret;
                // Index
                SignalMappingPos[] mappingPos = new SignalMappingPos[1]
                {
                    inputSignal1MappingPos[InputSignal1Mapping.MCodeStrobe],
                };
                // Get M-Code Strobe
                mCodeStrobe = GetBit(txPdoData.InputSignal1[mappingPos[0].ByteIndex], mappingPos[0].BitIndex);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                ret = -1;
            }
            return ret;
        }
        #endregion

        #region Input Signal 2
        private int GetMonitoringState(ref bool isMonitoring)
        {
            int ret = 0;
            try
            {
                // Read PDO
                ret = AXDEV.ECatReadPdoInputEx(BoardNo, bitOffset[PDOProcessImage.TxPdoInputSignal2], 32, txPdoData.InputSignal2);
                if (ret != 0)
                    return ret;
                // Index
                SignalMappingPos[] mappingPos = new SignalMappingPos[1]
                {
                    inputSignal2MappingPos[InputSignal2Mapping.Monitoring],
                };
                // Get Monitoring State
                isMonitoring = GetBit(txPdoData.InputSignal2[mappingPos[0].ByteIndex], mappingPos[0].BitIndex);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                ret = -1;
            }
            return ret;
        }
        private int GetInstructionCodeExecutionComplete(ref bool excutionComplete)
        {
            int ret = 0;
            try
            {
                // Read PDO
                ret = AXDEV.ECatReadPdoInputEx(BoardNo, bitOffset[PDOProcessImage.TxPdoInputSignal2], 32, txPdoData.InputSignal2);
                if (ret != 0)
                    return ret;
                // Index
                SignalMappingPos[] mappingPos = new SignalMappingPos[1]
                {
                    inputSignal2MappingPos[InputSignal2Mapping.InstructionCodeExecutionCompletion],
                };
                // Get Instruction Code Execution State
                excutionComplete = GetBit(txPdoData.InputSignal2[mappingPos[0].ByteIndex], mappingPos[0].BitIndex);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                ret = -1;
            }
            return ret;
        }
        #endregion

        #region Input Data
        private int GetInputMonitorCode1(ref uint code)
        {
            int ret = 0;
            try
            {
                // Read PDO
                ret = AXDEV.ECatReadPdoInputEx(BoardNo, bitOffset[PDOProcessImage.TxPdoInputData1], 32, txPdoData.InputData1);
                if (ret != 0)
                    return ret;
                // Get Monitor Code
                code = BitConverter.ToUInt32(txPdoData.InputData1, 0);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                ret = -1;
            }
            return ret;
        }
        private int GetInputMonitorCode2(ref uint code)
        {
            int ret = 0;
            try
            {
                // Read PDO
                ret = AXDEV.ECatReadPdoInputEx(BoardNo, bitOffset[PDOProcessImage.TxPdoInputData2], 32, txPdoData.InputData2);
                if (ret != 0)
                    return ret;
                // Get Monitor Code
                code = BitConverter.ToUInt32(txPdoData.InputData2, 0);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                ret = -1;
            }
            return ret;
        }
        private int GetInputMonitorCode3(ref uint code)
        {
            int ret = 0;
            try
            {
                // Read PDO
                ret = AXDEV.ECatReadPdoInputEx(BoardNo, bitOffset[PDOProcessImage.TxPdoInputData3], 32, txPdoData.InputData3);
                if (ret != 0)
                    return ret;
                // Get Monitor Code
                code = BitConverter.ToUInt32(txPdoData.InputData3, 0);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                ret = -1;
            }
            return ret;
        }
        private int GetInputMonitorCode4(ref uint code)
        {
            int ret = 0;
            try
            {
                // Read PDO
                ret = AXDEV.ECatReadPdoInputEx(BoardNo, bitOffset[PDOProcessImage.TxPdoInputData4], 32, txPdoData.InputData4);
                if (ret != 0)
                    return ret;
                // Get Monitor Code
                code = BitConverter.ToUInt32(txPdoData.InputData4, 0);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                ret = -1;
            }
            return ret;
        }
        private int GetInputMonitorCode5(ref uint code)
        {
            int ret = 0;
            try
            {
                // Read PDO
                ret = AXDEV.ECatReadPdoInputEx(BoardNo, bitOffset[PDOProcessImage.TxPdoInputData5], 32, txPdoData.InputData5);
                if (ret != 0)
                    return ret;
                // Get Monitor Code
                code = BitConverter.ToUInt32(txPdoData.InputData5, 0);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                ret = -1;
            }
            return ret;
        }
        #endregion

        #region Input Command
        private int GetResponseCommandCode(ref uint commandCode)
        {
            int ret = 0;
            try
            {
                // Read PDO
                ret = AXDEV.ECatReadPdoInputEx(BoardNo, bitOffset[PDOProcessImage.TxPdoInputCommand1], 32, txPdoData.InputCommand1);
                if (ret != 0)
                    return ret;
                // Get Command Code
                commandCode = BitConverter.ToUInt32(txPdoData.InputCommand1, 0);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                ret = -1;
            }
            return ret;
        }
        #endregion

        #endregion
    }
}
