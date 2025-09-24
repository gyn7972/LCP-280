using QMC.Common;
using System;
using System.Collections.Generic;
using System.Linq;
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

            


            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var eq = Equipment.Instance;

            EquipmentLocator.Initialize(eq); // 주입
            Equipment.Instance.InitializeEquipment();
            
            Application.Run(new QMC.Common.MainForm()); // 정확한 네임스페이스 지정
        }

        static void exceptionDump(object sender, System.Threading.ThreadExceptionEventArgs args)
        {
            //Exception e = args.Exception;
            //Console.WriteLine("errMsg: " + e.Message);
            //Console.WriteLine("errPos: " + e.TargetSite);

            //덤프 파일 경로 설정(MinidumpHelp.cs 에서도 수정)
            //MinidumpHelp.Minidump.install_self_mini_dump(Application.StartupPath);

            MinidumpHelp.Minidump.install_self_mini_dump();
        }
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            //Exception e = args.Exception;
            //Console.WriteLine("errMsg: " + e.Message);
            //Console.WriteLine("errPos: " + e.TargetSite);

            //덤프 파일 경로 설정(MinidumpHelp.cs 에서도 수정)
            //MinidumpHelp.Minidump.install_self_mini_dump(Application.StartupPath);

            MinidumpHelp.Minidump.install_self_mini_dump();
        }

    }
}
