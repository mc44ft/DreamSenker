using DialogueSystem;
using DialogueSystem.Data;
using PlayArk.DialogueSystem.Data;
using UnityEngine;

using DreamSenker.CameraSystem;
using DreamSenker.Data.Runtime;
using DreamSenker.Managers;
using DreamSenker.Shared;
using DreamSenker.UI.Panels;

namespace DreamSenker.Dialogue
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
                    if (!CheckConditions(m_dialogueInfoArray[i].Conditions))
                        continue;

                    //判断该触发Main对话还是Tail对话
                    if (!GameManager.Instance.GameSaveData.CheckDialogueTriggered(m_dialogueInfoArray[i].guid))
                    {
                        DialogueManager.Instance.PlayDialogueGraph(m_dialogueInfoArray[i].GraphMain, (shouldSave) =>
                        {
                            //恢复玩家镜头
                            CameraManager.Instance?.ExitDialogue();
                            if (shouldSave)
                            {
                                //将该对话信息记录到数据中
                                GameManager.Instance.GameSaveData.TriggeredDialogueGuidList.Add(m_dialogueInfoArray[i].guid);
                            }
                            //恢复玩家未对话状态
                            _isPlayerInsideDialogue = false;
                            //恢复游戏UI
                            UIManager.Instance.ShowPanel<GamePanel>(E_UILayer.Botton);
                            //恢复玩家交互
                            InputManager.Instance.SetPlayerInputAction(true);
                            //恢复其他UI交互
                            InputManager.Instance.SetUiInputAction(true);
                            //放在最后 不能漏掉了对话信息
                            //存档
                            GameManager.Instance.SaveDataAll();
                        });
                    }
                    else
                    {
                        DialogueManager.Instance.PlayDialogueGraph(m_dialogueInfoArray[i].GraphTail, (shouldSave) =>
                        {

                            //存档
                            GameManager.Instance.SaveDataAll();
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
                        });
                    }
                    return;
                }
            }
        }
        /// <summary>
        /// 检测当前对话配置的所有触发条件是否满足。
        /// </summary>
        private bool CheckConditions(DialogueConditionSO[] conditions)
        {
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
                if (!condition.IsMet())
                {
                    return false;
                }
            }

            return true;
        }
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (m_dialogueInfoArray == null) return;

            foreach (var info in m_dialogueInfoArray)
            {

                if (string.IsNullOrEmpty(info.guid))
                {
                    //生成唯一的guid
                    info.guid = System.Guid.NewGuid().ToString();
                }
            }
        }
#endif
    }


}
