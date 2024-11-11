using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class SelectSelectable : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (GetComponent<Selectable>() != null)
        {
            EventSystem.current.SetSelectedGameObject(this.gameObject);
        }
    }
}
