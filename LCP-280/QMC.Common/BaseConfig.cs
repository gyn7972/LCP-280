using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
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
        }

        // 공통 메서드
        public virtual void Reset()
        {
            // 기본값으로 리셋하는 로직
            LastModified = DateTime.Now;
        }

        public virtual bool Validate()
        {
            // Config 값의 유효성을 검사하는 로직
            return true;
        }

        // 로드, 저장 함수 추가
        public int SaveToFile(string filePath)
        {
            // Ensure the "config" directory exists
            string directoryPath = Path.GetDirectoryName(filePath);
            //if (!Directory.Exists(directoryPath))
            //{
            //    Directory.CreateDirectory(directoryPath);
            //}

            // Combine the file path with the "config" directory
            filePath = Path.Combine("config", filePath);

            return OnSaveToFile(filePath);

        }
        protected virtual int OnSaveToFile(string filePath)
        {
            try
            {
                var settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto, // 타입 정보 포함
                    ContractResolver = new DefaultContractResolver(),
                    Formatting = Formatting.Indented
                };
                string json = JsonConvert.SerializeObject(this, settings);
                System.IO.File.WriteAllText(filePath, json);
                // Write the JSON to the file

                return 0; // Success
            }
            catch (System.Exception ex)
            {
                Log.Write(ex);
                return -1; // General error
            }
        }

        public void LoadFromFile(string filePath)
        {
            filePath = Path.Combine("config", filePath);
            OnLoadConfig(filePath);
        }

        protected virtual void OnLoadConfig(string filePath)
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

        public PropertyCollection propertyBases { get; set; } = new PropertyCollection();
        public PropertyPosition PropertyPosition { get; set; } = new PropertyPosition();

    }
}
