using DialogueSystem;
using PlayArk.DialogueSystem.Data;
using UnityEngine;

using DreamSeeker.CameraSystem;
using DreamSeeker.Characters;
using DreamSeeker.Conditions;
using DreamSeeker.Managers;
using DreamSeeker.Shared;
using DreamSeeker.UI;

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
        private NpcModeController _npcModeController;//可选模式控制器，纯对话 NPC 可以不配置

        private void Awake()
        {
            // 可选缓存，兼容没有模式控制器的纯对话 NPC。
            _npcModeController = GetComponentInParent<NpcModeController>();
        }

        private void Start()
        {
            UpdateWorldTips();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag(Settings.PlayerTag))
            {
                _isPlayerInsideZone = true;
                UpdateWorldTips();
            }
        }
        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag(Settings.PlayerTag))
            {
                _isPlayerInsideZone = false;
                UpdateWorldTips();
            }
        }
        private void Update()
        {
            UpdateWorldTips();
            if (_isPlayerInsideZone && CanTriggerDialogue() && InputManager.Instance.UpButtonDown && !_isPlayerInsideDialogue)
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
                    });
                    return;
                }
            }
        }

        /// <summary>
        /// 判断当前 NPC 是否允许触发对话。
        /// </summary>
        private bool CanTriggerDialogue()
        {
            return _npcModeController == null || _npcModeController.IsFriendly;
        }

        /// <summary>
        /// 根据玩家范围和 NPC 模式刷新世界提示显隐。
        /// </summary>
        private void UpdateWorldTips()
        {
            if (_worldTips == null)
            {
                return;
            }

            _worldTips.SetActive(_isPlayerInsideZone && CanTriggerDialogue());
        }

        /// <summary>
        /// 检测当前对话配置的所有触发条件是否满足。
        /// </summary>
        private bool CheckConditions(DialogueNpcTriggerInfo dialogueInfo)
        {
            ConditionContext context = new ConditionContext
            (
                dialogueInfo.QuestDefinition,
                gameObject,
                GameManager.Instance.Player.gameObject);
            return ConditionUtility.AreAllMet(dialogueInfo.Conditions, context);
        }
    }


}
