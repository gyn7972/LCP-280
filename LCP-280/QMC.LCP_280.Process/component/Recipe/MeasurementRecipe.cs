using System;
using System.Collections.Generic;
using System.ComponentModel;
using QMC.Common;

namespace QMC.LCP_280.Process.Component
{
    [Serializable]
    public sealed class MeasurementRecipe : QMC.Common.BaseRecipe
    {
        // 명시적 매개변수 없는 생성자(new() 제약 만족)
        public MeasurementRecipe() : this(null) { }
        public MeasurementRecipe(string name = null) : base(name) { }

        // 1. MeasurementParameters (검사 파라미터) 참조
        [Category("Measurement"), DisplayName("Use Measurement Recipe")]
        [DefaultValue(true)]
        public bool UseMeasurementRecipe { get; set; } = true;

        [Category("Measurement"), DisplayName("Measurement Recipe Name")]
        [DefaultValue("DefaultMeasurement")]
        public string MeasurementRecipeName { get; set; } = "DefaultMeasurement";

        [Category("Measurement"), DisplayName("Measurement Recipe Path")]
        [DefaultValue("")]
        public string MeasurementRecipePath { get; set; } = string.Empty;

        // 2. VisionParameters (비전 파라미터) 참조
        [Category("Vision"), DisplayName("Use Vision Recipe")]
        [DefaultValue(true)]
        public bool UseVisionRecipe { get; set; } = true;

        [Category("Vision"), DisplayName("Vision Recipe Name")]
        [DefaultValue("DefaultVision")]
        public string VisionRecipeName { get; set; } = "DefaultVision";

        [Category("Vision"), DisplayName("Vision Recipe Path")]
        [DefaultValue("")]
        public string VisionRecipePath { get; set; } = string.Empty;

        // 3. WaferMap (웨이퍼 맵) 정보 (웨이퍼 직경, 칩 크기, 행/열 개수 등)
        // ===== Wafer Map =====
        [Category("Wafer Map"), DisplayName("Wafer Diameter (mm)")]
        [DefaultValue(300.0)]
        public double WaferDiameter { get; set; } = 300.0;

        [Category("Wafer Map"), DisplayName("Chip Width (mm)")]
        [DefaultValue(5.0)]
        public double ChipWidth { get; set; } = 5.0;

        [Category("Wafer Map"), DisplayName("Chip Height (mm)")]
        [DefaultValue(5.0)]
        public double ChipHeight { get; set; } = 5.0;

        [Category("Wafer Map"), DisplayName("Row Count")]
        [DefaultValue(60)]
        public int Rows { get; set; } = 60;

        [Category("Wafer Map"), DisplayName("Col Count")]
        [DefaultValue(60)]
        public int Cols { get; set; } = 60;

        [Category("Wafer Map"), DisplayName("Map File Path")]
        [DefaultValue("")]
        public string MapFilePath { get; set; } = string.Empty;

        public enum MapRotateOption { None, CW90, CW180, CW270 }
        [Category("Wafer Map"), DisplayName("Map Rotate")]
        [DefaultValue(MapRotateOption.None)]
        public MapRotateOption MapRotate { get; set; } = MapRotateOption.None;

        public enum MapMirrorOption { None, X, Y, XY }
        [Category("Wafer Map"), DisplayName("Map Mirror")]
        [DefaultValue(MapMirrorOption.None)]
        public MapMirrorOption MapMirror { get; set; } = MapMirrorOption.None;

        [Category("Wafer Map"), DisplayName("Map Match Limit (%)")]
        [DefaultValue(90.0)]
        public double MapMatchLimitPercent { get; set; } = 90.0;

        // Bin Array
        [Category("Bin Array"), DisplayName("Bin Count X")]
        [DefaultValue(50)]
        public int BinCountX { get; set; } = 50;

        [Category("Bin Array"), DisplayName("Bin Count Y")]
        [DefaultValue(50)]
        public int BinCountY { get; set; } = 50;

        [Category("Bin Array"), DisplayName("Bin Pitch X (um)")]
        [DefaultValue(1500)]
        public int BinPitchXUm { get; set; } = 1500;

        [Category("Bin Array"), DisplayName("Bin Pitch Y (um)")]
        [DefaultValue(1500)]
        public int BinPitchYUm { get; set; } = 1500;

        // Material
        [Category("Material"), DisplayName("Tape Thickness (um)")]
        [DefaultValue(80.0)]
        public double TapeThicknessUm { get; set; } = 80.0;

        [Category("Material"), DisplayName("Chip Thickness (um)")]
        [DefaultValue(120.0)]
        public double ChipThicknessUm { get; set; } = 120.0;

        // ===== Measurement Keys =====
        public List<MeasurementKey> Keys { get; set; } = new List<MeasurementKey>();

        public override void Reset()
        {
            Keys.Clear();
        }

        public override bool Validate()
        {
            if (Rows <= 0 || Cols <= 0) throw new ArgumentOutOfRangeException("Map size invalid.");
            if (WaferDiameter <= 0) throw new ArgumentOutOfRangeException(nameof(WaferDiameter));
            if (ChipWidth <= 0 || ChipHeight <= 0) throw new ArgumentOutOfRangeException("Chip size invalid.");
            if (MapMatchLimitPercent < 0 || MapMatchLimitPercent > 100) throw new ArgumentOutOfRangeException(nameof(MapMatchLimitPercent));
            return true;
        }
    }
}
