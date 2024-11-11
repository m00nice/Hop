using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ActiveTester
{
    public List<KeyCode> testers;
}

[System.Serializable]
public class KeyCodes
{
    public bool implemented;
    public int externalKeyCode;
    public KeyCode computerKeyCode;
    public string inputManagerIdentifier;
}

[System.Serializable]
public class InputSetup
{
    public string name;
    public ArcadeInputType arcadeInputType;
    public List<KeyCodes> keyCodes;
}

[CreateAssetMenu(fileName = "ArcadeInputButtonData", menuName = "Create Arcade Input Button Data")]
public class ArcadeInputButtonData : ScriptableObject
{
    public List<ActiveTester> activeTesters;
    public List<InputSetup> inputSetups;
}
