using Newtonsoft.Json.Linq;
using QMC.Common;
using QMC.Common.CustomControl;
using QMC.Common.DIO;
using QMC.Common.IO;
using QMC.Common.Motions;
using QMC.LCP_280.Process;
using QMC.LCP_280.Process.Unit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using static System.Windows.Forms.AxHost;

namespace QMC.LCP_280.Process.Unit
{
    /// <summary>
    /// Motion_Setup (refactored: minimal-change, safer wiring, clearer structure)
    /// </summary>
    partial class DigitalIO_Setup
    {
        private sealed class ModuleListItem
        {
            public string Display { get; set; }     // 리스트에 보이는 텍스트
            public DIOModuleSetup Module { get; set; }
            public bool? IsDI { get; set; }         // true=DI, false=DO, null=헤더/구분선

            public override string ToString() => Display;
        }

        // -------- Fields
        private readonly Equipment equipment = Equipment.Instance;

        // UI
        private ListBoxItemsView dioModuleListBoxItemsView;
        private IndividualMenuButton btn_Save_Setup_Ouput;
        private IndividualMenuButton btn_Save_Setup_Input;

        private System.ComponentModel.IContainer components = null;

        // Data
        private PropertyCollection _editorPropertiesConfig;
        private PropertyCollection _editorPropertiesSpeed;

        private IOPropertyCollectionView inputIOPropertyCollectionView;
        private IOPropertyCollectionView outputIOPropertyCollectionView;
        private PropertyCollectionView inputpropertyCollectionView;
        private PropertyCollectionView outputpropertyCollectionView;

        // Timers
        private Timer _axisPosTimer;

        // fields (폼 클래스 내부)
        private DIOUnit _unit;                 // Equipment에서 만든 _unitIO를 참조
        private DioScanService _scan;          // Equipment에서 만든 _dioScan를 참조
        private DIOModuleSetup _selected;      // 현재 선택 모듈
        private string _setupPath;             // Unit.dio.setup.json 경로
                                               // 현재 바인딩된 아이템들(인덱스 매핑용)
        private List<ModuleListItem> _moduleListItems = new List<ModuleListItem>();
        private DIOModuleSetup _lastDiModule;
        private DIOModuleSetup _lastDoModule;

        // -------- Dispose
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_axisPosTimer != null)
                {
                    //_axisPosTimer.Tick -= AxisPosTimer_Tick;
                    //_axisPosTimer.Dispose();
                    //_axisPosTimer = null;
                }

