
using DialogueSystem;
using DialogueSystem.Data;
using PlayArk.DialogueSystem.Data;
using UnityEngine;

using DreamSeeker.CameraSystem;
using DreamSeeker.Conditions;
using DreamSeeker.Data.Runtime;
using DreamSeeker.Managers;
using DreamSeeker.Shared;
using DreamSeeker.UI;

namespace DreamSeeker.Dialogue
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class DialogueOneTrigger : MonoBehaviour
    {
        [SerializeField] private DialogueOneTriggerInfo m_oneTriggerInfo;
        private void OnTriggerEnter2D(Collider2D collision)
        {

            if (collision.gameObject.CompareTag(Settings.PlayerTag))
            {
                //未被触发过
                if (!GameManager.Instance.GameSaveData.CheckDialogueTriggered(m_oneTriggerInfo.guid) && 
                    CheckConditions(m_oneTriggerInfo.Conditions))//并且满足条件
                {
                    //隐藏游戏UI
                    UIManager.Instance.HidePanel<GamePanel>();
                    //限制玩家交互
                    InputManager.Instance.SetPlayerInputAction(false);
                    //限制其他UI交互
                    InputManager.Instance.SetUiInputAction(false);
                    //一次性触发对话时，也交给统一相机系统切镜头
                    CameraManager.Instance?.EnterDialogue(transform);

                    DialogueManager.Instance.PlayDialogueGraph(m_oneTriggerInfo.GraphMain, (shouldSave) =>
                    {
                        if(shouldSave)
                        {
                            //将该对话信息记录到数据中
                            GameManager.Instance.GameSaveData.TriggeredDialogueGuidList.Add(m_oneTriggerInfo.guid);
                        }
                        //恢复玩家镜头
                        CameraManager.Instance?.ExitDialogue();
                        //恢复游戏UI
                        UIManager.Instance.ShowPanel<GamePanel>(E_UILayer.Botton);
                        //恢复玩家交互
                        InputManager.Instance.SetPlayerInputAction(true);
                        //恢复其他UI交互
                        InputManager.Instance.SetUiInputAction(true);
                    });
                }
            }
        }
        /// <summary>
        /// 检测当前对话配置的所有触发条件是否满足。
        /// </summary>
        private bool CheckConditions(ConditionSO[] conditions)
        {
            ConditionContext context = new ConditionContext
            (
                null,
                gameObject,
                GameManager.Instance.Player.gameObject
            );
            return ConditionUtility.AreAllMet(conditions, context);
        }
    }

}
