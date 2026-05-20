using System.IO;
using UnityEditor;
using UnityEngine;

namespace PlayArk.StateMachine.Editor
{
    public static class StateMachineAssetCreator
    {
        //覆盖Unity默认的CreateAssetMenu，避免在序列化回调中创建默认状态子资源。
        [MenuItem("Assets/Create/PlayArk Assets/GraphCore/State Machine")]
        private static void CreateStateMachineAsset()
        {
            string folderPath = GetSelectedFolderPath();
            //生成当前文件夹下不重名的状态机资源路径。
            string assetPath = AssetDatabase.GenerateUniqueAssetPath(folderPath + "/StateMachine_.asset");
            //在内存中实例化状态机资源对象。
            StateMachine stateMachineAsset = ScriptableObject.CreateInstance<StateMachine>();
            //先创建主资源，保证后续默认状态可以作为子资源挂载。
            AssetDatabase.CreateAsset(stateMachineAsset, assetPath);
            //补齐Entry和Any默认状态，并把它们挂到状态机资产下。
            stateMachineAsset.EnsureDefaultNodes();

            //标记并保存资源，确保新建状态机立即拥有完整的子资源结构。
            EditorUtility.SetDirty(stateMachineAsset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            //创建完成后选中新资源，方便继续编辑。
            Selection.activeObject = stateMachineAsset;
        }

        private static string GetSelectedFolderPath()
        {
            //得到当前Project选中对象对应的资源路径。
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);

            //未选中资源时默认创建到Assets目录。
            if (string.IsNullOrEmpty(path)) return "Assets";
            //选中的是文件夹时，直接把新资源创建在该文件夹下。
            if (AssetDatabase.IsValidFolder(path)) return path;
            //选中的是文件时，使用该文件所在的父级文件夹。
            return Path.GetDirectoryName(path).Replace("\\", "/");
        }
    }
}
