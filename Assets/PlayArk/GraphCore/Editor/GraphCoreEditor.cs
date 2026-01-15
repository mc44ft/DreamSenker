using PlayArk.GraphCore.Data;
using PlayArk.StateMachine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine.UIElements;
namespace PlayArk.GraphCore.Editor
{
    /// <summary>
    /// MVVM架构
    /// 担任了VM层的角色
    /// </summary>
    public abstract class GraphCoreEditor : EditorWindow
    {
        protected GraphCoreView _view;
        private void CreateGUI()
        {
            //得到主容器
            VisualElement root = rootVisualElement;

            //通过路径加载这个uxml资源
            //uxml资源对应的类就是VisualTreeAsset
            VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(GetPath() + "GraphCoreEditor.uxml");
            //Unity读取 UXML 里的层级结构，
            //把里面定义的按钮、容器等一个个创建出来，
            //并作为子物体塞进 root 里面。
            visualTree.CloneTree(root);

            //准备一个动态的容器来动态塞入GraphView
            VisualElement viewContainer = root.Q<VisualElement>("view-container");

            _view = CreateView();
            _view.style.flexGrow = 1;
            viewContainer.Add(_view);
            //保证新创建的资源能及时绑定
            OnSelectionChange();

        }
        //提供虚方法由子类决定要显示的画布
        protected abstract GraphCoreView CreateView();
        /// <summary>
        /// 得到该脚本所在的上级文件夹的路径
        /// </summary>
        /// <returns></returns>
        public static string GetPath()
        {
            //在工程中全局查找 叫GraphCoreEditor 且类型为Script的文件 也就是这个脚本 的guid
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
        [OnOpenAsset(10)]//优先级低于子类
        private static bool OnGraphCoreOpened(int instanceID)
        {
            //这个方法 返回true表示不执行后面的方法 返回false表示执行后面的方法
            if(EditorUtility.InstanceIDToObject(instanceID) is GraphCoreGraph)
            {
                //查找Unity编辑器中所有打开的窗口
                //如果有窗口的类型为 MapNodeWindow 就将其显示在最前面 并赋予其焦点
                //如果没有该窗口 则打开一个新的该类型的编辑器窗口 并将其命名为传入的参数
                //bool参数：决定了该窗口是一个可以停靠、随意拖拽的大窗口（false）
                //还是一个始终悬浮在最前面点击空白区域立即消失的小工具窗口（true）
                GetWindow<GraphCoreEditor>(false, "GraphCore");
                return true;
            }
            return false;
        }
        //private void Update()
        //{
        //    //未手动保存时 出现*号
        //    var currentGraph = Selection.activeObject as GraphCoreGraph;
        //    if(currentGraph != null)
        //    {
        //        this.hasUnsavedChanges = EditorUtility.IsDirty(currentGraph);
        //    }

        //}
        /// <summary>
        /// 在Project窗口中切换资源时调用
        /// 在Hierarchy窗口中切换游戏对象时调用（运行时和非运行时都会响应）
        /// 这两个窗口中聚焦的对象是唯一的
        /// </summary>
        protected virtual void OnSelectionChange()
        {
            //activeObject 涵盖了所有资产 SO、材质、贴图、预制体、场景中的游戏对象 等 都囊括在内
            //activeGameObject 只包括场景上的物体 和 Project中的预制体文件
            GraphCoreGraph graphCore = Selection.activeObject as GraphCoreGraph;
            
            
            if(graphCore != null)
            {
                _view.Refresh(graphCore);
            }
        }
    }
}

