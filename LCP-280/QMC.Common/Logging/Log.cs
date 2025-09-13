using QMC.Common.Component;
using QMC.Common.Unit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common
{
    public static class Log
    {
        public static void Write(string strClass, string strSource, string strMessage)
        {
            LogManager.Instance.Write(LogLevel.Normal, strClass, strSource, strMessage);
        }
        public static void Write(string strClass, string strOperator, string strSource, string strMessage)
        {
            LogManager.Instance.Write(LogLevel.Normal, strClass, strOperator, strSource, strMessage);
        }
        public static void Write(LogLevel level, string strClass, string strSource, string strMessage)
        {
            LogManager.Instance.Write(level, strClass, strSource, strMessage);
        }
        public static void Write(string strClass, string strMessage)
        {
            LogManager.Instance.Write(LogLevel.Normal, strClass, strMessage);
        }
        public static void Write(BaseComponent component, string strMessage)
        {
            LogManager.Instance.Write(LogLevel.Normal, component.Name, component.Name, strMessage);
        }
        public static void Write(LogLevel level, BaseComponent component, string strMessage)
        {
            LogManager.Instance.Write(level, component.Name, component.Name, strMessage);
        }
        public static void Write(LogLevel level, string strClass, BaseComponent component, string strMessage)
        {
            LogManager.Instance.Write(level, strClass, component.Name, strMessage);
        }
        public static void Write(Exception ex)
        {
            LogManager.Instance.Write(LogLevel.Highest, "ProgramExeption", ex.Source);
            LogManager.Instance.Write(LogLevel.Highest, "ProgramExeption", ex.Message);
            LogManager.Instance.Write(LogLevel.Highest, "ProgramExeption", ex.StackTrace);
            var st = new StackTrace(ex, true);  // true = 파일/줄 정보 포함
            var frame = st.GetFrame(0);         // 예외 발생 지점의 frame

            string fileName = frame?.GetFileName() ?? "UnknownFile";
            int lineNumber = frame?.GetFileLineNumber() ?? 0;
            string methodName = frame?.GetMethod()?.Name ?? "UnknownMethod";

            string log = $"[Exception] {ex.Message}\n"
                       + $"File: {fileName}\n"
                       + $"Line: {lineNumber}\n"
                       + $"Method: {methodName}\n"
                       + $"StackTrace:\n{ex.StackTrace}";
            LogManager.Instance.Write(LogLevel.Highest, "ProgramExeption", log);
        }
        public static void WriteWorkLog(string strMessage)
        {
            LogManager.Instance.WriteWorkLog(strMessage);
        }

        public static void Write(BaseUnit baseUnit, string strMessage)
        {
            LogManager.Instance.Write(LogLevel.Normal, baseUnit.UnitName, baseUnit.UnitName, strMessage);
        }
    }
}
