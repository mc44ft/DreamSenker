using System.Collections.Generic;
using UnityEngine;

namespace DreamSenker.Characters.Bosses
{
public class Waypoint : MonoBehaviour
{
    /// <summary>
    /// 该航点的邻居航点
    /// </summary>
    public List<Waypoint> ConnectedNodes;

    private void OnDrawGizmos()
    {
        //自己的点位是一个黄色小原点
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.3f);

        Gizmos.color = Color.white;
        //连接邻居航点
        if(ConnectedNodes != null)
        {
            foreach(var node in ConnectedNodes)
            {
                if(node != null)
                {
                    Gizmos.DrawLine(transform.position, node.transform.position);
                }
            }
        }
    }
}
}
