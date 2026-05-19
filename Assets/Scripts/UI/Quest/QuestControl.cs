using System;
using DreamSeeker.QuestSystem;
using DreamSeeker.QuestSystem.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DreamSeeker.UI
{
    public class QuestControl : MonoManager
    {
        [SerializeField] private TextMeshProUGUI _job;//任务目标对象：委托人、发布人、交付人
        [SerializeField] private TextMeshProUGUI _name;//任务目标对象名字
        [SerializeField] private TextMeshProUGUI _description;//任务简单描述
        
        [SerializeField] private Toggle _questState;//任务追踪状态
        
        private QuestDefinitionSO _questDefinition;

        private void OnEnable()
        {
            _questState.onValueChanged.AddListener(OnQuestStateValueChanged);
        }

        private void OnDisable()
        {
            _questState.onValueChanged.RemoveListener(OnQuestStateValueChanged);
        }

        private void OnQuestStateValueChanged(bool isOn)
        {
            if (isOn && gameObject.activeInHierarchy)
            {
                QuestManager.Instance.TrackQuest(_questDefinition.QuestId);
            }
        }

        public void Initialize(QuestDefinitionSO questDefinition, ToggleGroup group)
        {
            _questDefinition = questDefinition;

            //按任务类型指定不同job 当前给默认值
            _job.text = "委托人";
            _name.text = "鹿长老";
            _description.text = _questDefinition.Description;
            
            if(group != null)
                _questState.group = group;
        }
        
    }
}
