using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InsideMapTrigger : MonoBehaviour
{
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag(Settings.PlayerTag))
        {
            //玩家掉下地图

        }
    }
}