                if (components != null)
                    components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Designer (trimmed & corrected)
        private void InitializeComponent()
        {
            this.btn_Save_Setup_Input = new QMC.Common.IndividualMenuButton();
            this.btn_Save_Setup_Ouput = new QMC.Common.IndividualMenuButton();
            this.dioModuleListBoxItemsView = new QMC.Common.ListBoxItemsView();
            this.inputIOPropertyCollectionView = new QMC.Common.IOPropertyCollectionView("IO Property Group", 19);
            this.outputIOPropertyCollectionView = new QMC.Common.IOPropertyCollectionView("IO Property Group", 19);
            this.inputpropertyCollectionView = new QMC.Common.PropertyCollectionView();
            this.outputpropertyCollectionView = new QMC.Common.PropertyCollectionView();
            this.SuspendLayout();
            // 
            // btn_Save_Setup_Input
            // 
            this.btn_Save_Setup_Input.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Save_Setup_Input.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_Save_Setup_Input.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Save_Setup_Input.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Save_Setup_Input.CustomForeColor = System.Drawing.Color.Black;
            this.btn_Save_Setup_Input.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Save_Setup_Input.ForeColor = System.Drawing.Color.Black;
            this.btn_Save_Setup_Input.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_Save_Setup_Input.Location = new System.Drawing.Point(672, 687);
            this.btn_Save_Setup_Input.Name = "btn_Save_Setup_Input";
            this.btn_Save_Setup_Input.Size = new System.Drawing.Size(100, 40);
            this.btn_Save_Setup_Input.TabIndex = 5;
            this.btn_Save_Setup_Input.TabStop = false;
            this.btn_Save_Setup_Input.Text = "Save";
            this.btn_Save_Setup_Input.UseVisualStyleBackColor = false;
            this.btn_Save_Setup_Input.Click += new System.EventHandler(this.btn_Save_Setup_Input_Property_Click);
            // 
            // btn_Save_Setup_Ouput
            // 
            this.btn_Save_Setup_Ouput.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Save_Setup_Ouput.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_Save_Setup_Ouput.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Save_Setup_Ouput.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Save_Setup_Ouput.CustomForeColor = System.Drawing.Color.Black;
            this.btn_Save_Setup_Ouput.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Save_Setup_Ouput.ForeColor = System.Drawing.Color.Black;
            this.btn_Save_Setup_Ouput.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_Save_Setup_Ouput.Location = new System.Drawing.Point(1141, 687);
            this.btn_Save_Setup_Ouput.Name = "btn_Save_Setup_Ouput";
            this.btn_Save_Setup_Ouput.Size = new System.Drawing.Size(100, 40);
            this.btn_Save_Setup_Ouput.TabIndex = 4;
            this.btn_Save_Setup_Ouput.TabStop = false;
            this.btn_Save_Setup_Ouput.Text = "Save";
            this.btn_Save_Setup_Ouput.UseVisualStyleBackColor = false;
            this.btn_Save_Setup_Ouput.Click += new System.EventHandler(this.btn_Save_Setup_Output_Property_Click);
            // 
            // dioModuleListBoxItemsView
            // 
            this.dioModuleListBoxItemsView.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.dioModuleListBoxItemsView.BorderWidth = 2;
            this.dioModuleListBoxItemsView.GroupName = "DIO Module";
            this.dioModuleListBoxItemsView.Location = new System.Drawing.Point(12, 12);
            this.dioModuleListBoxItemsView.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.dioModuleListBoxItemsView.Name = "dioModuleListBoxItemsView";
            this.dioModuleListBoxItemsView.SelectedIndex = -1;
            this.dioModuleListBoxItemsView.Size = new System.Drawing.Size(305, 722);
            this.dioModuleListBoxItemsView.TabIndex = 2;
            // 
            // inputIOPropertyCollectionView
            // 
            this.inputIOPropertyCollectionView.GroupName = "Digital Input";
            this.inputIOPropertyCollectionView.Location = new System.Drawing.Point(323, 12);
            this.inputIOPropertyCollectionView.Name = "inputIOPropertyCollectionView";
            this.inputIOPropertyCollectionView.Size = new System.Drawing.Size(460, 722);
            this.inputIOPropertyCollectionView.TabIndex = 11;
            // 
            // outputIOPropertyCollectionView
            // 
            this.outputIOPropertyCollectionView.GroupName = "Digital Output";
            this.outputIOPropertyCollectionView.Location = new System.Drawing.Point(792, 12);
            this.outputIOPropertyCollectionView.Name = "outputIOPropertyCollectionView";
            this.outputIOPropertyCollectionView.Size = new System.Drawing.Size(460, 722);
            this.outputIOPropertyCollectionView.TabIndex = 12;
            // 
            // inputpropertyCollectionView
            // 
            this.inputpropertyCollectionView.GroupName = "Property";
            this.inputpropertyCollectionView.Location = new System.Drawing.Point(333, 540);
            this.inputpropertyCollectionView.Name = "inputpropertyCollectionView";
            this.inputpropertyCollectionView.Size = new System.Drawing.Size(323, 187);
            this.inputpropertyCollectionView.TabIndex = 13;
            // 
            // outputpropertyCollectionView
            // 
            this.outputpropertyCollectionView.GroupName = "Property";
            this.outputpropertyCollectionView.Location = new System.Drawing.Point(802, 540);
            this.outputpropertyCollectionView.Name = "outputpropertyCollectionView";
            this.outputpropertyCollectionView.Size = new System.Drawing.Size(323, 187);
            this.outputpropertyCollectionView.TabIndex = 14;
            // 
            // DigitalIO_Setup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1264, 752);
            this.Controls.Add(this.outputpropertyCollectionView);
            this.Controls.Add(this.btn_Save_Setup_Ouput);
            this.Controls.Add(this.btn_Save_Setup_Input);
            this.Controls.Add(this.inputpropertyCollectionView);
            this.Controls.Add(this.dioModuleListBoxItemsView);
            this.Controls.Add(this.inputIOPropertyCollectionView);
            this.Controls.Add(this.outputIOPropertyCollectionView);
            this.Name = "DigitalIO_Setup";
            this.Text = "Motion Setup";
            this.ResumeLayout(false);

        }
        #endregion

