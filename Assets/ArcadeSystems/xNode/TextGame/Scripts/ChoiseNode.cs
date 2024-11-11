using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

public class ChoiseNode : Node {

    [Input] public int choise;

    public string choiseText;

    [Output] public string chosen;
    // Use this for initialization
    protected override void Init() {
		base.Init();
		
	}

	// Return the correct value of an output port when requested
	public override object GetValue(NodePort port) {
		return null; // Replace this
	}
}