using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common
{
    public abstract class BaseConfig
    {
        // 공통 속성
        public string Name { get; set; }
        public bool IsEnabled { get; set; } = true;
        public DateTime LastModified { get; set; } = DateTime.Now;
        
        // 생성자
        protected BaseConfig(string name = null)
        {
            Name = name ?? GetType().Name;
            Reset();
        }

        public virtual void Reset()
        {
            // 기본값으로 리셋하는 로직
        }
        public virtual bool Validate()
        {
            // Config 값의 유효성을 검사하는 로직
            return true;
        }
        public virtual string GetFilePath()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", $"{Name}.json");
        }

        // 파일 로드 & 저장 (JSON)
        public int Save()
        {
            string filePath = GetFilePath();
            return SaveToFile(filePath);
        }
        public int Load()
        {
            string filePath = GetFilePath();
            if (File.Exists(filePath))
            {
                LoadFromFile(filePath);
                return 0; // Success
            }
            return -1;
        }
        protected int SaveToFile(string filePath)
        {
            try
            {
                string directoryPath = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                // Write the JSON to the file
                var settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto, // 타입 정보 포함
                    ContractResolver = new DefaultContractResolver(),
                    Formatting = Formatting.Indented
                };

                string json = JsonConvert.SerializeObject(this, settings);
                System.IO.File.WriteAllText(filePath, json);
                LastModified = DateTime.Now;
                return 0; // Success
            }
            catch (System.Exception ex)
            {
                Log.Write(ex);
                return -1; // General error
            }
        }
        protected void LoadFromFile(string filePath)
        {
            try
            {
                string json = System.IO.File.ReadAllText(filePath);

                var settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto, // 타입 정보 포함
                    ContractResolver = new DefaultContractResolver()
                };

                // JSON을 역직렬화하여 현재 객체에 복사
                var deserializedObject = JsonConvert.DeserializeObject(json, this.GetType());
                if (deserializedObject != null)
                {
                    foreach (var property in deserializedObject.GetType().GetProperties())
                    {
                        property.SetValue(this, property.GetValue(deserializedObject));
                    }
                }
            }
            catch (System.Exception ex)
            {
                Log.Write(ex);
            }
        }

        //public PropertyCollection propertyBases { get; set; } = new PropertyCollection();
        //[JsonProperty("propertyBases")]         
        //[JsonConverter(typeof(PropertyCollectionJsonConverter))]
        //[JsonIgnore]
        //public PropertyCollection propertyBases { get; set; } = new PropertyCollection();
        [JsonIgnore]
        public PropertyPosition PropertyPosition { get; set; } = new PropertyPosition();

        public virtual PropertyCollection GetPropertyCollection()
        {
            PropertyCollection pc = new PropertyCollection();
            PropertyBase prop = new StringProperty("Name", Name);
            pc.Add(prop);
            return pc;
        }
        public virtual int ApplyValueFromPropertyCollection(PropertyCollection pc)
        {
            // Do this
            return 0;
        }
    }
}