        // -------- Public/Init
        private void InitializeUI()
        {
            try
            {
                // 필요시 폼 로드시 초기 바인딩 추가
                // 1) Equipment에서 생성해둔 것들을 참조
                _unit = equipment.UnitIO;   // 이미 LoadOrCreateDefault 한 맵 사용
                _scan = equipment.DioScan;  // 이미 10ms로 Start된 스캐너 사용

                // 2) JSON 경로도 동일하게
                _setupPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", "Unit.dio.setup.json");

                // 3) 스캔 이벤트 구독 (중복 방지 위해 한 번만)
                if (_scan != null)
                {
                    _scan.InputChanged += OnInputChanged;
                    _scan.OutputChanged += OnOutputChanged;
                }

                WireIOSelectionEvent();
                BindModuleList();

                //InitializeStatusTimer();     // 실제 위치 주기 갱신 (필요 시)
                //InitializeRadioButtonView();
            }
            catch (Exception ex)
            {
                Log.Write("LCP-280", $"InitializeUI error: {ex}");
            }
        }

        // -------- Binding
        /// <summary>
        /// IO 목록 바인딩 (UNIT_NAME 기준)
        /// </summary>
        private void BindModuleList()
        {
            _moduleListItems.Clear();
            var items = new List<object>();

            // 헤더(선택 불가; 선택되면 무시)
            _moduleListItems.Add(new ModuleListItem { Display = "─ DI Modules ─", IsDI = null, Module = null });
            items.Add(_moduleListItems[_moduleListItems.Count - 1]);

            // DI가 하나라도 있는 모듈
            if (_unit?.Modules != null)
            {
                foreach (var m in _unit.Modules)
                {
                    if (m != null && m.Inputs != null && m.Inputs.Count > 0)
                    {
                        _moduleListItems.Add(new ModuleListItem
                        {
                            Display = m.ModuleName,
                            Module = m,
                            IsDI = true
                        });
                        items.Add(_moduleListItems[_moduleListItems.Count - 1]);
                    }
                }
            }

            // 빈 줄(시각 분리용)
            _moduleListItems.Add(new ModuleListItem { Display = "", IsDI = null, Module = null });
            items.Add(_moduleListItems[_moduleListItems.Count - 1]);

            // DO 헤더
            _moduleListItems.Add(new ModuleListItem { Display = "─ DO Modules ─", IsDI = null, Module = null });
            items.Add(_moduleListItems[_moduleListItems.Count - 1]);

            // DO가 하나라도 있는 모듈
            if (_unit?.Modules != null)
            {
                foreach (var m in _unit.Modules)
                {
                    if (m != null && m.Outputs != null && m.Outputs.Count > 0)
                    {
                        _moduleListItems.Add(new ModuleListItem
                        {
                            Display = m.ModuleName,
                            Module = m,
                            IsDI = false
                        });
                        items.Add(_moduleListItems[_moduleListItems.Count - 1]);
                    }
                }
            }

            // 리스트에 주입
            dioModuleListBoxItemsView.SetItems(items.ToArray());

            // 기본 선택: 첫 DI/DO 실제 항목
            int firstSelectable = _moduleListItems.FindIndex(x => x.IsDI != null);
            if (firstSelectable >= 0)
                dioModuleListBoxItemsView.SelectedIndex = firstSelectable;
        }

        // -------- Events
        private void WireIOSelectionEvent()
        {
            if (dioModuleListBoxItemsView == null) return;

            // 중복 구독 방지
            dioModuleListBoxItemsView.ItemSelected -= OnIOItemSelected;
            dioModuleListBoxItemsView.ItemSelected += OnIOItemSelected;

            // 🔹 IO 클릭 이벤트(이제 IOPropertyCollectionView가 제공)
            //inputIOPropertyCollectionView.ItemClicked += OnInputItemClicked;    // 보통 no-op
            outputIOPropertyCollectionView.ItemClicked += OnOutputItemClicked;   // 토글
            //outputIOPropertyCollectionView.ItemRightClicked += OnOutputItemRightClicked; // 펄스

        }

        /// <summary>
        /// Select IO 리스트에서 항목 선택 시 속성 에디터 구성
        /// </summary>
        private void OnIOItemSelected(object sender, int selectedIndex)
        {
            try
            {
                if (selectedIndex < 0 || selectedIndex >= _moduleListItems.Count) return;
                var item = _moduleListItems[selectedIndex];
                if (item == null || item.IsDI == null || item.Module == null) return;

                if (item.IsDI == true) BindChannels_DI(item.Module);   // DI만 갱신
                else BindChannels_DO(item.Module);   // DO만 갱신
            }
            catch (Exception ex)
            {
                Log.Write("LCP-280", $"OnIOItemSelected error: {ex}");
            }
        }

