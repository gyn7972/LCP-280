using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace QMC.Common
{
    public static class DefaultValueApplier
    {
        /// <summary>
        /// [DefaultValue]가 붙은 public get/set 프로퍼티에 대해,
        /// 값이 비어 있으면(default(T) 또는 null) 기본값을 주입한다.
        /// recurse=true면 하위 참조 타입(문자열 제외)도 재귀 처리.
        /// </summary>
        public static bool Apply(object target, bool recurse = false, int maxDepth = 4)
        {
            if (target == null) return false;
            return ApplyCore(target, recurse, maxDepth, 0);
        }

        private static bool ApplyCore(object target, bool recurse, int maxDepth, int depth)
        {
            bool changed = false;
            var t = target.GetType();

            foreach (var p in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                // 0) 인덱서/읽기전용/쓰기불가 프로퍼티는 스킵
                if (!p.CanRead || !p.CanWrite) continue;
                if (p.GetIndexParameters().Length != 0) continue;      // ★ 인덱서 방지

                object cur;
                try
                {
                    cur = p.GetValue(target, null);                    // C# 7.3: 명시적 null 인자
                }
                catch
                {
                    // 접근 불가/예외 발생 프로퍼티는 스킵
                    continue;
                }

                // 1) [DefaultValue] 적용
                var dv = (DefaultValueAttribute)p.GetCustomAttribute(typeof(DefaultValueAttribute), inherit: false);
                if (dv != null && IsMissing(p.PropertyType, cur))
                {
                    try
                    {
                        var val = Coerce(dv.Value, p.PropertyType);
                        p.SetValue(target, val, null);
                        changed = true;
                        cur = val; // 재귀에서 사용
                    }
                    catch
                    {
                        // 세팅 실패 시 무시
                    }
                }

                // 2) 재귀 처리(하위 복합객체)
                if (recurse && depth < maxDepth && cur != null)
                {
                    var pt = p.PropertyType;

                    // 문자열 제외, 컬렉션(IEnumerator/IEnumerable) 제외
                    if (!IsSimple(pt) &&
                        !(typeof(System.Collections.IEnumerable).IsAssignableFrom(pt) && pt != typeof(string)))
                    {
                        changed |= ApplyCore(cur, recurse, maxDepth, depth + 1);
                    }
                }
            }

            return changed;
        }


        private static bool IsMissing(Type type, object value)
        {
            if (value == null) return true;
            if (!type.IsValueType) return false; // ref-type은 null만 결핍
            object def = Activator.CreateInstance(type);
            return value.Equals(def);
        }

        private static object Coerce(object value, Type targetType)
        {
            if (value == null) return null;
            if (targetType.IsAssignableFrom(value.GetType())) return value;

            if (targetType.IsEnum && value is IConvertible)
                return Enum.ToObject(targetType, Convert.ToInt32(value));

            return Convert.ChangeType(value, targetType);
        }

        private static bool IsSimple(Type t)
        {
            if (t.IsPrimitive) return true;
            if (t.IsEnum) return true;
            if (t == typeof(string)) return true;
            if (t == typeof(decimal) || t == typeof(DateTime) || t == typeof(Guid) || t == typeof(TimeSpan))
                return true;
            return false;
        }
    }
}
