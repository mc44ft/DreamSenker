using UnityEngine;

public class GameResourcesSystem : MonoBehaviour
{
    private static GameResourcesSystem instance;
    public static GameResourcesSystem Instance
    {
        get
        {
            if (instance == null)
            {
                //这里只是把预制体上的脚本实例化了
                instance = ResourcesManager.Instance.Load<GameResourcesSystem>("GameResourcesSystem");
            }
            return instance;
        }
    }
    [field: SerializeField] public GameObject OverlayCanvas { get; private set; }
    [field: SerializeField] public GameObject CameraCanvas { get; private set; }
    [field: SerializeField] public GameObject EventSystem { get; private set; }
    [field: SerializeField] public GameObject UiCamera { get; private set; }
}
