using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace QMC.Common
{
    public class SaveManager
    {
        // ===== 내부 공통 JSON 직렬화기 =====
        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            // 필요 시 켜세요 (다형/인터페이스 필드가 많다면):
            TypeNameHandling = TypeNameHandling.Auto
        };

        private static bool VerifyFile(string strPath)
        {
            FileInfo fi = new FileInfo(strPath);
            return fi.Exists;
        }

        private static void CreateFileDirectory(string strFilePath)
        {
            int nLastIndex = strFilePath.LastIndexOf("\\", StringComparison.Ordinal);
            string strPath = nLastIndex >= 0 ? strFilePath.Substring(0, nLastIndex) : string.Empty;
            if (!string.IsNullOrEmpty(strPath))
                Directory.CreateDirectory(strPath);
        }

        // ====== 공개 API (호출부 그대로 사용 가능) ======

        // 기존 Load<T>: 내부를 JSON 역직렬화로 변경
        public static void Load<T>(string strFilePath, out T t)
        {
            if (VerifyFile(strFilePath))
            {
                using (FileStream fs = new FileStream(strFilePath, FileMode.Open, FileAccess.Read))
                using (var sr = new StreamReader(fs, Encoding.UTF8, true))
                using (var jr = new JsonTextReader(sr))
                {
                    var serializer = JsonSerializer.Create(JsonSettings);
                    t = serializer.Deserialize<T>(jr);
                }
            }
            else
            {
                t = default(T);
            }
        }

        // 기존 Save(string, object): 내부를 JSON 저장으로 변경
        public static void Save(string strFilePath, object t)
        {
            CreateFileDirectory(strFilePath);
            using (FileStream fs = new FileStream(strFilePath, FileMode.Create, FileAccess.Write))
            using (var sw = new StreamWriter(fs, new UTF8Encoding(false)))
            using (var jw = new JsonTextWriter(sw) { Formatting = Formatting.Indented })
            {
                var serializer = JsonSerializer.Create(JsonSettings);
                serializer.Serialize(jw, t);
            }
        }

        // 기존 Save(string, string, object): 백업 로직 유지 + JSON 저장
        public static void Save(string strFilePath, string strBackupFilePath, object t)
        {
            FileInfo fi = new FileInfo(strFilePath);
            if (fi.Exists)
            {
                CreateFileDirectory(strBackupFilePath);
                // 동일 경로 move 방지
                if (!strBackupFilePath.Equals(strFilePath, StringComparison.OrdinalIgnoreCase))
                    fi.MoveTo(strBackupFilePath);
            }
            else
            {
                CreateFileDirectory(strFilePath);
            }

            Save(strFilePath, t);
        }

        // 기존 LoadRecipe/SaveRecipe는 파일 포맷만 JSON으로 자연 교체
        //public static void LoadRecipe(out RecipeInfoCollection recipes)
        //{
        //    RecipeInfoCollection loadRecipes = new RecipeInfoCollection();
        //    RecipeHeader header = loadRecipes.Header;

        //    Load<RecipeHeader>(ConfigManager.GetRecipeFilePath(), out header);
        //    if (header == null)
        //    {
        //        header = new RecipeHeader();
        //    }

        //    loadRecipes.Header = header;
        //    loadRecipes.Clear();
        //    foreach (string name in header.RecipeList)
        //    {
        //        RecipeInfo recipe;
        //        Load<RecipeInfo>(ConfigManager.GetRecipeFilePath(name), out recipe);
        //        loadRecipes.Add(recipe);
        //    }

        //    recipes = loadRecipes;
        //}

        //public static void SaveRecipe(RecipeInfoCollection recipes)
        //{
        //    string strBackupPath = DateTime.Now.ToString("yyyyMMddHHmmssfff");
        //    Save(ConfigManager.GetRecipeFilePath(),
        //         ConfigManager.GetRecipeBackupFilePath(strBackupPath),
        //         recipes.Header);

        //    foreach (RecipeInfo recipe in recipes)
        //    {
        //        Save(ConfigManager.GetRecipeFilePath(recipe.Name),
        //             ConfigManager.GetRecipeBackupFilePath(recipe.Name, strBackupPath),
        //             recipe);
        //    }
        //}

        // ====== 아래 메서드들은 "이름을 유지"하되 JSON으로 동작하도록 교체 ======

        // BinarySerialize(Stream, object) -> JSON으로 동작 (호출부 변경 최소화)

        public static int BinarySerialize(Stream stream, object t)
        {
            try
            {
                if (stream == null) return -1;
                using (var sw = new StreamWriter(stream, new UTF8Encoding(false), 1024, true))
                using (var jw = new JsonTextWriter(sw) { Formatting = Formatting.Indented })
                {
                    var serializer = JsonSerializer.Create(JsonSettings);
                    serializer.Serialize(jw, t);
                }
                return 0;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return -1;
            }
        }

        // BinaryDeserialize(Stream, out T) -> JSON으로 동작
        public static int BinaryDeserialize<T>(Stream stream, out T t)
        {
            try
            {
                if (stream == null) { t = default(T); return -1; }
                using (var sr = new StreamReader(stream, Encoding.UTF8, true, 1024, true))
                using (var jr = new JsonTextReader(sr))
                {
                    var serializer = JsonSerializer.Create(JsonSettings);
                    t = serializer.Deserialize<T>(jr);
                }
                return 0;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                t = default(T);
                return -1;
            }
        }

        // BinarySerialize(ref byte[], object) -> JSON 텍스트 바이트로
        public static void BinarySerialize(ref byte[] bytes, object graph)
        {
            try
            {
                string json = JsonConvert.SerializeObject(graph, JsonSettings);
                bytes = Encoding.UTF8.GetBytes(json);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                bytes = Array.Empty<byte>();
            }
        }

        // BinaryDeserialize(byte[], out T) -> JSON 텍스트 바이트에서
        public static int BinaryDeserialize<T>(byte[] bytes, out T t)
        {
            try
            {
                if (bytes == null || bytes.Length == 0)
                {
                    t = default(T);
                    return -1;
                }
                string json = Encoding.UTF8.GetString(bytes);
                t = JsonConvert.DeserializeObject<T>(json, JsonSettings);
                return 0;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                t = default(T);
                return -1;
            }
        }
    }
}
