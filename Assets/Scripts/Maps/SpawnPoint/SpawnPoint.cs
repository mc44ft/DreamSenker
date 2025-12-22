using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public E_SpawnType SpawnType;
    [Tooltip("ChangeMap/Save/Teleport + _  + 序号")]
    public string ID;

    private void OnEnable()
    {
        //将自己注册到SpawnPointManager
        SpawnPointManager.Instance.RegisterSpawnPoint(this);
    }
    private void OnDisable()
    {
        SpawnPointManager.Instance.UnregisterSavePoint(this);
        
    }
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (SpawnType == E_SpawnType.FromSavePoint)
            Gizmos.color = Color.green;
        else if(SpawnType == E_SpawnType.TeleportPoint)
            Gizmos.color = Color.yellow;
        else
            Gizmos.color = Color.blue;

        Gizmos.DrawSphere(transform.position, 0.1f);
    }
#endif

}
