using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class AddressableCacheMenu
{
    //参数2:在此选项是一个可以开关的选项时开启
    //参数3:菜单排序优先级
    [MenuItem("Tools/Addressable/ClearCache", false, 100)]
    private static void ClearEditorAddressableCache()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.Log("Clear Editor Addressable cache Failed! Exit PlayMode Please");
        }
        //二次确认弹窗
        if (!EditorUtility.DisplayDialog(
                "Clear Addressable cache",
                "Are you sure?",
                "Sure",
                "Cancel"))
        {
            return;
        }

        string catalogCachePath = Path.Combine(Application.persistentDataPath, "com.unity.addressables");
        if (Directory.Exists(catalogCachePath))
        {
            Directory.Delete(catalogCachePath, true);
        }
    }
}
