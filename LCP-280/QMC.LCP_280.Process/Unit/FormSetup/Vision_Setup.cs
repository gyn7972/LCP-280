using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using QMC.Common;
using QMC.Common.Motions;

namespace QMC.LCP_280.Process.Unit
{
    /// <summary>
    /// Equipment와 연동하여 Config 및 Recipe 관리
    /// </summary>
    public partial class Vision_Setup : Form
    {
        // AxisManager에서 사용하던 키 케이스에 맞춰 소문자 통일
        private const string UNIT_NAME = "unit";

        /// <summary>Equipment 인스턴스 참조</summary>
        private Equipment Equipment => Equipment.Instance;

        // 에디터 컬렉션(좌: configuration / 우: speed)
        //private PropertyCollection _editorPropertiesConfig;
        //private PropertyCollection _editorPropertiesSpeed;

        // 저장 시 빠른 조회용 인덱스: (section,title) → Property
        private Dictionary<(string section, string title), PropertyBase> _configIndex;
        private Dictionary<(string section, string title), PropertyBase> _speedIndex;

        Form_AxisJogPopup _jogPopup = null;

        // === Viewer popup 관리 필드 (추가) ===
        private Form _viewerPopupForm;                 // 모델리스 팝업 폼
        private Control _viewerOriginalParent;         // 원래 부모
        private Rectangle _viewerOriginalBounds;       // 원래 위치/크기
        private bool _viewerPoppedOut;                 // 팝업 여부
        private bool _restoringViewer;                 // 복귀 중 플래그

        public Vision_Setup()
        {
            InitializeComponent();
            SuspendLayout();

            InitializeUI();

            // === 더블클릭 팝업 토글 이벤트 연결 (추가) ===
            if (visionImageViewer != null)
            {
                _viewerOriginalParent = visionImageViewer.Parent;
                _viewerOriginalBounds = new Rectangle(visionImageViewer.Location, visionImageViewer.Size);
                visionImageViewer.DoubleClick -= VisionImageViewer_DoubleClick; // 중복 방지
                visionImageViewer.DoubleClick += VisionImageViewer_DoubleClick;
            }

            _jogPopup = new Form_AxisJogPopup();
            //popupAxisJog.Owner = this;

            ResumeLayout(true);
            Console.WriteLine("DigitalIO_Setup 생성자 완료");
        }

        private void DigitalIO_Setup_Load(object sender, EventArgs e)
        {
            // 필요시 폼 로드시 초기 바인딩 추가
        }

        /// <summary>향후 Unit 초기화가 필요하면 이곳에 작성</summary>
        private void InitializeUnit()
        {
        }

        // =========================
        // Save (설정 저장)
        // =========================

