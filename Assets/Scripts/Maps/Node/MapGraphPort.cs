using PlayArk.GraphCore.Data;
using System;
using PlayArk.GraphCore;

[Serializable]
public class MapGraphPort : GraphCorePort
{
    public E_ExitType ExitType;//出口
    public E_SpawnType SpawnType;//入口
}
