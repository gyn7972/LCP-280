using Newtonsoft.Json;
using QMC.Common;
using QMC.Common.Vision;
using QMC.Common.VisionPart;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using static QMC.LCP_280.Process.PatternMatchingRunner;

namespace QMC.LCP_280.Process
{
    /// <summary>
    /// JSON 저장용 ROI 데이터
    /// </summary>
    [Serializable]
    public class PatternMatchingRoiJson
    {
        public Point TrainStart { get; set; }
        public Point TrainEnd { get; set; }
        public Point InspectStart { get; set; }
        public Point InspectEnd { get; set; }
    }

    /// <summary>
    /// 개별 Train 이미지 직렬화 정보
    /// </summary>
    [Serializable]
    public class SerializedTrainImage
    {
        public string Tag { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string ImageBase64 { get; set; } // v1.1 하위호환 (PNG)
        public string FilePath { get; set; }    // recipe 폴더 기준 상대경로 (예: {RecipeName}_Vision/0_Train0.png)
    }

    /// <summary>
    /// Mode 별 Pattern Matching 파라미터 + ROI를 JSON으로 저장/로드하는 컨테이너
    /// </summary>
    [Serializable]
    public class PatternMatchingModeRecipeJson
    {
        public MultiPatternMatchingParameters Parameters { get; set; }
        public PatternMatchingRoiJson Roi { get; set; } = new PatternMatchingRoiJson();
        public List<SerializedTrainImage> TrainImages { get; set; } = new List<SerializedTrainImage>();
    }

    /// <summary>
    /// Pattern Matching 전체 파라미터 + ROI를 JSON으로 저장/로드하는 컨테이너
    /// </summary>
    [Serializable]
    public class PatternMatchingRecipeJson
    {
        public string Version { get; set; } = "2.0"; // mode 지원
        public DateTime SavedAt { get; set; } = DateTime.Now;
        public string LastCameraName { get; set; }

        // [Legacy]
        public MultiPatternMatchingParameters Parameters { get; set; }
        public PatternMatchingRoiJson Roi { get; set; } = new PatternMatchingRoiJson();
        public List<SerializedTrainImage> TrainImages { get; set; } = new List<SerializedTrainImage>();

        // [NEW] Camera 내 Mode별 데이터
        public Dictionary<string, PatternMatchingModeRecipeJson> Modes { get; set; }
            = new Dictionary<string, PatternMatchingModeRecipeJson>(StringComparer.OrdinalIgnoreCase);
    }

    internal static class PatternMatchingRecipeStore
    {
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Include,
            ObjectCreationHandling = ObjectCreationHandling.Replace
        };

        #region Public API
        private static string NormalizeModeKey(string modeKey)
        {
            return string.IsNullOrWhiteSpace(modeKey) ? "Prealign" : modeKey.Trim();
        }

        private static string NormalizeModeKey(ProcessMode mode)
        {
            return mode.ToString(); // "Prealign", "MapMatching", "SecondAlign"
        }

        public static void Save(string filePath, PatternMatchingRecipeJson data, ProcessMode mode)
        {
            Save(filePath, data, NormalizeModeKey(mode));
        }
        public static PatternMatchingRecipeJson Load(string filePath, ProcessMode mode, bool fallbackLegacy = true)
        {
            return Load(filePath, NormalizeModeKey(mode), fallbackLegacy);
        }

        public static void Save(string filePath, PatternMatchingRecipeJson data, string modeKey)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            if (string.IsNullOrWhiteSpace(modeKey)) modeKey = "Prealign";
            if (data == null) data = new PatternMatchingRecipeJson();
            if (data.Modes == null)
                data.Modes = new Dictionary<string, PatternMatchingModeRecipeJson>(StringComparer.OrdinalIgnoreCase);

            if (!data.Modes.TryGetValue(modeKey, out var mode))
            {
                mode = new PatternMatchingModeRecipeJson();
                data.Modes[modeKey] = mode;
            }

            mode.Parameters = data.Parameters?.Clone();
            mode.Roi = data.Roi ?? new PatternMatchingRoiJson();

            // mode.TrainImages 직렬화는 기존 로직 재사용해서 mode 기준으로 채움
            mode.TrainImages.Clear();
            if (mode.Parameters?.TrainImages != null)
            {
                // 기존 Save 이미지 파일 분리 로직 그대로 재사용하되
                // 경로: {recipeFileName}/{modeKey}/...
                // (또는 현재처럼 같은 폴더 사용)
            }

            // legacy도 유지(하위 호환)
            data.Parameters = mode.Parameters?.Clone();
            data.Roi = mode.Roi;
            data.TrainImages = mode.TrainImages.ToList();

