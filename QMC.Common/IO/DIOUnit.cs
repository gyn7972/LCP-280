using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common.IO
{
    /// <summary>한 유닛의 DIO 전체 Setup (좌측 모듈 리스트가 여러 개일 때)</summary>
    [Serializable]
    public sealed class DIOUnit
    {
        [Category("Unit"), DisplayName("Unit Name")]
        public string UnitName { get; set; } = "CassetteLoadingElevator";

        [Browsable(false)]
        public List<DIOModuleSetup> Modules { get; set; } = new List<DIOModuleSetup>();

        private static JsonSerializerSettings Settings
        {
            get
            {
                return new JsonSerializerSettings
                {
                    Formatting = Formatting.None,
                    NullValueHandling = NullValueHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.Include,
                    ObjectCreationHandling = ObjectCreationHandling.Replace
                };
            }
        }

        public string ToJson(bool indented)
        {
            return JsonConvert.SerializeObject(this,
                indented ? Formatting.Indented : Formatting.None, Settings);
        }

        public void Save(string filePath, bool indented = true)
        {
            File.WriteAllText(filePath, ToJson(indented), Encoding.UTF8);
        }

        public static DIOUnit FromJson(string json)
        {
            var o = JsonConvert.DeserializeObject<DIOUnit>(json, Settings);
            if (o == null) throw new InvalidDataException("Invalid JSON for UnitDIOSetup.");
            return o;
        }

        public static DIOUnit Load(string filePath)
        {
            return FromJson(File.ReadAllText(filePath, Encoding.UTF8));
        }

        /// <summary>파일이 없으면 기본 템플릿 생성(X00~X16 / Y00~Y16 두 모듈)</summary>
        public static DIOUnit LoadOrCreateDefault(string filePath, string unitName,
                                                       int inputCountPerModule, int outputCountPerModule,
                                                       string model)
        {
            if (File.Exists(filePath)) 
                return Load(filePath);

            var u = new DIOUnit();
            u.UnitName = unitName;
            u.Modules.Add(DIOModuleSetup.CreateSimple("DIO Module1", model, 0, inputCountPerModule, outputCountPerModule, "X", "Y"));
            u.Modules.Add(DIOModuleSetup.CreateSimple("DIO Module2", model, 0, inputCountPerModule, outputCountPerModule, "X", "Y"));
            u.Save(filePath, true);
            return u;
        }
    }
}
