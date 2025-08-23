using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace QMC.Common.Motions
{
    public enum ProfileMode
    {
        Trapezoid = 0,
        SCurve = 1
    }

    [Serializable]
    public sealed class MotionAxisConfig
    {
        // ===== Home Speed =====
        [Category("Speed.Home")]
        [DisplayName("Home Speed (mm/s)")]
        public double HomeSpeed { get; set; } = 0.000;

        [Category("Speed.Home")]
        [DisplayName("H-Return Speed (mm/s)")]
        public double HomeReturnSpeed { get; set; } = 0.000;

        [Category("Speed.Home")]
        [DisplayName("H-Recursion Speed (mm/s)")]
        public double HomeRecursionSpeed { get; set; } = 0.000;

        [Category("Speed.Home")]
        [DisplayName("Z-Phase Speed (mm/s)")]
        public double ZPhaseSpeed { get; set; } = 0.000;

        [Category("Speed.Home")]
        [DisplayName("Home Acc (mm/s^2)")]
        public double HomeAcc { get; set; } = 0.000;

        [Category("Speed.Home")]
        [DisplayName("H-Return Acc (mm/s^2)")]
        public double HomeReturnAcc { get; set; } = 0.000;


        // ===== Jog =====
        [Category("Speed.Jog")]
        [DisplayName("Fine Velocity (mm/s)")]
        public double JogFineVelocity { get; set; } = 0.000;

        [Category("Speed.Jog")]
        [DisplayName("Coarse Velocity (mm/s)")]
        public double JogCoarseVelocity { get; set; } = 0.000;

        [Category("Speed.Jog")]
        [DisplayName("Accelerator (mm/s^2)")]
        public double JogAcc { get; set; } = 0.000;

        [Category("Speed.Jog")]
        [DisplayName("Decelerator (mm/s^2)")]
        public double JogDec { get; set; } = 0.000;


        // ===== Run =====
        [Category("Speed.Run")]
        [DisplayName("Max Velocity (mm/s)")]
        public double MaxVelocity { get; set; } = 0.000;

        [Category("Speed.Run")]
        [DisplayName("Accelerator (mm/s^2)")]
        public double RunAcc { get; set; } = 0.000;

        [Category("Speed.Run")]
        [DisplayName("Decelerator (mm/s^2)")]
        public double RunDec { get; set; } = 0.000;


        // ===== Profile =====
        [Category("Profile")]
        [DisplayName("Mode")]
        public ProfileMode ProfileMode { get; set; } = ProfileMode.SCurve;

        [Category("Profile")]
        [DisplayName("Accelerator Jerk (%)")]
        public int AccJerkPercent { get; set; } = 50;

        [Category("Profile")]
        [DisplayName("Decelerator Jerk (%)")]
        public int DecJerkPercent { get; set; } = 50;


        // ===== 공통 운전 보정/품질 =====
        [Category("Operation")]
        [DisplayName("Inposition Tolerance (mm)")]
        public double InposTolerance { get; set; } = 0.002;

        [Category("Operation")]
        [DisplayName("Logical Scale Factor")]
        public double LogicalScaleFactor { get; set; } = 1.0000;

        [Category("Operation")]
        [DisplayName("Offset (mm)")]
        public double Offset { get; set; } = 0.000;

        public void Validate()
        {
            if (AccJerkPercent < 0 || AccJerkPercent > 100)
                throw new ArgumentOutOfRangeException(nameof(AccJerkPercent));

            if (DecJerkPercent < 0 || DecJerkPercent > 100)
                throw new ArgumentOutOfRangeException(nameof(DecJerkPercent));

            if (InposTolerance < 0)
                throw new ArgumentOutOfRangeException(nameof(InposTolerance));
        }

        #region JSON Persistence
        private static JsonSerializerSettings JsonSettings
        {
            get
            {
                return new JsonSerializerSettings
                {
                    Formatting = Formatting.None,
                    NullValueHandling = NullValueHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.Include
                };
            }
        }

        public string ToJson(bool indented)
        {
            return JsonConvert.SerializeObject(
                this,
                indented ? Formatting.Indented : Formatting.None,
                JsonSettings
            );
        }

        public void Save(string filePath, bool indented = true)
        {
            // 저장 전에 유효성 검사
            Validate();

            var json = ToJson(indented);
            File.WriteAllText(filePath, json, Encoding.UTF8);
        }

        public static MotionAxisConfig FromJson(string json)
        {
            var obj = JsonConvert.DeserializeObject<MotionAxisConfig>(json, JsonSettings);
            if (obj == null) throw new InvalidDataException("Invalid JSON for MotionAxisConfig.");
            obj.Validate();
            return obj;
        }

        public static MotionAxisConfig Load(string filePath)
        {
            var json = File.ReadAllText(filePath, Encoding.UTF8);
            return FromJson(json);
        }

        public static bool TryLoad(string filePath, out MotionAxisConfig result, out string error)
        {
            result = null;
            error = null;
            try
            {
                result = Load(filePath);
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        public bool TrySave(string filePath, bool indented, out string error)
        {
            error = null;
            try
            {
                Validate();
                var dir = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var json = ToJson(indented);
                File.WriteAllText(filePath, json, Encoding.UTF8);
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        // 편의 오버로드
        public bool TrySave(string filePath, out string error)
        {
            return TrySave(filePath, true, out error);
        }
        #endregion

    }
}
