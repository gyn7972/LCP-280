// ICylinderIO.cs
using QMC.Common.Motion.Ajin;

public interface IDigitalIO
{
    bool ReadDI(int ch);                // 입력 채널 읽기
    void WriteDO(int ch, bool on);      // 출력 채널 쓰기
}

public sealed class IoPin
{
    public int Channel { get; }
    public bool Invert { get; }         // 활성 레벨이 Low면 true
    public IoPin(int channel, bool invert = false)
    {
        Channel = channel; Invert = invert;
    }
    public bool Read(IDigitalIO io) => Invert ? !io.ReadDI(Channel) : io.ReadDI(Channel);
    public void Write(IDigitalIO io, bool on)
    {
        bool value = Invert ? !on : on;
        io.WriteDO(Channel, value);
    }
}

