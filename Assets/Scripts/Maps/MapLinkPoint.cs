using UnityEngine;
//挂载到关卡传送点上 用于配置数据
[DisallowMultipleComponent]
public class MapLinkPoint : MonoBehaviour
{
    [field: SerializeField] public string ConnectionId { get; private set; }
    [field: SerializeField] public string PointId { get; private set; }

    private void OnEnable()
    {
        MapLinkPointManager.Instance.Register(this);
    }

    private void OnDisable()
    {
        MapLinkPointManager.Instance.Unregister(this);
    }
}
