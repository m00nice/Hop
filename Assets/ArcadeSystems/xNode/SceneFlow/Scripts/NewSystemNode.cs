using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace SceneFlow
{
	public class NewSystemNode : Node
	{


		[Input] public string next;
		public SceneFlowGraph textNodeSystem;
		public string startNode = "";
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