        private void OnInputChanged(string module, string disp, bool value)
        {
            if (_lastDiModule != null &&
                string.Equals(module, _lastDiModule.ModuleName, StringComparison.OrdinalIgnoreCase) &&
                inputIOPropertyCollectionView != null)
            {
                void Update() => inputIOPropertyCollectionView.SetStateByKey(disp, value);

                if (inputIOPropertyCollectionView.IsHandleCreated && inputIOPropertyCollectionView.InvokeRequired)
                    inputIOPropertyCollectionView.BeginInvoke((Action)Update);
                else
                    Update();
            }

            //if (_lastDiModule != null &&
            //    string.Equals(module, _lastDiModule.ModuleName, StringComparison.OrdinalIgnoreCase))
            //{
            //    // 부분 업데이트
            //    inputIOPropertyCollectionView.SetStateByKey(disp, value);
            //    // (또는 전체 바인딩 재호출: BindChannels_DI(_lastDiModule); )
            //}
        }

        private void OnOutputChanged(string module, string disp, bool value)
        {
            if (_lastDoModule != null &&
                string.Equals(module, _lastDoModule.ModuleName, StringComparison.OrdinalIgnoreCase) &&
                outputIOPropertyCollectionView != null)
            {
                void Update() => outputIOPropertyCollectionView.SetStateByKey(disp, value);

                if (outputIOPropertyCollectionView.IsHandleCreated && outputIOPropertyCollectionView.InvokeRequired)
                    outputIOPropertyCollectionView.BeginInvoke((Action)Update);
                else
                    Update();
            }

            //if (_lastDoModule != null &&
            //string.Equals(module, _lastDoModule.ModuleName, StringComparison.OrdinalIgnoreCase))
            //{
            //    outputIOPropertyCollectionView.SetStateByKey(disp, value);
            //    // (또는 BindChannels_DO(_lastDoModule); )
            //}
        }

        private void OnOutputItemClicked(object sender, string key)
        {
            var m = _lastDoModule;
            if (_scan == null || m == null || string.IsNullOrEmpty(key)) return;

            var ask = new MessageBoxYesNo();
            if (ask.ShowDialog("Info", "Signal 변경하시겠습니까?") == DialogResult.No) return;

            // 1) 지금 캐시값
            bool before = false;
            _scan.TryGetOutput(m.ModuleName, key, out before);

            // 2) 쓰기 (Reverse는 DioScanService에서 자동 반영)
            var rc = _scan.WriteOutput(m.ModuleName, key, !before);
            if (rc != 0)
            {
                // -1: 키 못 찾음(설정/키 불일치), 기타: 드라이버 에러
                new MessageBoxOk().ShowDialog("Error", $"WriteOutput 실패 (rc={rc})");
                return;
            }

            // 3) 보드에서 실제 값 재읽기(출력도 캐시에 반영)
            _scan.RefreshOnce();

            bool after = before;
            _scan.TryGetOutput(m.ModuleName, key, out after);

            new MessageBoxOk().ShowDialog("Info!", $"{key}: {before} -> {after}");
        }

        private void OnOutputItemRightClicked(object sender, string key)
        {
            var m = _lastDoModule;
            if (_scan == null || m == null || string.IsNullOrEmpty(key)) return;

            _scan.PulseOutput(m.ModuleName, key, 100);
        }

        // -------- Builders
        private void BindChannels_DI(DIOModuleSetup module)
        {
            _lastDiModule = module;

            var diProps = new PropertyCollection { ShowNoColumn = false };
            int no = 1;
            if (module != null && module.Inputs != null)
            {
                foreach (var ch in module.Inputs)
                {
                    bool v = false; if (_scan != null) _scan.TryGetInput(module.ModuleName, ch.DisplayNo, out v);
                    diProps.Add(new PropertyState(no.ToString("00"), $"{ch.DisplayNo} {ch.Name}", v));
                    no++;
                }
            }
            inputIOPropertyCollectionView?.SetProperties(diProps);

            // ★ 선택 직후 최신값 한 번 더 반영
            RefreshStatesOnce();
        }

        private void BindChannels_DO(DIOModuleSetup module)
        {
            _lastDoModule = module;

            var doProps = new PropertyCollection { ShowNoColumn = false };
            int no = 1;
            if (module != null && module.Outputs != null)
            {
                foreach (var ch in module.Outputs)
                {
                    bool v = false; if (_scan != null) _scan.TryGetOutput(module.ModuleName, ch.DisplayNo, out v);

                    // 상태 행
                    var state = new PropertyState(no.ToString("00"), $"{ch.DisplayNo} {ch.Name}", v);
                    doProps.Add(state);

                    no++;
                }
            }
            outputIOPropertyCollectionView?.SetProperties(doProps);

            // ★ 선택 직후 최신값 한 번 더 반영
            RefreshStatesOnce();
        }

