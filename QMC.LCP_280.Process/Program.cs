using QMC.Common;
using QMC.Common.Vision.Cognex;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QMC.LCP_280.Process
{
    internal static class Program
    {
        /// <summary>
        /// 해당 애플리케이션의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Log.Write("LCP_280", "Program Start--------------------.");

            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(exceptionDump);
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            // 전역 Mutex로 다중 실행 방지 (세션 간 포함)
            const string mutexName = @"Global\LCP-280_App_Mutex";
            bool createdNew;

            using (var mutex = new Mutex(initiallyOwned: true, name: mutexName, createdNew: out createdNew))
            {
                if (!createdNew)
                {
                    MessageBox.Show("이미 프로그램이 실행 중입니다.", "LCP-280", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                try
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);

                    // VisionPro 네이티브/매니지드 경로를 명시적으로 로드
                    VisionProBootstrapper.WarmUp();

                    var eq = Equipment.Instance;
                    EquipmentLocator.Initialize(eq); // 주입
                    Equipment.Instance.InitializeEquipment();

                    Application.Run(new QMC.Common.MainForm()); // 정확한 네임스페이스 지정
                }
                finally
                {
                    // using 블록에서 Mutex Dispose 시 자동 해제됩니다.
                }
            }
        }
        //static void Main()
        //{
        //    Log.Write("LCP_280", "Program Start--------------------.");

        //    Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(exceptionDump);
        //    Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        //    AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

        //    Application.EnableVisualStyles();
        //    Application.SetCompatibleTextRenderingDefault(false);

        //    // VisionPro 네이티브/매니지드 경로를 명시적으로 로드
        //    VisionProBootstrapper.WarmUp();

        //    var eq = Equipment.Instance;
        //    EquipmentLocator.Initialize(eq); // 주입
        //    Equipment.Instance.InitializeEquipment();

        //    Application.Run(new QMC.Common.MainForm()); // 정확한 네임스페이스 지정
        //}

        static void exceptionDump(object sender, System.Threading.ThreadExceptionEventArgs args)
        {
            MinidumpHelp.Minidump.install_self_mini_dump();
        }
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            MinidumpHelp.Minidump.install_self_mini_dump();
        }
        
    }
}
