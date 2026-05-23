using PlayArk.DialogueSystem.Data;

namespace DreamSeeker.Conditions
{
    /// <summary>
    /// 条件判断工具类，负责统一执行条件数组判断。
    /// </summary>
    public static class ConditionUtility
    {
        /// <summary>
        /// 判断一组条件是否全部满足。
        /// </summary>
        public static bool AreAllMet(ConditionSO[] conditions, ConditionContext context)
        {
            // 未配置条件时默认通过。
            if (conditions == null || conditions.Length == 0)
            {
                return true;
            }

            foreach (ConditionSO condition in conditions)
            {
                // 空条件跳过，避免单个配置缺失阻断整组条件。
                if (condition == null)
                {
                    continue;
                }

                // 任意条件不满足，则整组条件不通过。
                if (!condition.IsMet(context))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
