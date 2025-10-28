using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;


namespace QMC.Common
{
	/// <summary>
	/// Copy에 관련된 유용한 함수들을 제공한다.
	/// </summary>
	public static class CopyUtility
	{
		/// <summary>
		/// Source Object에 대한 Deep Copy본을 반환한다. Source Object는 Serializable이어야 한다.
		/// </summary>
		/// <param name="source">Source Object를 지정한다.</param>
		/// <returns>Source Object에 대한 Deep Copy본을 반환한다.</returns>
		public static object GetDeepCopy(object source)
		{
			object result = null;
			if (source == null)
			{
				return result;
			}
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			MemoryStream memoryStream = new MemoryStream();
			try
			{
				binaryFormatter.Serialize(memoryStream, source);
				byte[] buffer = memoryStream.ToArray();
				MemoryStream memoryStream2 = new MemoryStream(buffer);
				try
				{
					result = binaryFormatter.Deserialize(memoryStream2);
				}
				finally
				{
					if (memoryStream2 != null)
					{
						((IDisposable)memoryStream2).Dispose();
					}
				}
			}
			finally
			{
				if (memoryStream != null)
				{
					((IDisposable)memoryStream).Dispose();
				}
			}
			return result;
		}

		/// <summary>
		/// 지정된 개체에 대한 DeepCopy본을 가져온 다음 지정된 형식으로 반환합니다.
		/// </summary>
		/// <typeparam name="T">반환할 형식을 지정합니다.</typeparam>
		/// <param name="source">복사할 대상 개체입니다.</param>
		/// <returns>복사된 새로운 개체입니다.</returns>
		public static T GetDeepCopy<T>(object source)
		{
			return (T)GetDeepCopy(source);
		}

		/// <summary>
		/// 소스 개체의 인스턴스 필드들을 대상 개체에 복사합니다.
		/// </summary>
		/// <param name="source">인스턴스 필드를 복사할 소스 개체입니다.</param>
		/// <param name="target">인스턴스 필드를 복사할 대상 개체입니다.</param>
		/// <param name="deep">필드의 DeepCopy본을 복사할 지 여부를 지정합니다.</param>
		public static void CopyFields(object source, object target, bool deep)
		{
			FieldInfo[] array = null;
			FieldInfo[] array2 = null;
			FieldInfo fieldInfo = null;
			FieldInfo fieldInfo2 = null;
			object obj = null;
			if (source == null)
			{
				throw new ArgumentNullException();
			}
			if (target == null)
			{
				throw new ArgumentNullException();
			}
			if (source == target)
			{
				throw new ArgumentException();
			}
			array = TypeUtility.GetInstanceFields(source.GetType());
			array2 = TypeUtility.GetInstanceFields(target.GetType());
			for (int i = 0; i < array.Length; i++)
			{
				fieldInfo = array[i];
				for (int j = 0; j < array2.Length; j++)
				{
					fieldInfo2 = array2[j];
					if (!(fieldInfo == fieldInfo2))
					{
						continue;
					}
					obj = fieldInfo.GetValue(source);
					if (deep)
					{
						obj = GetDeepCopy(obj);
					}
					fieldInfo2.SetValue(target, obj);
					break;
				}
			}
		}
	}
}