        private void Clear_DI()
        {
            inputIOPropertyCollectionView?.SetProperties(new PropertyCollection { ShowNoColumn = false });
        }

        private void Clear_DO()
        {
            outputIOPropertyCollectionView?.SetProperties(new PropertyCollection { ShowNoColumn = false });
        }

        private PropertyCollection BindChannels(DIOModuleSetup module)
        {
            _selected = module;
            if (module == null)
            {
                inputIOPropertyCollectionView?.SetProperties(new PropertyCollection());
                outputIOPropertyCollectionView?.SetProperties(new PropertyCollection());
                return null;
            }

            // ===== DI: 입력 패널 =====
            var diProps = new PropertyCollection { ShowNoColumn = false };
            int no = 1;
            foreach (var ch in module.Inputs)
            {
                bool value = false;
                _scan?.TryGetInput(module.ModuleName, ch.DisplayNo, out value); // 캐시값 있으면 사용
                diProps.Add(new PropertyState(
                    no.ToString("00"),
                    $"{ch.DisplayNo} {ch.Name}",
                    value
                ));
                no++;
            }
            inputIOPropertyCollectionView?.SetProperties(diProps);

            // ===== DO: 출력 패널 =====
            var doProps = new PropertyCollection { ShowNoColumn = false };
            no = 1;
            foreach (var ch in module.Outputs)
            {
                bool value = false;
                _scan?.TryGetOutput(module.ModuleName, ch.DisplayNo, out value);
                doProps.Add(new PropertyState(
                    no.ToString("00"),
                    $"{ch.DisplayNo} {ch.Name}",
                    value
                ));
                no++;
            }
            outputIOPropertyCollectionView?.SetProperties(doProps);

            // 선택 직후 1회 값 동기화(스캐너 캐시 기준)
            RefreshStatesOnce();

            return null;
        }

        private void RefreshStatesOnce()
        {
            if (_scan == null) return;

            // DI
            if (_lastDiModule?.Inputs != null && inputIOPropertyCollectionView != null)
            {
                foreach (var ch in _lastDiModule.Inputs)
                {
                    if (_scan.TryGetInput(_lastDiModule.ModuleName, ch.DisplayNo, out var v))
                    {
                        void Update() => inputIOPropertyCollectionView.SetStateByKey(ch.DisplayNo, v);

                        if (inputIOPropertyCollectionView.IsHandleCreated && inputIOPropertyCollectionView.InvokeRequired)
                            inputIOPropertyCollectionView.BeginInvoke((Action)Update);
                        else
                            Update();
                    }
                }
            }

            // DO
            if (_lastDoModule?.Outputs != null && outputIOPropertyCollectionView != null)
            {
                foreach (var ch in _lastDoModule.Outputs)
                {
                    if (_scan.TryGetOutput(_lastDoModule.ModuleName, ch.DisplayNo, out var v))
                    {
                        void Update() => outputIOPropertyCollectionView.SetStateByKey(ch.DisplayNo, v);

                        if (outputIOPropertyCollectionView.IsHandleCreated && outputIOPropertyCollectionView.InvokeRequired)
                            outputIOPropertyCollectionView.BeginInvoke((Action)Update);
                        else
                            Update();
                    }
                }
            }

            //if (_selected == null || _scan == null) return;

            //// DI
            //if (_selected.Inputs != null)
            //{
            //    int i = 1;
            //    foreach (var ch in _selected.Inputs)
            //    {
            //        if (_scan.TryGetInput(_selected.ModuleName, ch.DisplayNo, out var v))
            //        {
            //            // 같은 순서(i)로 넣었으니 필요하면 여기서 PropertyCollectionView의 값 업데이트 API 사용
            //            // 예: inputIOPropertyCollectionView.SetValueByRow(i-1, v);  (컨트롤 헬퍼가 있으면 사용)
            //        }
            //        i++;
            //    }
            //}

            //// DO
            //if (_selected.Outputs != null)
            //{
            //    int i = 1;
            //    foreach (var ch in _selected.Outputs)
            //    {
            //        if (_scan.TryGetOutput(_selected.ModuleName, ch.DisplayNo, out var v))
            //        {
            //            // 예: outputIOPropertyCollectionView.SetValueByRow(i-1, v);
            //        }
            //        i++;
            //    }
            //}
        }
        // --- Paint / Resize (keep)
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            this.Invalidate();
        }

        

    }
}
