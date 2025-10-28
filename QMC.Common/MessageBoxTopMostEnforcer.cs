using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace QMC.Common
{
    public static class MessageBoxTopMostEnforcer
    {
        private const int WH_CBT = 5;
        private const int HCBT_CREATEWND = 3;
        private const int HCBT_DESTROYWND = 2;
        private const int HCBT_ACTIVATE = 5;

        private static IntPtr _hook = IntPtr.Zero;
        private static HookProc _proc; // prevent GC
        private static bool _lowerOtherTopMost;
        private static readonly Dictionary<IntPtr, List<Form>> _loweredForms = new Dictionary<IntPtr, List<Form>>();

        public static void Enable(bool lowerOtherTopMostInstead = false)
        {
            if (_hook != IntPtr.Zero) return; // already enabled
            _lowerOtherTopMost = lowerOtherTopMostInstead;
            _proc = HookCallback;
            _hook = SetWindowsHookEx(WH_CBT, _proc, IntPtr.Zero, GetCurrentThreadId());
        }

        public static void Disable()
        {
            if (_hook != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hook);
                _hook = IntPtr.Zero;
                _proc = null;
                _loweredForms.Clear();
            }
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            try
            {
                if (nCode == HCBT_CREATEWND || nCode == HCBT_ACTIVATE)
                {
                    var className = GetWindowClass(wParam);
                    if (className == "#32770") // standard dialog (MessageBox 포함)
                    {
                        if (_lowerOtherTopMost)
                        {
                            LowerOtherTopMost(wParam);
                        }
                        else
                        {
                            // MessageBox 자체를 TopMost로 승격
                            SetWindowPos(wParam, HWND_TOPMOST, 0, 0, 0, 0,
                                SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
                        }
                    }
                }
                else if (nCode == HCBT_DESTROYWND)
                {
                    if (_lowerOtherTopMost && _loweredForms.TryGetValue(wParam, out var forms))
                    {
                        foreach (var f in forms)
                        {
                            if (f != null && !f.IsDisposed) f.TopMost = true;
                        }
                        _loweredForms.Remove(wParam);
                    }
                }
            }
            catch { /* ignore hook exceptions */ }

            return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }

        private static void LowerOtherTopMost(IntPtr dialogHwnd)
        {
            // 보이는 TopMost 폼들을 잠시 내리고 기록
            var list = new List<Form>();
            foreach (Form f in Application.OpenForms)
            {
                if (f != null && f.Visible && f.TopMost)
                {
                    list.Add(f);
                    f.TopMost = false;
                }
            }
            if (list.Count > 0)
                _loweredForms[dialogHwnd] = list;
        }

        private static string GetWindowClass(IntPtr hWnd)
        {
            var sb = new System.Text.StringBuilder(256);
            GetClassName(hWnd, sb, sb.Capacity);
            return sb.ToString();
        }

        // P/Invoke
        private delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int GetClassName(IntPtr hWnd, System.Text.StringBuilder lpClassName, int nMaxCount);

        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_SHOWWINDOW = 0x0040;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
            int X, int Y, int cx, int cy, uint uFlags);
    }
}