        private void btn_Save_Setup_Cylinder_Click(object sender, EventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                MessageBox.Show($"저장 중 오류: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // =========================
        // 인덱스 빌드 & 조회 유틸
        // =========================

        /// <summary>
        /// TitleOnlyProperty로 섹션을 추적하면서 (section,title) 키로 인덱스 생성
        /// </summary>
        private static Dictionary<(string section, string title), PropertyBase> BuildIndex(PropertyCollection pc)
        {
            var map = new Dictionary<(string section, string title), PropertyBase>(StringTupleComparer.OrdinalIgnoreCase);
            if (pc == null || pc.Count == 0) return map;

            string currentSection = string.Empty;
            foreach (var p in pc)
            {
                if (p == null) continue;

                // 섹션 헤더(TitleOnlyProperty)는 섹션명으로만 사용
                if (p is TitleOnlyProperty)
                {
                    currentSection = GetName(p) ?? string.Empty;
                    continue;
                }

                var title = GetName(p);
                if (string.IsNullOrEmpty(title)) continue;

                var key = (currentSection, title);
                // 뒤에 같은 타이틀이 있어도 최초 1개를 신뢰(중복 방지)
                if (!map.ContainsKey(key))
                    map[key] = p;
            }
            return map;
        }

        private PropertyBase Find(string section, string title)
        {
            if (_configIndex != null && _configIndex.TryGetValue((section ?? string.Empty, title), out var p1))
                return p1;
            return null;
        }

        private PropertyBase FindS(string section, string title)
        {
            if (_speedIndex != null && _speedIndex.TryGetValue((section ?? string.Empty, title), out var p1))
                return p1;
            return null;
        }

        private double GetDouble(string section, string title, double fallback)
            => ReadDouble(Find(section, title), fallback);

        private double GetDoubleS(string section, string title, double fallback)
            => ReadDouble(FindS(section, title), fallback);

        private bool GetBool(string section, string title, bool fallback)
            => ReadBool(Find(section, title), fallback);

        private int GetInt(string section, string title, int fallback)
            => ReadInt(Find(section, title), fallback);

        private int GetIntS(string section, string title, int fallback)
            => ReadInt(FindS(section, title), fallback);

        private static double ReadDouble(PropertyBase p, double fallback)
        {
            if (p == null) return fallback;

            if (p is DoubleProperty dp) return dp.Value;
            if (p is BoolProperty bp) return bp.Value ? 1.0 : 0.0;

            var vProp = p.GetType().GetProperty("Value");
            if (vProp != null)
            {
                var v = vProp.GetValue(p);
                if (v is IConvertible)
                {
                    try { return Convert.ToDouble(v, CultureInfo.InvariantCulture); }
                    catch { }
                }
            }
            return fallback;
        }

        private static bool ReadBool(PropertyBase p, bool fallback)
        {
            if (p == null) return fallback;

            if (p is BoolProperty bp) return bp.Value;

            var vProp = p.GetType().GetProperty("Value");
            if (vProp != null)
            {
                var v = vProp.GetValue(p);
                if (v is bool b) return b;
                if (v is IConvertible)
                {
                    try { return Convert.ToDouble(v, CultureInfo.InvariantCulture) != 0.0; }
                    catch { }
                }
            }
            return fallback;
        }

        private static int ReadInt(PropertyBase p, int fallback)
        {
            if (p == null) return fallback;

            switch (p)
            {
                case IntProperty ip: return ip.Value;
                case LongProperty lp: try { checked { return (int)lp.Value; } } catch { return fallback; }
                case FloatProperty fp: return (int)Math.Round(fp.Value);
                case DoubleProperty dp: return (int)Math.Round(dp.Value);
                case BoolProperty bp: return bp.Value ? 1 : 0;
                case StringProperty sp:
                    if (int.TryParse(sp.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var i)) return i;
                    if (double.TryParse(sp.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var d)) return (int)Math.Round(d);
                    return fallback;
            }

            var vProp = p.GetType().GetProperty("Value");
            if (vProp != null)
            {
                var v = vProp.GetValue(p);
                if (v is int i) return i;
                if (v is long l) { try { checked { return (int)l; } } catch { return fallback; } }
                if (v is float f) return (int)Math.Round(f);
                if (v is double d) return (int)Math.Round(d);
                if (v is bool b) return b ? 1 : 0;
                if (v is string s)
                {
                    if (int.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var i2)) return i2;
                    if (double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var d2)) return (int)Math.Round(d2);
                }
                if (v is IConvertible)
                {
                    try { return Convert.ToInt32(v, CultureInfo.InvariantCulture); } catch { }
                }
            }
            return fallback;
        }

        private static string GetName(PropertyBase p)
        {
            if (p == null) return null;

            // Name 우선, 없으면 Title 시도
            var nameProp = p.GetType().GetProperty("Name");
            var titleProp = p.GetType().GetProperty("Title");
            var key = nameProp?.GetValue(p)?.ToString() ?? titleProp?.GetValue(p)?.ToString();
            return key;
        }

        // 섹션/타이틀 키의 대소문자 무시 튜플 비교자
        private sealed class StringTupleComparer : IEqualityComparer<(string section, string title)>
        {
            public static readonly StringTupleComparer OrdinalIgnoreCase = new StringTupleComparer();
            private readonly StringComparer _cmp = StringComparer.OrdinalIgnoreCase;

            public bool Equals((string section, string title) x, (string section, string title) y)
                => _cmp.Equals(x.section, y.section) && _cmp.Equals(x.title, y.title);

            public int GetHashCode((string section, string title) obj)
                => HashCode.Combine(_cmp.GetHashCode(obj.section ?? string.Empty), _cmp.GetHashCode(obj.title ?? string.Empty));
        }

        private void btn_JogPopup_Click(object sender, EventArgs e)
        {
            ShowOrRestoreJogPopup(this);
        }

