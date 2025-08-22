using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace QMC.Common.CustomControl
{
    public enum VerticalAlignment
    {
        Top,
        Middle,
        Bottom
    }

    [ToolboxItem(true)]
    public class VerticalAlignTextBox : TextBox
    {
        private const int EM_SETRECT = 0x00B3;

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left, Top, Right, Bottom;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, ref RECT lParam);

        private VerticalAlignment _verticalAlignment = VerticalAlignment.Middle;

        // 페인트 후 지연 업데이트 중복 방지 플래그
        private bool _pendingUpdateFormatRect;

        [Category("Appearance")]
        [DefaultValue(VerticalAlignment.Middle)]
        public VerticalAlignment VerticalContentAlignment
        {
            get => _verticalAlignment;
            set
            {
                if (_verticalAlignment != value)
                {
                    _verticalAlignment = value;
                    SafeUpdateFormatRect();
                    // 필요 시 다음 페인트에서 반영되며, 추가 Invalidate는 불필요
                }
            }
        }

        private bool IsInDesigner
        {
            get
            {
                try
                {
                    if (LicenseManager.UsageMode == LicenseUsageMode.Designtime) return true;
                    if (Site?.DesignMode == true) return true;
                    if (DesignMode) return true;
                    var p = Process.GetCurrentProcess();
                    if (p.ProcessName?.IndexOf("devenv", StringComparison.OrdinalIgnoreCase) >= 0) return true;
                }
                catch { }
                return false;
            }
        }

        public VerticalAlignTextBox()
        {
            try
            {
                Multiline = true;          // 세로 정렬을 위해 필요
                BorderStyle = BorderStyle.FixedSingle;
                WordWrap = false;
                ScrollBars = ScrollBars.None;
            }
            catch { /* 디자이너 안전 */ }
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            SafeUpdateFormatRect();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            SafeUpdateFormatRect();
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            SafeUpdateFormatRect();
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            SafeUpdateFormatRect();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            SafeUpdateFormatRect();
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            SafeUpdateFormatRect();
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            // WM_PAINT 이후 지연 호출로 포맷 사각형을 갱신(재귀 페인트 방지)
            if (m.Msg == 0x000F /* WM_PAINT */)
            {
                if (!_pendingUpdateFormatRect)
                {
                    _pendingUpdateFormatRect = true;
                    BeginInvoke((Action)(() =>
                    {
                        _pendingUpdateFormatRect = false;
                        SafeUpdateFormatRect();
                    }));
                }
            }
        }

        private void SafeUpdateFormatRect()
        {
            // 디자이너에선 아무 것도 하지 않음
            if (IsInDesigner) return;

            if (!IsHandleCreated) return;
            if (ClientSize.Width <= 1 || ClientSize.Height <= 1) return;

            try
            {
                int lineHeight = Math.Max(1, TextRenderer.MeasureText("A", Font).Height);
                int top;
                switch (_verticalAlignment)
                {
                    case VerticalAlignment.Top:
                        top = 0;
                        break;
                    case VerticalAlignment.Bottom:
                        top = Math.Max(0, ClientSize.Height - lineHeight - 1);
                        break;
                    default:
                        top = Math.Max(0, (ClientSize.Height - lineHeight) / 2);
                        break;
                }

                var rc = new RECT
                {
                    Left = 1,
                    Top = top,
                    Right = Math.Max(1, ClientSize.Width - 1),
                    Bottom = Math.Max(1, top + lineHeight + 2)
                };

                // wParam=0: 즉시 리드로우하지 않음(페인트 루프 방지)
                SendMessage(Handle, EM_SETRECT, IntPtr.Zero, ref rc);
                // Invalidate() 제거: WM_PAINT와의 상호작용으로 무한 페인트 루프 유발 가능
            }
            catch
            {
                // 디자인/런타임 환경 차이로 인한 예외는 무시하여 크래시 방지
            }
        }
    }
}