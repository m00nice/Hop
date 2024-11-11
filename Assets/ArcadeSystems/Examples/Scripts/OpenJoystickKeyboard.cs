using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenJoystickKeyboard : MonoBehaviour
{
    private void Start()
    {
        ArcadeInput.Initialize();
        OpenKeyboard();
    }
    void Update()
    {
        //Either the keyboard can be controlled by an external script like this.
        //Alternatively the checkbox registerInputsInternally can be checked, and so the keyboard will do it for you (Using this exact setup)

        if (JoystickKeyboard.Main.IsActive())
        {
            //This is the basic input setup for moving around the keyboard grid
            //To test for player 2 inputs, just change the first parameter to 2
            //To enable both players on the same time, use 0 as player input.
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
    }

    public void OpenKeyboard ()
    {
        if (JoystickKeyboard.Main != null)
        {
            //Opens the keyboard, and also defines the function that will
            //be called, when "Ok" is being pushed on the keyboard. In this example it is a function called KeyboardFinished()
            JoystickKeyboard.Main.Activate(() => KeyboardFinished());
        }
        
    }
    void KeyboardFinished()
    {
        //This is the callback when "Ok" is pressed. 
        //The keyboard will close itself, and call this. Do whatever in here to move on...
        //The text that was written can be retreived like this
        string writtenText = JoystickKeyboard.Main.GetCurrentText();
        Debug.Log("You wrote: " + writtenText);
    }
}
