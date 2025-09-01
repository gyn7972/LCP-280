using System;
using System.Reflection;


namespace QMC.Common
{
	/// <summary>
	/// 형식에 관련된 유용한 함수들을 제공합니다.
	/// </summary>
	public static class TypeUtility
	{
		/// <summary>
		/// 지정된 형식의 개체를 생성하고 지정된 소스의 필드들을 복사합니다.
		/// </summary>
		/// <param name="targetType">생성하려는 인스턴스의 형식입니다.</param>
		/// <param name="source">필드를 복사할 소스 개체입니다.</param>
		/// <returns>새로 생성된 대상 형식의 개체입니다.</returns>
		public static object CreateInstanceFrom(Type targetType, object source)
		{
			if (targetType == null)
			{
				throw new ArgumentNullException();
			}
			object obj = Activator.CreateInstance(targetType);
			if (source == null)
			{
				return obj;
			}
			FieldInfo[] instanceFields = GetInstanceFields(source.GetType());
			FieldInfo[] instanceFields2 = GetInstanceFields(targetType);
			FieldInfo[] array = instanceFields;
			foreach (FieldInfo fieldInfo in array)
			{
				FieldInfo[] array2 = instanceFields2;
				int num = 0;
				while (true)
				{
					if (num < array2.Length)
					{
						FieldInfo fieldInfo2 = array2[num];
						if (fieldInfo == fieldInfo2)
						{
							fieldInfo2.SetValue(obj, fieldInfo.GetValue(source));
							break;
						}
						num++;
						continue;
					}
				}
			}
			return obj;
		}

		/// <summary>
		/// Base Type Instance를 이용해서 Target Type Instance를 생성한다. Base Type Instance가 null이면 Target Type에 대한 Default Instance가 반환된다.
		/// </summary>
		/// <param name="targetType">생성하려는 인스턴스의 형식입니다.</param>
		/// <param name="baseTypeObject">인스턴스를 생성하려는 대상 형식의 상위 형식의 인스턴스입니다.</param>
		/// <returns>새로 생성된 대상 형식의 개체입니다.</returns>
		public static object CreateInstanceByBaseTypeInstance(Type targetType, object baseTypeObject)
		{
			Type type = null;
			if (targetType == null)
			{
				throw new ArgumentNullException();
			}
			if (baseTypeObject == null)
			{
				Activator.CreateInstance(targetType);
			}
			type = baseTypeObject.GetType();
			if (!targetType.IsSubclassOf(type))
			{
				string message = string.Format("{0} : {1}", targetType.FullName, type.FullName);
				throw new InvalidOperationException(message);
			}
			return CreateInstanceFrom(targetType, baseTypeObject);
		}

		/// <summary>
		/// 지정된 대상 형식의 개체를 생성하고 지정된 상위 형식 개체의 필드들을 복사합니다.
		/// </summary>
		/// <typeparam name="T">생성하려는 인스턴스의 형식입니다.</typeparam>
		/// <param name="baseTypeObject">인스턴스를 생성하려는 대상 형식의 상위 형식의 인스턴스입니다.</param>
		/// <returns>새로 생성된 대상 형식의 개체입니다.</returns>
		public static T CreateInstanceByBaseTypeInstance<T>(object baseTypeObject)
		{
			return Cast<T>(CreateInstanceByBaseTypeInstance(typeof(T), baseTypeObject));
		}

		/// <summary>
		/// 주어진 Type에 정의된 모든 Instance Field 정보를 Access Modifier에 상관없이 검색한다.
		/// includeBases가 true이면 Base Type들의 Instance Field 정보가 모두 포함되고, 
		/// includeBases가 false이면 주어진 Type에 정의된 Instance Field 정보만 검색한다.
		/// </summary>
		/// <param name="type">대상 형식을 지정합니다.</param>
		/// <param name="includeBases">상위 형식에 정의된 필드를 검색할지 여부를 지정합니다.</param>
		/// <returns>검색된 인스턴스 필드 정보의 배열입니다.</returns>
		public static FieldInfo[] GetInstanceFields(Type type, bool includeBases)
		{
			FieldInfo[] result = new FieldInfo[0];
			BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField;
			if (type == null)
			{
				return result;
			}
			FieldInfo[] fields = type.GetFields(bindingAttr);
			FieldInfo[] array = new FieldInfo[0];
			if (includeBases && type.BaseType != null)
			{
				array = GetInstanceFields(type.BaseType, includeBases);
			}
			result = new FieldInfo[fields.Length + array.Length];
			Array.Copy(fields, result, fields.Length);
			Array.Copy(array, 0, result, fields.Length, array.Length);
			return result;
		}

		/// <summary>
		/// 주어진 Type에 정의된 모든 Instance Field 정보를 Access Modifier에 상관없이 검색한다.
		/// Base Type이 존재하면 Base Type들의 Instance Field 정보가 모두 포함된다.
		/// </summary>
		/// <param name="type">대상 형식을 지정합니다.</param>
		/// <returns>검색된 인스턴스 필드 정보의 배열입니다.</returns>
		public static FieldInfo[] GetInstanceFields(Type type)
		{
			return GetInstanceFields(type, includeBases: true);
		}

		/// <summary>
		/// 주어진 Name을 가진 PropertyInfo를 찾아서 반환한다. Name의 중복이 있는 경우 가장 하위 Class에 선언된 Property를 반환한다.
		/// 참고로, Name의 중복은 같은 이름의 Property가 서로 다른 Return Type으로 선언되어 있을 때 발생한다.
		/// </summary>
		/// <param name="type">대상 형식을 지정합니다.</param>
		/// <param name="name">검색할 속성의 이름입니다.</param>
		/// <returns>검색된 속성 정보입니다.</returns>
		public static PropertyInfo GetProperty(Type type, string name)
		{
			PropertyInfo[] properties = type.GetProperties();
			PropertyInfo[] array = properties;
			PropertyInfo result = null;
			foreach (PropertyInfo propertyInfo in array)
			{
				if (propertyInfo.Name == name)
				{
					result = propertyInfo;
				}
			}
			return result;
		}

		/// <summary>
		/// 주어진 Object를 T 형식으로 Casting한다.
		/// </summary>
		/// <typeparam name="T">대상 형식을 지정합니다.</typeparam>
		/// <param name="value">소스 개체를 지정합니다.</param>
		/// <returns>Casting된 개체입니다.</returns>
		public static T Cast<T>(object value)
		{
			return (T)value;
		}

		/// <summary>
		/// 지정된 형식이 크기를 산출할 수 있는 형식인지 검사합니다.
		/// </summary>
		/// <param name="type">대상 형식을 지정합니다.</param>
		/// <returns>대상 형식이 크기를 산출할 수 있는 형식이면 true이고, 그렇지 않으면 false입니다.</returns>
		public static bool SizeCalculatable(Type type)
		{
			if (type == null)
			{
				return false;
			}
			int result;
			if (!type.IsExplicitLayout)
			{
				result = (type.IsLayoutSequential ? 1 : 0);
			}
			else
			{
				result = 1;
			}
			return (byte)result != 0;
		}
	}
}
