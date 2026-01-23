using QMC.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using System.Text;

namespace QMC.LCP_280.Process
{
    /// <summary>
    /// 설비 전체 Config 관리자
    /// </summary>
    public class EquipmentConfigManager
    {
        private ConcurrentDictionary<string, BaseConfig> _unitConfigs;
        private readonly object _saveLock = new object();

        public EquipmentConfigManager()
        {
            _unitConfigs = new ConcurrentDictionary<string, BaseConfig>();
        }

        /// <summary>
        /// Unit Config 등록
        /// </summary>
        public void RegisterUnitConfig(string unitName, BaseConfig config)
        {
            if (config != null)
            {
                _unitConfigs[unitName] = config;
                Console.WriteLine($"Unit '{unitName}' Config 등록됨");
            }
        }

        /// <summary>
        /// Unit Config 등록 해제
        /// </summary>
        public bool UnregisterUnitConfig(string unitName)
        {
            return _unitConfigs.TryRemove(unitName, out _);
        }

        /// <summary>
        /// 특정 Unit의 Config 가져오기
        /// </summary>
        public T GetUnitConfig<T>(string unitName) where T : class
        {
            if (_unitConfigs.TryGetValue(unitName, out var config))
            {
                return config as T;
            }
            return null;
        }

        /// <summary>
        /// 특정 Unit의 Config 설정
        /// </summary>
        public void SetUnitConfig(string unitName, BaseConfig config)
        {
            if (config != null)
            {
                _unitConfigs[unitName] = config;
            }
        }

        /// <summary>
        /// 모든 Unit Config 가져오기
        /// </summary>
        public Dictionary<string, BaseConfig> GetAllUnitConfigs()
        {
            return new Dictionary<string, BaseConfig>(_unitConfigs);
        }

