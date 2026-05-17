using DialogueSystem;
using DialogueSystem.Data;
using PlayArk.DialogueSystem.Data;
using UnityEngine;

using DreamSeeker.CameraSystem;
using DreamSeeker.Data.Runtime;
using DreamSeeker.Managers;
using DreamSeeker.Shared;
using DreamSeeker.UI.Panels;

namespace DreamSeeker.Dialogue
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class DialogueNpcTrigger : MonoBehaviour
    {
        /// <summary>
        /// 数组中索引靠前 优先级大
        /// </summary>
        [SerializeField] private DialogueNpcTriggerInfo[] m_dialogueInfoArray;

        [Header("OTHER")]
        [SerializeField] private GameObject _worldTips;


        private bool _isPlayerInsideZone = false;//玩家是否进入了检测区域
        private bool _isPlayerInsideDialogue = false;//玩家是否在对话当中 防止多次按W键

        private void Start()
        {
            _worldTips.SetActive(false);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag(Settings.PlayerTag))
            {
                _isPlayerInsideZone = true;
                _worldTips.SetActive(true);
            }
        }
        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag(Settings.PlayerTag))
            {
                _isPlayerInsideZone = false;
                _worldTips.SetActive(false);
            }
        }
        private void Update()
        {
            if (_isPlayerInsideZone && InputManager.Instance.UpButtonDown && !_isPlayerInsideDialogue)
            {
                //玩家进入对话状态
                _isPlayerInsideDialogue = true;
                //限制玩家交互
                InputManager.Instance.SetPlayerInputAction(false);
                //限制其他UI交互
                InputManager.Instance.SetUiInputAction(false);
                //隐藏游戏UI
                UIManager.Instance.HidePanel<GamePanel>();
                //切换到对话镜头
                CameraManager.Instance?.EnterDialogue(transform);

                for (int i = 0; i < m_dialogueInfoArray.Length; i++)
                {
                    //跳过不符合触发条件的对话
                    if (!CheckConditions(m_dialogueInfoArray[i]))
                        continue;

                    //播放第一个满足条件的对话资源
                    DialogueManager.Instance.PlayDialogueGraph(m_dialogueInfoArray[i].GraphMain, (shouldSave) =>
                    {
                        //恢复玩家镜头
                        CameraManager.Instance?.ExitDialogue();
                        //恢复玩家未对话状态
                        _isPlayerInsideDialogue = false;
                        //恢复游戏UI
                        UIManager.Instance.ShowPanel<GamePanel>(E_UILayer.Botton);
                        //恢复玩家交互
                        InputManager.Instance.SetPlayerInputAction(true);
                        //恢复其他UI交互
                        InputManager.Instance.SetUiInputAction(true);
                        // //保存对话过程中产生的运行时数据
                        // GameManager.Instance.SaveDataAll();
                    });
                    return;
                }
            }
        }
        /// <summary>
        /// 检测当前对话配置的所有触发条件是否满足。
        /// </summary>
        private bool CheckConditions(DialogueNpcTriggerInfo dialogueInfo)
        {
            if (dialogueInfo == null)
            {
                return false;
            }

            DialogueConditionSO[] conditions = dialogueInfo.Conditions;

            //未配置条件时默认允许触发
            if (conditions == null || conditions.Length == 0)
            {
                return true;
            }

            foreach (var condition in conditions)
            {
                //空条件跳过，避免单个资源缺失阻断整条对话
                if (condition == null)
                {
                    continue;
                }

                //任意条件不满足，则当前对话配置不可用
                if (!condition.IsMet(dialogueInfo.QuestDefinition))
                {
                    return false;
                }
            }

            return true;
        }
    }


}
