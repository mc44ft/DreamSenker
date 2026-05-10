using UnityEditor;

using DreamSenker.MapSystem;
using DreamSenker.MapSystem.Data;

namespace DreamSenker.MapSystem.Editor
{
[CustomEditor(typeof(MapDefinitionSO))]
public class MapDefinitionSOEditor : UnityEditor.Editor
{
    //重写Inspector绘制函数 Unity每次需要刷新Inspector时会调用它
    public override void OnInspectorGUI()
    {
        //target是当前选中的资产对象的实例
        //serializedObject是为target封装的一层Unity序列化编辑器接口
        
        //这句代码的意思是将target的序列化数据刷新到serializedObject中 供下面调用
        serializedObject.Update();
        
        //禁用区域，在这里面的控件会变灰 不能被编辑（只读不改）
        //using的意思是绑定一个可释放对象 离开花括号后，调用这个可释放对象的Dispose()方法
        //可释放对象就是实现IDisposable接口的类对象
        
        //EditorGUI.DisabledScope(true)是Unity做好的一个“作用域对象”，实现了IDisposable接口
        //它创建时 会把GUI设为禁用 它执行Dispose方法 会恢复之前的GUI状态 所以在这个区域绘制的控件都不可编辑
        //相当于这样简单明了的写法
        //bool oldEnabled = GUI.enabled;
        //GUI.enabled = false;
        //EditorGUILayout.PropertyField(serializedObject.FindProperty("<MapId>k__BackingField"));
        //GUI.enabled = oldEnabled;
        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("<MapId>k__BackingField"));
        }

        EditorGUILayout.PropertyField(serializedObject.FindProperty("<SceneName>k__BackingField"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("<PlayerShadowDarknessStrength>k__BackingField"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("<FollowCameraOrthoSize>k__BackingField"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("<FollowCameraBossOrthoSize>k__BackingField"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("<DialogueCameraOrthoSize>k__BackingField"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("<DialogueCameraOffsetY>k__BackingField"));

        //将serializedObject中的更改写回到target中
        serializedObject.ApplyModifiedProperties();
    }
}

[CustomEditor(typeof(MapLinkPoint))]
public class MapLinkPointEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("<PointGuid>k__BackingField"));
        }

        EditorGUILayout.PropertyField(serializedObject.FindProperty("<DisplayName>k__BackingField"));

        serializedObject.ApplyModifiedProperties();
    }
}
}
