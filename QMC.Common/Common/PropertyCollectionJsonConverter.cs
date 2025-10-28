// QMC.Common.PropertyCollectionJsonConverter.cs  (C# 7.3 호환)
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace QMC.Common
{
    /// <summary>
    /// JSON 배열 <-> QMC.Common.PropertyCollection 변환기
    /// </summary>
    public sealed class PropertyCollectionJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            // 정확한 타입 매칭(네임스페이스/이름)으로 안전하게 한정
            return objectType != null && objectType.FullName == "QMC.Common.PropertyCollection";
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return Activator.CreateInstance(objectType);

            // JSON을 배열로 읽고, Add(T) 메서드의 파라미터 타입으로 각 요소를 변환
            var jarr = JArray.Load(reader);

            var coll = Activator.CreateInstance(objectType);
            var add = objectType.GetMethod("Add", BindingFlags.Instance | BindingFlags.Public);
            if (add == null)
                throw new JsonSerializationException("PropertyCollection에 Add 메서드가 없습니다.");

            var ps = add.GetParameters();
            if (ps.Length != 1)
                throw new JsonSerializationException("PropertyCollection.Add 시그니처가 유효하지 않습니다.");

            var itemType = ps[0].ParameterType; // 예: PropertyBase 또는 파생
            foreach (var token in jarr)
            {
                var item = token.ToObject(itemType, serializer);
                add.Invoke(coll, new[] { item });
            }
            return coll;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            // IEnumerable로 펼쳐 일반 배열로 직렬화 (재귀 방지)
            var list = new List<object>();
            var enumerable = value as IEnumerable;
            if (enumerable != null)
            {
                foreach (var item in enumerable)
                    list.Add(item);
            }
            serializer.Serialize(writer, list);
        }
    }
}
