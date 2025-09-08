using QMC.Common;
using QMC.Common.CustomControl;
using QMC.Common.DIO; // 추가
using QMC.Common.IO;  // DIOUnit, DIOModuleSetup
using QMC.Common.Motions;
using QMC.LCP_280.Process;
using QMC.LCP_280.Process.Component; // TeachingPositionControl
using QMC.LCP_280.Process.Unit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace QMC.LCP_280.Process.Unit
{
    partial class InputStageUnit_Config
    {
        private IOPropertyCollectionView inputView;
        private IOPropertyCollectionView outputView;
        private IndividualMenuButton btnSave; // (legacy placeholders kept if referenced elsewhere)
        private IndividualMenuButton btnCancel;
        private GroupBox gbPositionTeaching;
        private GroupBox gbDigitalIO;
        private GroupBox gbMoveAxis;
        private JogControl jogControl;
        private System.ComponentModel.IContainer components = null;
        private Timer _axisPosTimer; // (unused currently)

        // Digital IO helper structs
        private struct _IoRef { public string Module; public string Disp; public PropertyState Prop; }
        private readonly List<_IoRef> _ioInputs = new List<_IoRef>();
        private readonly List<_IoRef> _ioOutputs = new List<_IoRef>();
        private Timer _ioTimer; // unused timer kept for potential future use

        // New unified teaching control
        private TeachingPositionControl teachingPositionControl;

        // MISSING designer fields re-added
        private System.Windows.Forms.TableLayoutPanel ioTableLayoutPanel;
        private QMC.Common.ListBoxItemsView axisPositionsView;
        private System.Windows.Forms.TableLayoutPanel mainTableLayoutPanel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }
        #region Windows Form Designer generated code
        private void InitializeComponent()
        {
            this.gbPositionTeaching = new System.Windows.Forms.GroupBox();
            this.gbDigitalIO = new System.Windows.Forms.GroupBox();
            this.ioTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.inputView = new QMC.Common.IOPropertyCollectionView();
            this.outputView = new QMC.Common.IOPropertyCollectionView();
            this.gbMoveAxis = new System.Windows.Forms.GroupBox();
            this.jogControl = new QMC.LCP_280.Process.Unit.JogControl();
            this.axisPositionsView = new QMC.Common.ListBoxItemsView();
            this.mainTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.gbPositionTeaching.SuspendLayout();
            this.gbDigitalIO.SuspendLayout();
            this.ioTableLayoutPanel.SuspendLayout();
            this.gbMoveAxis.SuspendLayout();
            this.mainTableLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbPositionTeaching
            // 
            this.gbPositionTeaching.BackColor = System.Drawing.Color.White;
            this.mainTableLayoutPanel.SetColumnSpan(this.gbPositionTeaching, 2);
            this.gbPositionTeaching.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.gbPositionTeaching.Location = new System.Drawing.Point(3, 3);
            this.gbPositionTeaching.Name = "gbPositionTeaching";
            this.gbPositionTeaching.Size = new System.Drawing.Size(626, 384);
            this.gbPositionTeaching.TabIndex = 8;
            this.gbPositionTeaching.TabStop = false;
            this.gbPositionTeaching.Text = "Teaching Positions";
            // teachingPositionControl (added after groupbox constructed)
            this.teachingPositionControl = new TeachingPositionControl();
            this.teachingPositionControl.Dock = DockStyle.Fill;
            this.gbPositionTeaching.Controls.Add(this.teachingPositionControl);
            // 
            // gbDigitalIO
            // 
            this.gbDigitalIO.BackColor = System.Drawing.Color.White;
            this.mainTableLayoutPanel.SetColumnSpan(this.gbDigitalIO, 2);
            this.gbDigitalIO.Controls.Add(this.ioTableLayoutPanel);
            this.gbDigitalIO.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbDigitalIO.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.gbDigitalIO.Location = new System.Drawing.Point(3, 393);
            this.gbDigitalIO.Name = "gbDigitalIO";
            this.gbDigitalIO.Size = new System.Drawing.Size(626, 384);
            this.gbDigitalIO.TabIndex = 9;
            this.gbDigitalIO.TabStop = false;
            this.gbDigitalIO.Text = "Digital I/O";
            // 
            // ioTableLayoutPanel
            // 
            this.ioTableLayoutPanel.ColumnCount = 2;
            this.ioTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.ioTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.ioTableLayoutPanel.Controls.Add(this.inputView, 0, 0);
            this.ioTableLayoutPanel.Controls.Add(this.outputView, 1, 0);
            this.ioTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ioTableLayoutPanel.Location = new System.Drawing.Point(3, 21);
            this.ioTableLayoutPanel.Name = "ioTableLayoutPanel";
            this.ioTableLayoutPanel.RowCount = 1;
            this.ioTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.ioTableLayoutPanel.Size = new System.Drawing.Size(620, 360);
            this.ioTableLayoutPanel.TabIndex = 2;
            // 
            // inputView
            // 
            this.inputView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.inputView.GroupName = "Input";
            this.inputView.Location = new System.Drawing.Point(4, 6);
            this.inputView.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.inputView.Name = "inputView";
            this.inputView.Size = new System.Drawing.Size(302, 348);
            this.inputView.TabIndex = 1;
            // 
            // outputView
            // 
            this.outputView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.outputView.GroupName = "Output";
            this.outputView.Location = new System.Drawing.Point(314, 6);
            this.outputView.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.outputView.Name = "outputView";
            this.outputView.Size = new System.Drawing.Size(302, 348);
            this.outputView.TabIndex = 1;
            // 
            // gbMoveAxis
            // 
            this.gbMoveAxis.BackColor = System.Drawing.Color.White;
            this.gbMoveAxis.Controls.Add(this.jogControl);
            this.gbMoveAxis.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbMoveAxis.Font = new System.Drawing.Font("맑은 고딕", 10F);
            this.gbMoveAxis.Location = new System.Drawing.Point(635, 3);
            this.gbMoveAxis.Name = "gbMoveAxis";
            this.mainTableLayoutPanel.SetRowSpan(this.gbMoveAxis, 2);
            this.gbMoveAxis.Size = new System.Drawing.Size(310, 774);
            this.gbMoveAxis.TabIndex = 10;
            this.gbMoveAxis.TabStop = false;
            this.gbMoveAxis.Text = "Move Axis";
            // 
            // jogControl
            // 
            this.jogControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.jogControl.Location = new System.Drawing.Point(3, 21);
            this.jogControl.Margin = new System.Windows.Forms.Padding(0);
            this.jogControl.Name = "jogControl";
            this.jogControl.Size = new System.Drawing.Size(304, 750);
            this.jogControl.TabIndex = 0;
            // 
            // axisPositionsView
            // 
            this.axisPositionsView.BorderWidth = 2;
            this.axisPositionsView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.axisPositionsView.GroupName = "Axis Positions";
            this.axisPositionsView.Location = new System.Drawing.Point(951, 3);
            this.axisPositionsView.Name = "axisPositionsView";
            this.mainTableLayoutPanel.SetRowSpan(this.axisPositionsView, 2);
            this.axisPositionsView.SelectedIndex = -1;
            this.axisPositionsView.Size = new System.Drawing.Size(310, 774);
            this.axisPositionsView.TabIndex = 11;
            // 
            // mainTableLayoutPanel
            // 
            this.mainTableLayoutPanel.ColumnCount = 4;
            this.mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.mainTableLayoutPanel.Controls.Add(this.axisPositionsView, 3, 0);
            this.mainTableLayoutPanel.Controls.Add(this.gbDigitalIO, 0, 1);
            this.mainTableLayoutPanel.Controls.Add(this.gbPositionTeaching, 0, 0);
            this.mainTableLayoutPanel.Controls.Add(this.gbMoveAxis, 2, 0);
            this.mainTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainTableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.mainTableLayoutPanel.Name = "mainTableLayoutPanel";
            this.mainTableLayoutPanel.RowCount = 2;
            this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.mainTableLayoutPanel.Size = new System.Drawing.Size(1264, 780);
            this.mainTableLayoutPanel.TabIndex = 12;
            // 
            // InputStageUnit_Config
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1264, 780);
            this.Controls.Add(this.mainTableLayoutPanel);
            this.Name = "InputStageUnit_Config";
            this.Text = "InputStage Unit Configuration";
            this.gbPositionTeaching.ResumeLayout(false);
            this.gbDigitalIO.ResumeLayout(false);
            this.ioTableLayoutPanel.ResumeLayout(false);
            this.gbMoveAxis.ResumeLayout(false);
            this.mainTableLayoutPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        private void InitializeUI()
        {
            try
            {
                InitializeDigitalIO();
                InitializeTeachingControl();
            }
            catch (Exception ex)
            {
                Console.WriteLine("InitializeUI error: " + ex.Message);
            }
        }

        private void InitializeTeachingControl()
        {
            try
            {
                teachingPositionControl?.ClearUnits();
                var eq = Equipment.Instance;
                if (eq?.Units != null && eq.Units.TryGetValue("InputStage", out var u) && u is InputStage stage)
                {
                    teachingPositionControl.RegisterUnit(
                        "InputStage",
                        stage,
                        () => stage.InputStageConfig?.TeachingPositions,
                        (name, vel) => stage.MoveToTeachingPosition(name, vel: vel),
                        tp => stage.InputStageConfig?.SetTeachingPosition(tp),
                        autoReload: true);

                    teachingPositionControl.AlwaysShowSaveCancel = true;
                    teachingPositionControl.SetSaveCancelVisible(true, true);
                    teachingPositionControl.RefreshData();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("InitializeTeachingControl error: " + ex.Message);
            }
        }

        // ===== Digital IO 초기화 (retained from original) =====
        private void InitializeDigitalIO()
        {
            try
            {
                if (inputView == null) return;
                var eq = Equipment.Instance; var scan = eq?.DioScan; var unitIO = eq?.UnitIO;
                if (scan == null || unitIO == null)
                { inputView.SetProperties(new PropertyCollection()); outputView.SetProperties(new PropertyCollection()); return; }
                _ioInputs.Clear(); _ioOutputs.Clear();
                HardInputDef[] hardInputs; HardOutputDef[] hardOutputs;
                const string UNIT_NAME = "InputStage";
                if (eq?.Units != null && eq.Units.TryGetValue(UNIT_NAME, out var unit) && unit is InputStage stage && stage.InputStageConfig != null)
                {
                    var cfg = stage.InputStageConfig; var cfgType = cfg.GetType();
                    var piIn = cfgType.GetProperty("HardInputs");
                    hardInputs = piIn?.GetValue(cfg) as HardInputDef[] ?? Array.Empty<HardInputDef>();
                    var piOut = cfgType.GetProperty("HardOutputs");
                    hardOutputs = piOut?.GetValue(cfg) as HardOutputDef[] ?? Array.Empty<HardOutputDef>();
                }
                else { hardInputs = Array.Empty<HardInputDef>(); hardOutputs = Array.Empty<HardOutputDef>(); }

                Func<string, Tuple<string, string>> resolveIn = disp => { if (unitIO?.Modules == null) return new Tuple<string, string>(null, disp); foreach (var m in unitIO.Modules) { if (m?.Inputs == null) continue; foreach (var ch in m.Inputs) if (string.Equals(ch.DisplayNo, disp, StringComparison.OrdinalIgnoreCase)) return new Tuple<string, string>(m.ModuleName, ch.DisplayNo); } return new Tuple<string, string>(null, disp); };
                Func<string, Tuple<string, string>> resolveOut = disp => { if (unitIO?.Modules == null) return new Tuple<string, string>(null, disp); foreach (var m in unitIO.Modules) { if (m?.Outputs == null) continue; foreach (var ch in m.Outputs) if (string.Equals(ch.DisplayNo, disp, StringComparison.OrdinalIgnoreCase)) return new Tuple<string, string>(m.ModuleName, ch.DisplayNo); } return new Tuple<string, string>(null, disp); };

                if (hardInputs.Length > 0)
                {
                    var pcIn = new PropertyCollection { ShowNoColumn = true, IsInputParameter = false }; pcIn.Add(new TitleOnlyProperty("No", "Name", "State"));
                    foreach (var item in hardInputs)
                    { var map = resolveIn(item.Disp); bool cur = false; if (map.Item1 != null) scan.TryGetInput(map.Item1, map.Item2, out cur); string nameCell = $"{item.Disp} {item.Name}"; var ps = new PropertyState(item.No.ToString(), nameCell, cur); pcIn.Add(ps); _ioInputs.Add(new _IoRef { Module = map.Item1, Disp = map.Item2, Prop = ps }); }
                    inputView.SetProperties(pcIn);
                }
                else inputView.SetProperties(new PropertyCollection());

                if (hardOutputs.Length > 0)
                { var pcOut = new PropertyCollection { ShowNoColumn = true, IsInputParameter = false }; pcOut.Add(new TitleOnlyProperty("No", "Name", "State")); foreach (var item in hardOutputs) { var map = resolveOut(item.Disp); string nameCell = $"{item.Disp} {item.Name}"; var ps = new PropertyState(item.No.ToString(), nameCell, false); pcOut.Add(ps); _ioOutputs.Add(new _IoRef { Module = map.Item1, Disp = map.Item2, Prop = ps }); } outputView.SetProperties(pcOut); }
                else outputView.SetProperties(new PropertyCollection());

                scan.InputChanged -= OnDioInputChanged; scan.InputChanged += OnDioInputChanged;
            }
            catch (Exception ex) { Console.WriteLine("InitializeDigitalIO error: " + ex.Message); }
        }

        private void OnDioInputChanged(string module, string disp, bool value)
        {
            try
            {
                for (int i = 0; i < _ioInputs.Count; i++)
                {
                    if (_ioInputs[i].Module == module && string.Equals(_ioInputs[i].Disp, disp, StringComparison.OrdinalIgnoreCase))
                    { _ioInputs[i].Prop.State = value; inputView.SetStateByKey(disp, value); break; }
                }
            }
            catch { }
        }
    }
}