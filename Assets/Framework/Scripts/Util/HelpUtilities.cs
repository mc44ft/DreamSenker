using LitJson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HelpUtilities
{
	/// <summary>
	/// 深拷贝
	/// </summary>
	public static T DeepCopy<T>(T source) where T : class
	{
		//这种方法性能一般 但是对于偶尔的存档加载很好用
		string json = JsonMapper.ToJson(source);
		return JsonMapper.ToObject<T>(json);
	}
}
