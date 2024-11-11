using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class Inputs : MonoBehaviour
{
    public int player;
    public TMP_Text playerText;
    public TMP_Text currentMapping;
    public TMP_Text aLastInit;
    public TMP_Text aLastEnd;
    public TMP_Text bLastInit;
    public TMP_Text bLastEnd;
    public TMP_Text cLastInit;
    public TMP_Text cLastEnd;
    public TMP_Text dLastInit;
    public TMP_Text dLastEnd;
    public TMP_Text eLastInit;
    public TMP_Text eLastEnd;
    public TMP_Text fLastInit;
    public TMP_Text fLastEnd;


    public TMP_Text aExtLastInit;
    public TMP_Text aExtLastEnd;
    public TMP_Text bExtLastInit;
    public TMP_Text bExtLastEnd;
    public TMP_Text cExtLastInit;
    public TMP_Text cExtLastEnd;
    public TMP_Text dExtLastInit;
    public TMP_Text dExtLastEnd;
    public TMP_Text eExtLastInit;
    public TMP_Text eExtLastEnd;
    public TMP_Text fExtLastInit;
    public TMP_Text fExtLastEnd;
    public TMP_Text gExtLastInit;
    public TMP_Text gExtLastEnd;
    public TMP_Text hExtLastInit;
    public TMP_Text hExtLastEnd;

    public TMP_Text jstUpInit;
    public TMP_Text jstUpEnd;
    public TMP_Text jstDownInit;
    public TMP_Text jstDownEnd;
    public TMP_Text jstLeftInit;
    public TMP_Text jstLeftEnd;
    public TMP_Text jstRightInit;
    public TMP_Text jstRightEnd;
    
    void Update()
    {
        if (true)
        {
            foreach (KeyCode kcode in Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(kcode))
                {
                    //Debug.Log("KeyCode down: " + kcode);
                } 
                else if (Input.GetKeyUp(kcode))
                {
                   // Debug.Log("KeyCode up: " + kcode);
                } 
                else
                {
                    
                }
                  
                
                    
            }
        }


        playerText.text = player.ToString();
        currentMapping.text = (ArcadeInputRun.Instance.CurrentMapping[player - 1]+1).ToString();

        if (ArcadeInput.InputInitiated(player, ArcadeInputType.ButtonA))
        {
            aLastInit.text = Time.time.ToString("0.00");
        }
        if (ArcadeInput.InputEnded(player, ArcadeInputType.ButtonA))
        {
            aLastEnd.text = Time.time.ToString("0.00");
        }

        if (ArcadeInput.InputInitiated(player, ArcadeInputType.ButtonB))
        {
            bLastInit.text = Time.time.ToString("0.00");
        }
        if (ArcadeInput.InputEnded(player, ArcadeInputType.ButtonB))
        {
            bLastEnd.text = Time.time.ToString("0.00");
        }

        if (ArcadeInput.InputInitiated(player, ArcadeInputType.ButtonC))
        {
            cLastInit.text = Time.time.ToString("0.00");
        }
        if (ArcadeInput.InputEnded(player, ArcadeInputType.ButtonC))
        {
            cLastEnd.text = Time.time.ToString("0.00");
        }

        if (ArcadeInput.InputInitiated(player, ArcadeInputType.ButtonD))
        {
            dLastInit.text = Time.time.ToString("0.00");
        }
        if (ArcadeInput.InputEnded(player, ArcadeInputType.ButtonD))
        {
            dLastEnd.text = Time.time.ToString("0.00");
        }

        if (ArcadeInput.InputInitiated(player, ArcadeInputType.ButtonE))
        {
            eLastInit.text = Time.time.ToString("0.00");
        }
        if (ArcadeInput.InputEnded(player, ArcadeInputType.ButtonE))
        {
            eLastEnd.text = Time.time.ToString("0.00");
        }

        if (ArcadeInput.InputInitiated(player, ArcadeInputType.ButtonF))
        {
            fLastInit.text = Time.time.ToString("0.00");
        }
        if (ArcadeInput.InputEnded(player, ArcadeInputType.ButtonF))
        {
            fLastEnd.text = Time.time.ToString("0.00");
        }







        if (ArcadeInput.InputInitiated(3, ArcadeInputType.ButtonA))
        {
            aExtLastInit.text = Time.time.ToString("0.00");
        }
        if (ArcadeInput.InputEnded(3, ArcadeInputType.ButtonA))
        {
            aExtLastEnd.text = Time.time.ToString("0.00");
        }
        if (ArcadeInput.InputInitiated(3, ArcadeInputType.ButtonB))
        {
            bExtLastInit.text = Time.time.ToString("0.00");
        }
        if (ArcadeInput.InputEnded(3, ArcadeInputType.ButtonB))
        {
            bExtLastEnd.text = Time.time.ToString("0.00");
        }
        if (ArcadeInput.InputInitiated(3, ArcadeInputType.ButtonC))
        {
            cExtLastInit.text = Time.time.ToString("0.00");
        }
        if (ArcadeInput.InputEnded(3, ArcadeInputType.ButtonC))
        {
            cExtLastEnd.text = Time.time.ToString("0.00");
        }
        if (ArcadeInput.InputInitiated(3, ArcadeInputType.ButtonD))
        {
            dExtLastInit.text = Time.time.ToString("0.00");
        }
        if (ArcadeInput.InputEnded(3, ArcadeInputType.ButtonD))
        {
            dExtLastEnd.text = Time.time.ToString("0.00");
        }
        if (ArcadeInput.InputInitiated(3, ArcadeInputType.ButtonE))
        {
            eExtLastInit.text = Time.time.ToString("0.00");
        }
        if (ArcadeInput.InputEnded(3, ArcadeInputType.ButtonE))
        {
            eExtLastEnd.text = Time.time.ToString("0.00");
        }
        if (ArcadeInput.InputInitiated(3, ArcadeInputType.ButtonF))
        {
            fExtLastInit.text = Time.time.ToString("0.00");
        }
        if (ArcadeInput.InputEnded(3, ArcadeInputType.ButtonF))
        {
            fExtLastEnd.text = Time.time.ToString("0.00");
        }
        if (ArcadeInput.InputInitiated(3, ArcadeInputType.ButtonG))
        {
            gExtLastInit.text = Time.time.ToString("0.00");
        }
        if (ArcadeInput.InputEnded(3, ArcadeInputType.ButtonG))
        {
            gExtLastEnd.text = Time.time.ToString("0.00");
        }
        if (ArcadeInput.InputInitiated(3, ArcadeInputType.ButtonH))
        {
            hExtLastInit.text = Time.time.ToString("0.00");
        }
        if (ArcadeInput.InputEnded(3, ArcadeInputType.ButtonH))
        {
            hExtLastEnd.text = Time.time.ToString("0.00");
        }








        if (ArcadeInput.InputInitiated(player, ArcadeInputType.JoystickUp, AxisType.Raw))
        {
            jstUpInit.text = Time.time.ToString("0.00");
        }
        if (ArcadeInput.InputEnded(player, ArcadeInputType.JoystickUp, AxisType.Raw))
        {
            jstUpEnd.text = Time.time.ToString("0.00");
        }

        if (ArcadeInput.InputInitiated(player, ArcadeInputType.JoystickDown, AxisType.Raw))
        {
            jstDownInit.text = Time.time.ToString("0.00");
        }
        if (ArcadeInput.InputEnded(player, ArcadeInputType.JoystickDown, AxisType.Raw))
        {
            jstDownEnd.text = Time.time.ToString("0.00");
        }

        if (ArcadeInput.InputInitiated(player, ArcadeInputType.JoystickLeft, AxisType.Raw))
        {
            jstLeftInit.text = Time.time.ToString("0.00");
        }
        if (ArcadeInput.InputEnded(player, ArcadeInputType.JoystickLeft, AxisType.Raw))
        {
            jstLeftEnd.text = Time.time.ToString("0.00");
        }

        if (ArcadeInput.InputInitiated(player, ArcadeInputType.JoystickRight, AxisType.Raw))
        {
            jstRightInit.text = Time.time.ToString("0.00");
        }
        if (ArcadeInput.InputEnded(player, ArcadeInputType.JoystickRight, AxisType.Raw))
        {
            jstRightEnd.text = Time.time.ToString("0.00");
        }
    }

    public void Quit()
    {
        if (!Application.isEditor)
        {

            Application.Quit();
        }
        else
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }



    }
}
