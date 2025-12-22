using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
namespace PlayArk.GraphCore.Editor
{
    public class GraphCoreEditor : EditorWindow
    {
        private GraphCoreView _view;
        private void CreateGUI()
        {
            //得到主容器
            VisualElement root = rootVisualElement;

            VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(GetPath() + "GraphCoreEditor.uxml");

            visualTree.CloneTree(root);
            _view = root.Q<GraphCoreView>(); 

        }
        /// <summary>
        /// 得到该脚本所在的上级文件夹的路径
        /// </summary>
        /// <returns></returns>
        public static string GetPath()
        {
            //在工程中全局查找 叫StateMachineEditor 且类型为Script的文件 也就是这个脚本 的guid
            //这样保证了该脚本无论放在工程中的哪个地方 都可以被准确找到
            string[] guids = AssetDatabase.FindAssets("GraphCoreEditor t:Script");

            if(guids.Length > 0)
            {
                //将找到的资源guid 转换为Asset的相对路径
                string scriptPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                //GetDirectoryName方法去掉资源名称 返回上级文件夹路径
                //将Windows使用的反斜杠转换为Unity使用的正斜杠
                //这里的一个误区：AssetDatabase.GUIDToAssetPath返回的就是正斜杠的路径
                //只是在用System.IO.Path.GetDirectoryName这个方法的时候被污染了
                return System.IO.Path.GetDirectoryName(scriptPath).Replace("\\", "/") + "/";
            }
            return "Assets/";
        }
        [MenuItem("PlayArk/Tools/GraphCore")]
        private static void ShowWindow()
        {
            //查找Unity编辑器中所有打开的窗口
            //如果有窗口的类型为 MapNodeWindow 就将其显示在最前面 并赋予其焦点
            //如果没有该窗口 则打开一个新的该类型的编辑器窗口 并将其命名为传入的参数
            //bool参数：决定了该窗口是一个可以停靠、随意拖拽的大窗口（false）
            //还是一个始终悬浮在最前面点击空白区域立即消失的小工具窗口（true）
            GetWindow<GraphCoreEditor>(false, "GraphCore");
        }
    }
}

