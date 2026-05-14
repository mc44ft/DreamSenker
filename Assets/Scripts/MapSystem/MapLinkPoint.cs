using UnityEngine;
//挂载到关卡传送点上 用于配置数据

namespace DreamSenker.MapSystem
{
[DisallowMultipleComponent]
public class MapLinkPoint : MonoBehaviour
{
    public static bool SuppressAutoGeneratePointGuid { get; set; }
    [field: ReadOnly]
    [field: SerializeField] public string PointGuid { get; private set; }
    [field: SerializeField] public string DisplayName { get; private set; }

    private void OnValidate()
    {
        if (SuppressAutoGeneratePointGuid)
        {
            return;
        }

        //运行时只认稳定 GUID，显示名只给编辑器下拉看
        if (string.IsNullOrWhiteSpace(PointGuid))
        {
            PointGuid = System.Guid.NewGuid().ToString("N");
        }
    }

    private void OnEnable()
    {
        MapLinkPointManager.Instance.Register(this);
    }

    private void OnDisable()
    {
        MapLinkPointManager.Instance.Unregister(this);
    }
}
}
