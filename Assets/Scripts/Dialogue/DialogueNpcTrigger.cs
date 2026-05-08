using DialogueSystem;
using DialogueSystem.Data;
using PlayArk.DialogueSystem.Data;
using UnityEngine;
namespace DialogueSystem.RunTime
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class DialogueNpcTrigger : MonoBehaviour
    {
        /// <summary>
        /// 数组中索引靠前 优先级大
        /// </summary>
        [SerializeField] private DialogueNpcTriggerInfo[] m_dialogueInfoArray;

        [Header("OTHER")]
        [SerializeField] private GameObject m_camera;
        [SerializeField] private GameObject _worldTips;


        private bool m_isPlayerInsideZone = false;//玩家是否进入了检测区域
        private bool m_isPlayerInsideDialogue = false;//玩家是否在对话当中 防止多次按W键

        private void Start()
        {
            _worldTips.SetActive(false);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag(Settings.PlayerTag))
            {
                m_isPlayerInsideZone = true;
                _worldTips.SetActive(true);
            }
        }
        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag(Settings.PlayerTag))
            {
                m_isPlayerInsideZone = false;
                _worldTips.SetActive(false);
            }
        }
        private void Update()
        {
            if (m_isPlayerInsideZone && InputManager.Instance.UpButtonDown && !m_isPlayerInsideDialogue)
            {
                //玩家进入对话状态
                m_isPlayerInsideDialogue = true;
                //限制玩家交互
                InputManager.Instance.SetPlayerInputAction(false);
                //限制其他UI交互
                InputManager.Instance.SetUiInputAction(false);
                //隐藏游戏UI
                UIManager.Instance.HidePanel<GamePanel>();
                //切换到对话镜头
                m_camera.SetActive(true);

                for (int i = 0; i < m_dialogueInfoArray.Length; i++)
                {
                    //跳过不符合触发条件的对话
                    if (!GameManager.Instance.CheckGameCondition(m_dialogueInfoArray[i].eGameCondition))
                        continue;

                    //判断该触发Main对话还是Tail对话
                    if (!GameManager.Instance.GameSaveData.CheckDialogueTriggered(m_dialogueInfoArray[i].guid))
                    {
                        DialogueManager.Instance.PlayDialogueGraph(m_dialogueInfoArray[i].GraphMain, (shouldSave) =>
                        {
                            //恢复玩家镜头
                            m_camera.SetActive(false);
                            if (shouldSave)
                            {
                                //将该对话信息记录到数据中
                                GameManager.Instance.GameSaveData.TriggeredDialogueGuidList.Add(m_dialogueInfoArray[i].guid);
                            }
                            //恢复玩家未对话状态
                            m_isPlayerInsideDialogue = false;
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
                            m_camera.SetActive(false);
                            //恢复玩家未对话状态
                            m_isPlayerInsideDialogue = false;
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