            try
            {
                if (data.Parameters.TrainImages != null)
                {
                    string recipeDir = Path.GetDirectoryName(filePath);

                    // 폴더명 = 레시피 파일명(확장자 제외) 그대로
                    // ex) 레시피 파일: ABC_Vision.json  --> 이미지 폴더: ABC_Vision
                    string imageDirName = Path.GetFileNameWithoutExtension(filePath);
                    string imageDir = Path.Combine(recipeDir, imageDirName);

                    if (Directory.Exists(imageDir))
                    {
                        try { Directory.Delete(imageDir, true); } catch { }
                    }
                    Directory.CreateDirectory(imageDir);

                    for (int i = 0; i < data.Parameters.TrainImages.Count; i++)
                    {
                        var v = data.Parameters.TrainImages[i];
                        if (v == null || v.GetImage() == null) continue;

                        var serialized = ToSerialized(v);

                        string safeTag = SanitizeFileName(serialized.Tag ?? $"Train{i}");
                        string fileName = $"{i}_{safeTag}.png";
                        string saveFullPath = Path.Combine(imageDir, fileName);

                        try
                        {
                            var bmp = v.GetImage();
                            bmp.Save(saveFullPath, ImageFormat.Png);

                            // 상대경로: "{레시피명}_Vision/0_Train0.png"
                            serialized.FilePath = Path.Combine(imageDirName, fileName).Replace('\\', '/');
                            serialized.ImageBase64 = null;
                        }
                        catch (Exception ex)
                        {
                            // 파일 저장 실패 시 Base64 fallback
                            try
                            {
                                var bmp = v.GetImage();
                                using (var ms = new MemoryStream())
                                {
                                    bmp.Save(ms, ImageFormat.Png);
                                    serialized.ImageBase64 = Convert.ToBase64String(ms.ToArray());
                                }
                            }
                            catch { }

                            Log.Write("PatternMatchingRecipeStore", "Save image file failed: " + ex.Message);
                        }

                        data.TrainImages.Add(serialized);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write("PatternMatchingRecipeStore", "Serialize Train Images failed: " + ex.Message);
            }

            try { Directory.CreateDirectory(Path.GetDirectoryName(filePath)); } catch { }
            var json = JsonConvert.SerializeObject(data, Settings);
            File.WriteAllText(filePath, json);
        }

        public static PatternMatchingRecipeJson Load(string filePath)
        {
            if (!File.Exists(filePath)) return null;

            try
            {
                var json = File.ReadAllText(filePath);
                var container = JsonConvert.DeserializeObject<PatternMatchingRecipeJson>(json, Settings);

                if (container == null)
                    return null;

                // 외부 파일 기반 역직렬화
                if (container.TrainImages != null && container.TrainImages.Count > 0)
                {
                    if (container.Parameters == null)
                        container.Parameters = new MultiPatternMatchingParameters();

                    container.Parameters.TrainImages.Clear();

                    string recipeDir = Path.GetDirectoryName(filePath);

                    foreach (var ti in container.TrainImages)
                    {
                        VisionImage img = null;

                        // FilePath 우선
                        if (!string.IsNullOrEmpty(ti.FilePath))
                        {
                            string full = Path.Combine(recipeDir, ti.FilePath.Replace('/', Path.DirectorySeparatorChar));
                            if (File.Exists(full))
                            {
                                try
                                {
                                    using (var bmp = (Bitmap)Image.FromFile(full))
                                    {
                                        var clone = new Bitmap(bmp);
                                        img = VisionImage.CreateInstance(clone);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Log.Write("PatternMatchingRecipeStore", "Load image file failed: " + ex.Message);
                                }
                            }
                        }

                        // Base64 하위호환
                        if (img == null && !string.IsNullOrEmpty(ti.ImageBase64))
                            img = FromSerialized(ti);

                        if (img != null)
                        {
                            img.Tag = ti.Tag;
                            container.Parameters.TrainImages.Add(img);
                        }
                    }
                }

                return container;
            }
            catch (Exception ex)
            {
                Log.Write("PatternMatchingRecipeStore", "Load failed: " + ex.Message);
                return null;
            }
        }

        public static PatternMatchingRecipeJson Load(string filePath, string modeKey, bool fallbackLegacy = true)
        {
            var container = Load(filePath); // 기존 Load
            if (container == null) 
                return null;

            if (string.IsNullOrWhiteSpace(modeKey)) modeKey = "Prealign";

            if (container.Modes != null && container.Modes.TryGetValue(modeKey, out var slot) && slot != null)
            {
                container.Parameters = slot.Parameters?.Clone();
                container.Roi = slot.Roi ?? new PatternMatchingRoiJson();
                return container;
            }

            if (fallbackLegacy)
            {
                if (container.Modes == null)
                    container.Modes = new Dictionary<string, PatternMatchingModeRecipeJson>(StringComparer.OrdinalIgnoreCase);

                // 기존 단일 파라미터를 mode로 승격(자동 마이그레이션)
                if (container.Parameters != null || (container.TrainImages?.Count > 0))
                {
                    container.Modes[modeKey] = new PatternMatchingModeRecipeJson
                    {
                        Parameters = container.Parameters?.Clone(),
                        Roi = container.Roi ?? new PatternMatchingRoiJson(),
                        TrainImages = container.TrainImages?.ToList() ?? new List<SerializedTrainImage>()
                    };
                }
                return container;
            }

            return null;
        }
        #endregion

        #region Standard Path Resolver (RecipeName-only)
        // [ADD] 외부( Runner / Control / Hub )에서 "레시피명만" 안전하게 쓰기 위한 공개 정규화 API
        public static string NormalizeRecipeName(string name)
        {
            return NormalizeRecipeBaseName(name);
        }

        /// <summary>
        /// MeasurementRecipe.UseVisionRecipe / VisionRecipePath 를 완전히 무시하고,
        /// RecipeRootDirectory + CameraName + RecipeName 기준으로만 경로를 만든다.
        ///
        /// 결과:
        /// - {Root}\{Camera}\{RecipeName}_Vision.json
        /// - 이미지 폴더: {Root}\{Camera}\{RecipeName}_Vision\
        /// </summary>
        public static string ResolveRecipePath(
            string recipeRootDirectory,
            string cameraName,
            string recipeName,
            bool createDirectoryForSave = true)
        {
            cameraName = string.IsNullOrWhiteSpace(cameraName) ? "NoCamera" : cameraName;
            recipeName = NormalizeRecipeBaseName(recipeName);

            if (string.IsNullOrWhiteSpace(recipeRootDirectory))
            {
                // 기본 경로: {Base}\Recipes\{RecipeName}\PatternMatching
                recipeRootDirectory = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Recipes",
                    NormalizeRecipeBaseName(recipeName),
                    "PatternMatching");
            }

            string dir = Path.Combine(recipeRootDirectory, cameraName);
            string fileName = BuildVisionRecipeFileName(recipeName); // "{RecipeName}_Vision.json"
            string path = Path.Combine(dir, fileName);

            TryEnsureParentDirectory(path, createDirectoryForSave);
            return path;
        }

        private static string BuildVisionRecipeFileName(string recipeBaseName)
        {
            recipeBaseName = NormalizeRecipeBaseName(recipeBaseName);
            return recipeBaseName + "_Vision.json";
        }

        public static string NormalizeRecipeBaseName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "Default";

            name = name.Trim();

            // 입력이 이미 파일명 형태인 경우도 정리
            if (name.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                name = Path.GetFileNameWithoutExtension(name);

            // 과거 포맷(.Vision / _Vision) 들어오면 제거 후 "_Vision"을 1회만 붙이도록
            if (name.EndsWith(".Vision", StringComparison.OrdinalIgnoreCase))
                name = name.Substring(0, name.Length - ".Vision".Length);

            if (name.EndsWith("_Vision", StringComparison.OrdinalIgnoreCase))
                name = name.Substring(0, name.Length - "_Vision".Length);

            return name;
        }

        private static void TryEnsureParentDirectory(string filePath, bool create)
        {
            if (!create) return;
            try
            {
                var dir = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrWhiteSpace(dir))
                    Directory.CreateDirectory(dir);
            }
            catch { }
        }
        #endregion

