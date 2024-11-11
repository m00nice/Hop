using UnityEngine.EventSystems;
using UnityEngine;
public class PlayerSpecificEventSystem : EventSystem
{
    //StandaloneArcadeInputModule[] inputModules;
    public int player = 1;
    public GameObject selectedObject;
    public GameObject lastSelectedObject;
    public GameObject lastFrameSelectedObject;
    protected override void OnEnable()
    {
        //inputModules = FindObjectsOfType<StandaloneArcadeInputModule>();
        base.OnEnable();
    }
    protected override void Update()
    {
        

        lastFrameSelectedObject = selectedObject;
        
        EventSystem originalCurrent = EventSystem.current;
        current = this;
#pragma warning disable
        base.Update();
#pragma warning restore
        current = originalCurrent;

        if (selectedObject != currentSelectedGameObject)
        {
            lastSelectedObject = selectedObject;
        }
        selectedObject = currentSelectedGameObject;
    }

    
}