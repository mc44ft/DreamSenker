using System;
using UnityEngine;

namespace DreamSeeker.Data.Runtime
{
[Serializable]
public class PlayerSaveData : IRunningData
{
    public int CurrentHealth;
    //攻击力乘数
    public float AttackMultiply = 1f;

    [Header("UNLOCK ABILITY")]
    public bool IsUnlockSwitchStateSkill;

    [Header("PACKAGE DETAILS")]
    public PackageData PackageData = new PackageData();
}
}
