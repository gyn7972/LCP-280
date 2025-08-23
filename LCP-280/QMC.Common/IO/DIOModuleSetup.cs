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
    /// <summary>DIO Module 1개(좌측 리스트 한 항목). 내부에 DI/DO 채널 배열 보관.</summary>
    [Serializable]
    public sealed class DIOModuleSetup
    {
        // 좌측 리스트 표시용
        [Category("Module"), DisplayName("Module Name")]
        public string ModuleName { get; set; } = "DIO Module1";

        [Category("Module"), DisplayName("Model")]
        public string Model { get; set; } = "DB64R";

        [Category("Module"), DisplayName("Board No")]
        public int BoardNo { get; set; } = 0;

        [Category("Module"), DisplayName("Description")]
        public string Description { get; set; } = "";

        // 채널 목록
        [Browsable(false)]
        public List<DIOChannel> Inputs { get; set; } = new List<DIOChannel>();

        [Browsable(false)]
        public List<DIOChannel> Outputs { get; set; } = new List<DIOChannel>();

        // 기본 생성기 (X00~ / Y00~ 자동 생성)
        public static DIOModuleSetup CreateSimple(string moduleName, string model, int boardNo,
                                                  int inputCount, int outputCount,
                                                  string diPrefix, string doPrefix)
        {
            var m = new DIOModuleSetup();
            m.ModuleName = moduleName;
            m.Model = model;
            m.BoardNo = boardNo;

            for (int i = 0; i < inputCount; i++)
            {
                var disp = diPrefix + i.ToString("00");
                m.Inputs.Add(DIOChannel.Create(false, i, disp, disp + " Item Name"));
            }
            for (int i = 0; i < outputCount; i++)
            {
                var disp = doPrefix + i.ToString("00");
                m.Outputs.Add(DIOChannel.Create(true, i, disp, disp + " Item Name"));
            }
            return m;
        }

        // JSON 저장/로드 (Setup만)
        private static JsonSerializerSettings Settings
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
            return JsonConvert.SerializeObject(this,
                indented ? Formatting.Indented : Formatting.None, Settings);
        }

        public void Save(string filePath, bool indented = true)
        {
            File.WriteAllText(filePath, ToJson(indented), Encoding.UTF8);
        }

        public static DIOModuleSetup FromJson(string json)
        {
            var o = JsonConvert.DeserializeObject<DIOModuleSetup>(json, Settings);
            if (o == null) throw new InvalidDataException("Invalid JSON for DIOModuleSetup.");
            return o;
        }

        public static DIOModuleSetup Load(string filePath)
        {
            return FromJson(File.ReadAllText(filePath, Encoding.UTF8));
        }

        public static bool TrySave(string filePath, DIOModuleSetup module, out string error, bool indented = true)
        {
            error = null;
            try { module.Save(filePath, indented); return true; }
            catch (Exception ex) { error = ex.Message; return false; }
        }

        public static bool TryLoad(string filePath, out DIOModuleSetup module, out string error)
        {
            module = null; error = null;
            try { module = Load(filePath); return true; }
            catch (Exception ex) { error = ex.Message; return false; }
        }
    }
}
