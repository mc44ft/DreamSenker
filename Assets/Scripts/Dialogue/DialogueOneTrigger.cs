

using DialogueSystem.Data;
using PlayArk.DialogueSystem.Data;
using UnityEngine;
namespace DialogueSystem.RunTime
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
                    GameManager.Instance.CheckGameCondition(m_oneTriggerInfo.eGameCondition))//并且满足条件
                {
                    //隐藏游戏UI
                    UIManager.Instance.HidePanel<GamePanel>();
                    //限制玩家交互
                    InputManager.Instance.SetPlayerInputAction(false);
                    //限制其他UI交互
                    InputManager.Instance.SetUiInputAction(false);

                    DialogueManager.Instance.PlayDialogueGraph(m_oneTriggerInfo.GraphMain, (shouldSave) =>
                    {
                        if(shouldSave)
                        {
                            //将该对话信息记录到数据中
                            GameManager.Instance.GameSaveData.TriggeredDialogueGuidList.Add(m_oneTriggerInfo.guid);
                        }
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
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (m_oneTriggerInfo == null) return;

            if (string.IsNullOrEmpty(m_oneTriggerInfo.guid))
            {
                //生成唯一的guid
                m_oneTriggerInfo.guid = System.Guid.NewGuid().ToString();
            }
        }
#endif
    }

}
