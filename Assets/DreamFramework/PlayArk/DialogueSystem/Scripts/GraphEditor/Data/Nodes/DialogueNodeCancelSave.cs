using DialogueSystem.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace PlayArk.DialogueSystem.Data.Nodes
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

        protected override void Finished()
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