        private void ShowOrRestoreJogPopup(IWin32Window owner)
        {
            if (_jogPopup == null || _jogPopup.IsDisposed)
            {
                _jogPopup = new Form_AxisJogPopup();
                _jogPopup.StartPosition = FormStartPosition.CenterParent;
                _jogPopup.ShowInTaskbar = false;
                _jogPopup.FormClosed += (s, _) => { _jogPopup = null; };

                // [선택] 닫기(X)를 눌러도 종료하지 말고 숨기기만 하고 싶으면:
                _jogPopup.FormClosing += (s, ev) =>
                {
                    if (ev.CloseReason == CloseReason.UserClosing) { ev.Cancel = true; _jogPopup.Hide(); }
                };
            }

            // 숨겨져 있으면 다시 보여주기 (Hide() 상태 등)
            if (!_jogPopup.Visible)
                _jogPopup.Show(owner);

            // ★ 최소화되어 있으면 복구
            if (_jogPopup.WindowState == FormWindowState.Minimized)
                _jogPopup.WindowState = FormWindowState.Normal;

            // 맨 앞으로 / 포커스 가져오기 (신뢰도 높이기용 토글 트릭 포함)
            _jogPopup.BringToFront();
            _jogPopup.TopMost = true;   // 잠깐 TopMost로
            _jogPopup.TopMost = false;  // 원복
            _jogPopup.Activate();
            _jogPopup.Focus();
        }

        // === Viewer 더블클릭: 팝업 토글 구현 (추가) ===
        private void VisionImageViewer_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                if (visionImageViewer == null || _restoringViewer) return;
                if (!_viewerPoppedOut) PopOutViewer(); else RestoreViewer();
            }
            catch (Exception ex)
            {
                Log.Write("Vision_Setup", "Viewer popup toggle error: " + ex.Message);
            }
        }

        private void PopOutViewer()
        {
            if (visionImageViewer == null || _viewerPoppedOut) return;
            _viewerOriginalParent = visionImageViewer.Parent;
            _viewerOriginalBounds = new Rectangle(visionImageViewer.Location, visionImageViewer.Size);

            _viewerPopupForm = new Form
            {
                Text = "Camera View",
                FormBorderStyle = FormBorderStyle.SizableToolWindow,
                StartPosition = FormStartPosition.CenterParent,
                ClientSize = new Size(800, 600),
                ShowInTaskbar = false
            };
            _viewerPopupForm.FormClosed += (s, e) =>
            {
                if (!_restoringViewer) RestoreViewer();
            };

            visionImageViewer.SuspendLayout();
            _viewerOriginalParent.Controls.Remove(visionImageViewer);
            visionImageViewer.Dock = DockStyle.Fill;
            _viewerPopupForm.Controls.Add(visionImageViewer);
            visionImageViewer.ResumeLayout();

            _viewerPoppedOut = true;
            _viewerPopupForm.Show(this); // 모델리스
            _viewerPopupForm.BringToFront();
        }

        private void RestoreViewer()
        {
            if (visionImageViewer == null || !_viewerPoppedOut) return;
            _restoringViewer = true;
            try
            {
                if (_viewerPopupForm != null && !_viewerPopupForm.IsDisposed)
                {
                    try { _viewerPopupForm.Controls.Remove(visionImageViewer); } catch { }
                }
                if (_viewerOriginalParent != null && !_viewerOriginalParent.IsDisposed)
                {
                    visionImageViewer.SuspendLayout();
                    visionImageViewer.Dock = DockStyle.None;
                    visionImageViewer.Location = _viewerOriginalBounds.Location;
                    visionImageViewer.Size = _viewerOriginalBounds.Size;
                    _viewerOriginalParent.Controls.Add(visionImageViewer);
                    visionImageViewer.ResumeLayout();
                    visionImageViewer.BringToFront();
                }
                if (_viewerPopupForm != null)
                {
                    try { if (!_viewerPopupForm.IsDisposed) _viewerPopupForm.Close(); } catch { }
                    try { _viewerPopupForm.Dispose(); } catch { }
                }
            }
            finally
            {
                _viewerPopupForm = null;
                _viewerPoppedOut = false;
                _restoringViewer = false;
            }
        }

        /// <summary>
        /// FormConfig 탭 호스트가 전달하는 가용 크기에 맞춰 자동으로 폼 크기를 조정합니다.
        /// </summary>
        /// <param name="width">가용 너비</param>
        /// <param name="height">가용 높이 (탭 헤더 제외)</param>
        public void SetPanelSize(int width, int height)
        {
            try
            {
                this.SuspendLayout();

                // 호스트(TabPage)의 클라이언트 영역을 그대로 사용
                this.Size = new Size(width, height);
                this.ClientSize = new Size(width, height);

                // 포함된 컨트롤들이 Dock=Fill 등으로 배치되었다면 자동으로 맞춰짐
                // 필요 시 내부 루트 컨테이너가 있다면 여기에서 Size/Dock 조정 가능

                this.Invalidate();
                this.Update();
            }
            finally
            {
                this.ResumeLayout(true);
            }

            Console.WriteLine($"📐 {nameof(Vision_Setup)}.SetPanelSize → {width}x{height}");
        }
    }
}
