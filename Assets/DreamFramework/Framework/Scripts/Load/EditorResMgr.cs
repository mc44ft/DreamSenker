using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Video;

/// <summary>
/// 仅限开发时使用
/// 只实现同步加载
/// </summary>
public class EditorResMgr : BaseManager<EditorResMgr>
{
    //所有的资源都放在这个路径下
    private string _rootPath = "Assets/DreamFramework/Framework/Editor/ArtRes/";

    private EditorResMgr()
    { }

    /// <summary>
    /// 加载单个资源
    /// 需要填写后缀
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <returns></returns>
    public T LoadRes<T>(string path) where T : Object
    {
#if UNITY_EDITOR
        //后缀名
        string suffixName = "";
        //预制体 纹理（图片）材质球 音效等等资源
        if (typeof(T) == typeof(GameObject))//预制体
            suffixName = ".prefab";
        else if (typeof(T) == typeof(Texture2D))//图片
            suffixName = ".png";
        else if (typeof(T) == typeof(Material))//材质球
            suffixName = ".mat";
        else if (typeof(T) == typeof(AudioClip))//音效切片
            suffixName = ".wav";
        else if (typeof(T) == typeof(Sprite))//精灵图片
            suffixName = ".png";
        else if (typeof(T) == typeof(AnimationClip))//动画切片
            suffixName = ".anim";
        else if (typeof(T) == typeof(TextAsset))//文本资源
            suffixName = ".txt";
        else if (typeof(T) == typeof(Font))//字体资源
            suffixName = ".ttf";
        else if (typeof(T) == typeof(Shader))//shader资源
            suffixName = ".shader";
        else if (typeof(T) == typeof(VideoClip))//视频切片
            suffixName = ".mp4";
        else if (typeof(T) == typeof(Animation))//动画文件
            suffixName = ".anim";
        else if (typeof(T) == typeof(AudioMixer))//不知道
            suffixName = ".mixer";
        else if (typeof(T) == typeof(AnimatorOverrideController))//不知道
            suffixName = ".overrideController";
        else if (typeof(T) == typeof(Avatar))//化身系统
            suffixName = ".avatar";
        else if (typeof(T) == typeof(Cubemap))//不知道
            suffixName = ".cubemap";
        else if (typeof(T) == typeof(LightmapParameters))//不知道
            suffixName = ".lightmapParameters";
        else if (typeof(T) == typeof(PhysicsMaterial2D))//2d物理材质
            suffixName = ".physicsMaterial2D";
        else
        {
            Debug.LogError("不支持的资源类型: " + typeof(T).Name);
            return null;
        }
        T res = AssetDatabase.LoadAssetAtPath<T>(_rootPath + path + suffixName);
        return res;
#else
        return null;
#endif
    }

    /// <summary>
    /// 加载图集中的单个图片
    /// </summary>
    /// <param name="path">图集路径</param>
    /// <param name="spriteName">精灵图片在图集中的名称</param>
    /// <returns></returns>
    public Sprite LoadSprite(string path, string spriteName)
    {
#if UNITY_EDITOR
        //这是个用于加载所有子资源的方法
        Object[] sprites = AssetDatabase.LoadAllAssetRepresentationsAtPath(_rootPath + path);
        foreach (var sprite in sprites)
        {
            if (sprite.name == spriteName)
            {
                return sprite as Sprite;
            }
        }
        return null;
#else
        return null;
#endif
    }

    /// <summary>
    /// 加载整张图集 并以字典形式返回
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public Dictionary<string, Sprite> LoadSprites(string path)
    {
#if UNITY_EDITOR
        Dictionary<string, Sprite> spriteDic = new Dictionary<string, Sprite>();
        Object[] sprites = AssetDatabase.LoadAllAssetRepresentationsAtPath(_rootPath + path);
        foreach (var sprite in sprites)
        {
            if (sprite is Sprite s)
            {
                spriteDic[s.name] = s;
            }
        }
        return spriteDic;
#else
        return null;
#endif
    }
}
