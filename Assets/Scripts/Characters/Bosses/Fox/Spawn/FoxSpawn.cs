using UnityEngine;

namespace DreamSeeker.Characters.Bosses
{
public class FoxSpawn : MonoBehaviour
{
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, 0.5f);
    }
#endif
}
}
