using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ActiveConnectionTester : MonoBehaviour
{

    public TMP_Text data;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        data.text = "";
        for (int i = 0; i < ArcadeInputRun.Instance.ActiveTesters.Count; i++)
        {
            data.text += "Player: " + (i + 1) + "\r\n";
            for (int j = 0; j < ArcadeInputRun.Instance.ActiveTesters[i].testers.Count; j++)
            {
                data.text += Input.GetKey(ArcadeInputRun.Instance.ActiveTesters[i].testers[j]) + "\r\n";
            }
        }


    }
}
