using System;
using System.Collections.Generic;
using DreamSeeker.QuestSystem;
using UnityEngine;
using UnityEngine.UI;

namespace DreamSeeker.UI
{
    public class QuestPage : MonoManager
    {
        [SerializeField] private QuestControl _questPrefab;
        [SerializeField] private ToggleGroup _questStateGroup;
        
        private List<QuestControl> _controls = new List<QuestControl>();

        private void OnEnable()
        {
            Refresh();
        }

        private void Refresh()
        {
            foreach (QuestControl control in _controls)
            {
                Destroy(control.gameObject);
            }
            _controls.Clear();

            //根据玩家目前持有任务来刷新任务面板
            var quests = QuestManager.Instance.GetAllRunningQuests();
            foreach (var quest in quests)
            {
                QuestControl control = Instantiate(_questPrefab.gameObject, transform).GetComponent<QuestControl>();
                if (control != null)
                {
                    control.Initialize(quest, _questStateGroup);
                    _controls.Add(control);
                }
            }
        }
        
    }
}
