using DreamSeeker.QuestSystem.Data;
using UnityEngine;

namespace DreamSeeker.Conditions
{
    /// <summary>
    /// 条件判断上下文，负责把调用方提供的外部信息传给条件资源。
    /// </summary>
    public class ConditionContext
    {
        /// <summary>
        /// 当前判断关联的任务配置，可为空。
        /// </summary>
        public readonly QuestDefinitionSO QuestDefinition;

        /// <summary>
        /// 当前条件判断的拥有者，例如 NPC、门、机关。
        /// </summary>
        public readonly GameObject Owner;

        /// <summary>
        /// 触发本次判断的对象，例如玩家、交互者。
        /// </summary>
        public readonly GameObject Instigator;

        public ConditionContext(QuestDefinitionSO questDefinition, GameObject owner, GameObject instigator)
        {
            QuestDefinition = questDefinition;
            Owner = owner;
            Instigator = instigator;
        }
    }
}
