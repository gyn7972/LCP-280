// QMC.Common.Motion.Ajin/HW/AjinAxlBoardHost.cs
using System;
using System.IO;
using System.Text;
// Ajin .NET Wrappers
//using AJINEXTEK.AXL;
//using CAXL = AJINEXTEK.AXL.CAXL;
//using CAXM = AJINEXTEK.AXL.CAXM;

namespace QMC.Common.Motion.Ajin.HW
{
    /// <summary>
    /// AXL 라이브러리의 수명과 모터 파라미터(.mot) 로딩을 관리하는 호스트.
    /// - Open() 1회 호출로 AXL Open + mot 로드
    /// - Close()에서 AXL Close
    /// - 싱글톤으로 쓰거나, DI 컨테이너에서 1개만 생성해서 쓰면 안전
    /// </summary>
    public sealed class AjinAxlBoardHost : IDisposable
    {
        public bool IsOpen { get; private set; }
        public string ParameterFilePath { get; private set; }

        private readonly int _axlOpenFlags;

        public AjinAxlBoardHost(string parameterFilePath, int axlOpenFlags = 0x07)
        {
            _axlOpenFlags = axlOpenFlags;
            ParameterFilePath = parameterFilePath;
        }

        /// <summary>
        /// INI를 읽어 레이저 타입에 따라 .mot 파일 경로를 고르는 예시 (선택)
        /// 실제 프로젝트의 ConfigManager/NativeMethods가 있다면 그걸 사용해도 됨.
        /// </summary>
        public static string ResolveMotFileFromIni(string iniPath, string defaultCo2, string defaultUv)
        {
            if (!File.Exists(iniPath)) return defaultCo2;

            var sb = new StringBuilder(256);
            // GetPrivateProfileString 사용부는 프로젝트 유틸을 쓰세요.
            // 여기서는 간단히 텍스트 파싱 예시로 대체합니다.
            var lines = File.ReadAllLines(iniPath);
            bool co2 = true; // 기본 CO2
            for (int i = 0; i < lines.Length; i++)
            {
                var l = lines[i].Trim();
                if (l.StartsWith("Laser_Type", StringComparison.OrdinalIgnoreCase))
                {
                    // Laser_Type=True/False
                    var parts = l.Split(new[] { '=', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2)
                        co2 = !(parts[1].Equals("False", StringComparison.OrdinalIgnoreCase));
                    break;
                }
            }
            return co2 ? defaultCo2 : defaultUv;
        }

        public int Open()
        {
            //if (IsOpen) return 0;
            if (AXL.IsOpened()) return 0;

            //int rc = AXL.Open(_axlOpenFlags);
            int rc = AXL.Open();
            if (rc != 0)
                throw new InvalidOperationException("AxlOpen failed. rc=" + rc);

            if (!string.IsNullOrEmpty(ParameterFilePath) && File.Exists(ParameterFilePath))
            {
                rc = (int)AXM.AxmMotLoadParaAll(ParameterFilePath);
                if (rc != 0)
                {
                    // 파라미터 로드 실패해도 AXL은 열려 있으므로, 필요 시 Close
                    //AXL.Close();
                    //throw new InvalidOperationException("AxmMotLoadParaAll failed. rc=" + rc + ", file=" + ParameterFilePath);
                }
                else
                {
                    IsOpen = false;
                }
            }
            else
            {
                IsOpen = true;
            }

            return 0;
        }

        public void Close()
        {
            if (!IsOpen) return;
            try { AXL.Close(); }
            catch { /* ignore */ }
            IsOpen = false;
        }

        public void Dispose() { Close(); }
    }
}
