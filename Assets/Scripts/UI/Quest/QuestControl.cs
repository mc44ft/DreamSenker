using DreamSeeker.QuestSystem.Data;
using TMPro;
using UnityEngine;

namespace DreamSeeker.UI.Quest
{
    public class QuestControl : MonoManager
    {
        [SerializeField] private TextMeshProUGUI _job;//任务目标对象：委托人、发布人、交付人
        [SerializeField] private TextMeshProUGUI _name;//任务目标对象名字
        [SerializeField] private TextMeshProUGUI _description;//任务简单描述
        
        private QuestDefinitionSO _questDefinition;

        public void Initialize(QuestDefinitionSO questDefinition)
        {
            _questDefinition = questDefinition;

            //按任务类型指定不同job 当前给默认值
            _job.text = "委托人";
            _name.text = "鹿长老";
            _description.text = _questDefinition.Description;
        }
    }
}
