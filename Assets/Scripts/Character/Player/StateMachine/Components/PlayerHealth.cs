using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : NewHealth
{
    /// <summary>
    /// 把受击效果嵌入到动画内部
    /// </summary>
    private void GetHitAnimEvent()
    {
        if (!this.enabled) return;
        GameManager.Instance.CameraShake();
        GameManager.Instance.DoHitStop(_healthConfig.GetHitStopTime);
    }
}