        /// <summary>
        /// 모든 Unit Config JSON 저장 (각 Config 의 GetFilePath 사용 또는 지정 폴더).
        /// </summary>
        public bool SaveAllConfigs(string directoryPath = null)
        {
            lock (_saveLock)
            {
                bool allOk = true;
                foreach (var kv in _unitConfigs)
                {
                    var unitName = kv.Key;
                    var cfg = kv.Value;
                    try
                    {
                        string targetPath;
                        if (directoryPath == null)
                        {
                            targetPath = cfg.GetFilePath();
                        }
                        else
                        {
                            Directory.CreateDirectory(directoryPath);
                            var fileName = Path.GetFileName(cfg.GetFilePath());
                            targetPath = Path.Combine(directoryPath, fileName);
                        }

                        if (cfg.Save(targetPath) != 0)
                        {
                            Console.WriteLine($"Unit '{unitName}' Config 저장 실패(rc!=0)");
                            allOk = false;
                        }
                        else
                        {
                            Console.WriteLine($"Unit '{unitName}' Config 저장: {targetPath}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex);
                        allOk = false;
                    }
                }
                return allOk;

            }
        }

        /// <summary>
        /// 모든 Unit Config JSON 로드 (파일 없으면 건너뜀).
        /// directoryPath 지정 시 그 폴더에서 동일 파일명 찾아 로드.
        /// </summary>
        public bool LoadAllConfigs(string directoryPath = null)
        {
            bool allOk = true;
            foreach (var kv in _unitConfigs)
            {
                var unitName = kv.Key;
                var cfg = kv.Value;
                try
                {
                    string sourcePath;
                    if (directoryPath == null)
                    {
                        sourcePath = cfg.GetFilePath();
                    }
                    else
                    {
                        var fileName = Path.GetFileName(cfg.GetFilePath());
                        sourcePath = Path.Combine(directoryPath, fileName);
                    }

                    if (!File.Exists(sourcePath))
                    {
                        // 없으면 스킵
                        continue;
                    }

                    // 임시 로드 방식: cfg.Load() 를 그대로 쓰되 경로가 다르면 수동 디시리얼라이즈
                    if (directoryPath == null)
                    {
                        cfg.Load();
                    }
                    else
                    {
                        var json = File.ReadAllText(sourcePath, Encoding.UTF8);
                        var settings = QMC.Common.ConfigStore.DefaultJsonSettings;
                        var temp = Newtonsoft.Json.JsonConvert.DeserializeObject(json, cfg.GetType(), settings);
                        if (temp != null)
                        {
                            foreach (var p in cfg.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
                            {
                                if (p.CanRead && p.CanWrite)
                                {
                                    p.SetValue(cfg, p.GetValue(temp));
                                }
                            }
                        }
                    }
                    Console.WriteLine($"Unit '{unitName}' Config 로드: {sourcePath}");
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                    allOk = false;
                }
            }
            return allOk;
        }

        /// <summary>
        /// 전역 DryRun 적용 후 전체 저장.
        /// </summary>
        public void ApplyGlobalDryRunAndSave(bool isDryRun, bool save = false)
        {
            BaseConfig.SetGlobalDryRun(isDryRun);
            if (save)
                SaveAllConfigs();
        }

        public void ApplyGlobalSimulationAndSave(bool isDryRun, bool save = false)
        {
            BaseConfig.SetGlobalSimulation(isDryRun);
            
        }


    }

    /// <summary>
    /// 설비 전체 Recipe 관리자
    /// </summary>
    public class EquipmentRecipeManager
    {
        private ConcurrentDictionary<string, BaseRecipe> _unitRecipes;
        private readonly object _saveLock = new object();

        public EquipmentRecipeManager()
        {
            _unitRecipes = new ConcurrentDictionary<string, BaseRecipe>();
        }

        /// <summary>
        /// Unit Recipe 등록
        /// </summary>
        public void RegisterUnitRecipe(string unitName, BaseRecipe recipe)
        {
            if (recipe != null)
            {
                _unitRecipes[unitName] = recipe;
                Console.WriteLine($"Unit '{unitName}' Recipe 등록됨");
            }
        }

        /// <summary>
        /// Unit Recipe 등록 해제
        /// </summary>
        public bool UnregisterUnitRecipe(string unitName)
        {
            return _unitRecipes.TryRemove(unitName, out _);
        }

        /// <summary>
        /// 특정 Unit의 Recipe 가져오기
        /// </summary>
        public T GetUnitRecipe<T>(string unitName) where T : BaseRecipe
        {
            if (_unitRecipes.TryGetValue(unitName, out var recipe))
            {
                return recipe as T;
            }
            return null;
        }

        /// <summary>
        /// 특정 Unit의 Recipe 설정
        /// </summary>
        public void SetUnitRecipe(string unitName, BaseRecipe recipe)
        {
            if (recipe != null)
            {
                _unitRecipes[unitName] = recipe;
            }
        }

        /// <summary>
        /// 모든 Unit Recipe 가져오기
        /// </summary>
        public Dictionary<string, BaseRecipe> GetAllUnitRecipes()
        {
            return new Dictionary<string, BaseRecipe>(_unitRecipes);
        }

        /// <summary>
        /// 모든 Recipe 저장 (XML 형식)
        /// </summary>
        public bool SaveAllRecipes(string directoryPath = null)
        {
            lock (_saveLock)
            {
                try
                {
                    directoryPath = directoryPath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Recipe");
                    
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    bool allSuccess = true;

                    foreach (var kvp in _unitRecipes)
                    {
                        var unitName = kvp.Key;
                        var recipe = kvp.Value;
                        
                        try
                        {
                            var filePath = Path.Combine(directoryPath, $"{unitName}_Recipe.xml");
                            
                            var serializer = new XmlSerializer(recipe.GetType());
                            using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
                            {
                                serializer.Serialize(writer, recipe);
                            }
                            
                            Console.WriteLine($"Unit '{unitName}' Recipe 저장됨: {filePath}");
                        }
                        catch (Exception ex)
                        {
                            Log.Write(ex);
                            allSuccess = false;
                        }
                    }

                    return allSuccess;
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                    return false;
                }
            }
        }

        /// <summary>
        /// 모든 Recipe 로드 (XML 형식)
        /// </summary>
        public bool LoadAllRecipes(string directoryPath = null)
        {
            try
            {
                directoryPath = directoryPath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Recipe");
                
                if (!Directory.Exists(directoryPath))
                {
                    Console.WriteLine($"Recipe 디렉토리가 없습니다: {directoryPath}");
                    return false;
                }

                var recipeFiles = Directory.GetFiles(directoryPath, "*_Recipe.xml");
                bool allSuccess = true;

                foreach (var filePath in recipeFiles)
                {
                    try
                    {
                        var fileName = Path.GetFileNameWithoutExtension(filePath);
                        var unitName = fileName.Replace("_Recipe", "");
                        
                        // 현재 등록된 Recipe 타입을 기반으로 역직렬화
                        if (_unitRecipes.TryGetValue(unitName, out var existingRecipe))
                        {
                            var serializer = new XmlSerializer(existingRecipe.GetType());
                            using (var reader = new StreamReader(filePath))
                            {
                                var loadedRecipe = serializer.Deserialize(reader);
                                if (loadedRecipe is BaseRecipe baseRecipe)
                                {
                                    _unitRecipes[unitName] = baseRecipe;
                                    Console.WriteLine($"Unit '{unitName}' Recipe 로드됨: {filePath}");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex);
                        allSuccess = false;
                    }
                }

                return allSuccess;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return false;
            }
        }
    }

    /// <summary>
    /// 기본 Recipe 클래스
    /// </summary>
    public abstract class BaseRecipe
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModified { get; set; }
        public string CreatedBy { get; set; }

        protected BaseRecipe() : this(null) { }

        protected BaseRecipe(string name)
        {
            Name = name ?? GetType().Name;
            CreatedDate = DateTime.Now;
            LastModified = DateTime.Now;
            CreatedBy = Environment.UserName;
        }

        public virtual bool Validate()
        {
            return !string.IsNullOrEmpty(Name);
        }

        public virtual void Reset()
        {
            LastModified = DateTime.Now;
        }
    }

    /// <summary>
    /// 기본 Unit Recipe
    /// </summary>
    public class DefaultUnitRecipe : BaseRecipe
    {
        public Dictionary<string, string> Parameters { get; set; }

        public DefaultUnitRecipe() : this(null) { }

        public DefaultUnitRecipe(string unitName) : base($"DefaultRecipe_{unitName}")
        {
            Parameters = new Dictionary<string, string>();
        }
    }

    /// <summary>
    /// CassetteElevator 전용 Recipe
    /// </summary>
    public class CassetteElevatorRecipe : BaseRecipe
    {
        public double ReadyPosition { get; set; } = 0.0;
        public double LoadingPosition { get; set; } = 10.0;
        public double UnloadingPosition { get; set; } = 20.0;
        public double ScanningPosition { get; set; } = 15.0;
        public double MoveSpeed { get; set; } = 100.0;
        public double Acceleration { get; set; } = 200.0;
        public int SettlingTime { get; set; } = 500;

        public CassetteElevatorRecipe() : base("CassetteElevatorRecipe")
        {
        }

        public override bool Validate()
        {
            if (LoadingPosition < ReadyPosition)
                return false;
                
            if (UnloadingPosition < LoadingPosition)
                return false;
                
            if (MoveSpeed <= 0 || Acceleration <= 0)
                return false;
                
            return base.Validate();
        }
    }
}