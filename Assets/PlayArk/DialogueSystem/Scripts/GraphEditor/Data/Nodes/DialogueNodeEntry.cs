using DialogueSystem.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayArk.DialogueSystem.Data.Nodes
{
    public class DialogueNodeEntry : DialogueNodeBase
    {
        public override Color GetColor()
            => new Color(48 / 255f, 90 / 255f, 86 / 255f);

        public override void Init(string uniqueID, Vector2 viewPosition)
        {
            base.Init(uniqueID, viewPosition);
            SetTitle("Entry");
        }

        protected override void OnExecute()
        {
            OnFinished();
        }

        protected override void Finished()
        {
        
        }
    }
}