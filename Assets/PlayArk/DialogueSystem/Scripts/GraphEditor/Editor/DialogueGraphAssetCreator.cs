using System.IO;
using PlayArk.DialogueSystem.Data;
using UnityEditor;
using UnityEngine;

namespace PlayArk.DialogueSystem.Editor
{
    public static class DialogueGraphAssetCreator
    {
        //覆盖Unity默认的CreateAssetMenu 自己书写创建资源的流程
        //因为本资源比较特殊 需要在创建时添加默认的子资源
        //这样的实现思路 会跳过Unity默认创建资源时的手动命名阶段
        [MenuItem("Assets/Create/PlayArk Assets/GraphCore/DialogueGraph")]
        private static void CreateDialogueGraphAsset()
        {
            string folderPath = GetSelectedFolderPath();
            //这个方法能保证得到一个在当前文件夹下不重名的资产路径 如果重名Unity会自动改名（在后面加_1这种序号）
            string assetPath = AssetDatabase.GenerateUniqueAssetPath(folderPath + "/DialogueGraph_.asset");
            //在内存中实例化这个资产对象
            DialogueGraph graphAsset = ScriptableObject.CreateInstance<DialogueGraph>();
            //将其写入到硬盘里
            AssetDatabase.CreateAsset(graphAsset, assetPath);
            //将图资源对象完整化，主要是添加默认节点
            //这里是在写入硬盘序列化后再添加默认节点，而不是选择更早的在序列化开始之前将图资源对象完整化
            //主要是因为AddObjectToAsset 需要 graph 已经是资产。你最多只能提前把引用关系放内存里，不能提前完成子资源挂载。
            graphAsset.EnsureDefaultNodes();

            //将资源标记为脏 表示其已经被修改
            EditorUtility.SetDirty(graphAsset);
            AssetDatabase.SaveAssets();//强制保存一下
            AssetDatabase.Refresh();//刷新一下

            //更改当前选中焦点
            Selection.activeObject = graphAsset;
        }

        private static string GetSelectedFolderPath()
        {
            //得到当前焦点的对象所处路径
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);

            //判断路径是否有效
            if (string.IsNullOrEmpty(path)) return "Assets";
            //判断路径是否是文件夹
            if (AssetDatabase.IsValidFolder(path)) return path;
            //如果选中的是文件 获取改文件的父级文件夹
            //并将windows风格的反斜杠替换为Unity使用的正斜杠
            return Path.GetDirectoryName(path).Replace("\\", "/");
        }
    }
}