        #region Helpers
        private static SerializedTrainImage ToSerialized(VisionImage vimg)
        {
            var ret = new SerializedTrainImage();
            try
            {
                var bmp = vimg.GetImage();
                ret.Width = bmp.Width;
                ret.Height = bmp.Height;
                ret.Tag = vimg.Tag?.ToString();

                // 기본 값 (파일 저장 성공하면 제거 가능)
                using (var ms = new MemoryStream())
                {
                    bmp.Save(ms, ImageFormat.Png);
                    ret.ImageBase64 = Convert.ToBase64String(ms.ToArray());
                }
            }
            catch { }
            return ret;
        }

        private static VisionImage FromSerialized(SerializedTrainImage sti)
        {
            if (sti == null || string.IsNullOrEmpty(sti.ImageBase64)) return null;
            try
            {
                byte[] bytes = Convert.FromBase64String(sti.ImageBase64);
                using (var ms = new MemoryStream(bytes))
                using (var bmp = (Bitmap)Image.FromStream(ms))
                {
                    var cloneBmp = new Bitmap(bmp);
                    return VisionImage.CreateInstance(cloneBmp);
                }
            }
            catch { return null; }
        }

        private static string SanitizeFileName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "Train";
            foreach (var c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return name.Length > 40 ? name.Substring(0, 40) : name;
        }
        #endregion
    }
}
