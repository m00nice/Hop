using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class ScoreBoardSetupExample : MonoBehaviour
{
    public InputField score;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        if (JoystickKeyboard.Main.IsActive())
        {
            if (ArcadeInput.InputInitiated(1, ArcadeInputType.JoystickDown, AxisType.Raw, JoystickKeyboard.Main))
            {
                JoystickKeyboard.Main.MoveDown();
            }
            if (ArcadeInput.InputInitiated(1, ArcadeInputType.JoystickUp, AxisType.Raw, JoystickKeyboard.Main))
            {
                JoystickKeyboard.Main.MoveUp();
            }
            if (ArcadeInput.InputInitiated(1, ArcadeInputType.JoystickLeft, AxisType.Raw, JoystickKeyboard.Main))
            {
                JoystickKeyboard.Main.MoveLeft();
            }
            if (ArcadeInput.InputInitiated(1, ArcadeInputType.JoystickRight, AxisType.Raw, JoystickKeyboard.Main))
            {
                JoystickKeyboard.Main.MoveRight();
            }
            if (ArcadeInput.AnyButtonInitiated(1, JoystickKeyboard.Main))
            {
                JoystickKeyboard.Main.PushSelected();
            }
        }
        else
        {
            if (ArcadeInput.InputInitiated(1, ArcadeInputType.ButtonBlack, AxisType.Raw, ScoreBoard.instance) && ScoreBoard.instance.ReadyForName())
            {
                Debug.Log("Cancelling!");
                ScoreBoard.instance.CancelNewHighscore();
            }
            else if (ArcadeInput.AnyButtonInitiated(1, ScoreBoard.instance))
            {
                if (ScoreBoard.instance.ReadyForName())
                {
                    JoystickKeyboard.Main.Activate(() => { KeyboardFinished(); });
                }
                else
                {
                    if (ScoreBoard.instance.IsActive())
                    {
                        ScoreBoard.instance.EndScoreBoard();
                    }
                    Debug.Log("Return To Start!");
                }
            }
        }
        

    }

    public void KeyboardFinished (bool save = true)
    {
        if (save)
        {
            ScoreBoard.instance.SaveNewScore(JoystickKeyboard.Main.GetCurrentText());
        }
        
    }

    public void ShowScoreBoard()
    {
        ScoreBoard.instance.ShowScoreBoard();
        
    }

    public void PrepareNewScore()
    {
        if (!ScoreBoard.instance.IsActive())
        {
            if (score.text != "")
            {
                bool newScoreIsValid = ScoreBoard.instance.PrepareNewScore(float.Parse(score.text));

                if (newScoreIsValid)
                {
                    ScoreBoard.instance.cancelNewScore = () => KeyboardFinished(false);
                    ScoreBoard.instance.ShowScoreBoard();
                }
                else
                {
                    ScoreBoard.instance.ShowScoreBoard();
                }
            }
        }
        
    }

    public void ReturnAction ()
    {
        Debug.Log("Has returned!");
    }

    public void SaveNewScore()
    {

    }

    public void CloseScoreBoard()
    {

    }
}
