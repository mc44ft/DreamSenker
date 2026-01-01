using DialogueSystem.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace DialogueSystem.Data
{
    public class DialogueNodeCancelSave : DialogueNodeBase
    {
        protected override void OnExecute()
        {
            //DialogueGraph nowGraph = graph as DialogueGraph;
            //if(nowGraph != null)
            //{
            //    nowGraph.ForceEnd(false);
            //}
            //else
            //{
            //    Finished();
            //}
        }

        protected override void OnFinished()
        {

        }

        public override void Init(string uniqueID, Vector2 viewPosition)
        {
            throw new System.NotImplementedException();
        }

        public override void SetPosition(Vector2 viewPosition)
        {
            throw new System.NotImplementedException();
        }
    }
}

