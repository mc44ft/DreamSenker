using System.Collections.Generic;
using UnityEngine;

namespace DreamSeeker.QuestSystem.Data
{
    [CreateAssetMenu(fileName = "QuestList_", menuName = "ScriptableObject/Quest/Quest List")]
    public class QuestListSO : ScriptableObject
    {
        public List<QuestDefinitionSO> Quests = new List<QuestDefinitionSO>();
    }
}