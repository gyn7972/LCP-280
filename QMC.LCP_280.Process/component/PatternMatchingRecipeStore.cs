using System;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Drawing.Imaging;
using Newtonsoft.Json;
using QMC.Common;
using QMC.Common.VisionPart;
using QMC.Common.Vision;

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
        public string FilePath { get; set; }    // v1.2: recipe 폴더 기준 상대경로 (예: Default_Images/0_Train0.png)
    }

    /// <summary>
    /// Pattern Matching 전체 파라미터 + ROI를 JSON으로 저장/로드하는 컨테이너
    /// </summary>
    [Serializable]
    public class PatternMatchingRecipeJson
    {
        public string Version { get; set; } = "1.2"; // 1.2 : TrainImage 외부 PNG 분리 저장
        public DateTime SavedAt { get; set; } = DateTime.Now;
        public MultiPatternMatchingParameters Parameters { get; set; }
        public PatternMatchingRoiJson Roi { get; set; } = new PatternMatchingRoiJson();
        public string LastCameraName { get; set; }
        public List<SerializedTrainImage> TrainImages { get; set; } = new List<SerializedTrainImage>();
    }

    internal static class PatternMatchingRecipeStore
    {
        private static JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Include,
            ObjectCreationHandling = ObjectCreationHandling.Replace
        };

        #region Public API
        public static void Save(string filePath, PatternMatchingRecipeJson data)
        {
            if (data != null && data.Parameters != null)
            {
                data.TrainImages.Clear();
                try
                {
                    if (data.Parameters.TrainImages != null)
                    {
                        // 이미지 저장 폴더 (카메라별/레시피별)
                        string recipeDir = Path.GetDirectoryName(filePath);
                        string recipeName = Path.GetFileNameWithoutExtension(filePath); // ex) Default
                        string imageDirName = recipeName + "_Images"; // ex) Default_Images
                        string imageDir = Path.Combine(recipeDir, imageDirName);

                        if (Directory.Exists(imageDir))
                        {
                            // 기존 파일 정리 (오래된 TrainImages 제거)
                            try { Directory.Delete(imageDir, true); } catch { }
                        }
                        Directory.CreateDirectory(imageDir);

                        for (int i = 0; i < data.Parameters.TrainImages.Count; i++)
                        {
                            var v = data.Parameters.TrainImages[i];
                            if (v == null || v.GetImage() == null) continue;
                            var serialized = ToSerialized(v);

                            // 파일명: index_tag.png (tag에 파일명 부적합 문자 제거)
                            string safeTag = SanitizeFileName(serialized.Tag ?? $"Train{i}");
                            string fileName = $"{i}_{safeTag}.png";
                            string saveFullPath = Path.Combine(imageDir, fileName);
                            try
                            {
                                var bmp = v.GetImage();
                                bmp.Save(saveFullPath, ImageFormat.Png);
                                serialized.FilePath = Path.Combine(imageDirName, fileName).Replace('\\', '/'); // JSON 저장용 상대경로
                                serialized.ImageBase64 = null; // 외부 파일 우선, Base64 제거
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
            }

            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
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
                if (container != null)
                {
                    // 1.2 이상 외부 파일 기반 역직렬화
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
                            {
                                img = FromSerialized(ti);
                            }
                            if (img != null)
                            {
                                img.Tag = ti.Tag;
                                container.Parameters.TrainImages.Add(img);
                            }
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
        #endregion

        #region Helpers
        private static SerializedTrainImage ToSerialized(VisionImage vimg)
        {
            SerializedTrainImage ret = new SerializedTrainImage();
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
