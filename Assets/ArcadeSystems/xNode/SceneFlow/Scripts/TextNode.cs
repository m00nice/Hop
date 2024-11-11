using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace SceneFlow
{
    public class TextNode : Node
    {


        [Input] public string next;
        public string text;
        [Output] public int choiceA;
        [Output] public int choiceB;
        [Output] public int choiceC;
        [Output] public int choiceD;
        [Output] public int choiceE;
        // Use this for initialization
        protected override void Init()
        {
            base.Init();

        }

        // Return the correct value of an output port when requested
        public override object GetValue(NodePort port)
        {
            return null; // Replace this
        }
    }
}
