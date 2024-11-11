using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcadeInputExamples : MonoBehaviour
{
    //The following is an explanation of how to use the ArcadeInput system.
    //The System has been made by Henrik Kragh
    void Start()
    {
        //To make this script "modal" (All othe scripts querying for inputs will receive false) Call this:
        ArcadeInput.SetModal(this);

        //To release the modal mode of this, call this. But should be called when whatever system "closes", not directly after setting it modal ;)
        ArcadeInput.EndModal(this);

        //Make sure to End modals in the same order they were set.
        //Example: Script A is set modal. No other script will get Inputs. Script A launches another modal script: B
        //Now only B will get inputs. End modal on B before ending modal on A.
    }
    
    //All ArcadeInput queries are done in Update
    void Update()
    {
        /*
         Parameter descriptions
            player: 1 or 2 or 0 (To differentiate between the two sets of controls)
            if 0 is provided as "player", then both sets of controls will be queried,
            and return true if either of the controls are active for the given control type.

            arcadeInputType: an enum for which input to query. To see the full list, set cursor on "ArcadeInputType", and push f12
            
            axisType: Only really valid for joystick queries. Optional (But required if next, see below, parameter is set)
            Raw=No smoothing is applied, and initiation/ending is true/false on the frame they are performed.
            Smooth=Smoothing applied. Is really buggy at the moment, as Unity seems to have introduced bugs in later versions. Can "hang" on values other than 0 indefinitely, if settings in InputManager is not set on specific values
            modalObject: If this parameter is applied, and if Modal is set with the same object in SetModal (See in the Start function), then the inputs will be registered.
         */

        //All the following examples will also work on the keyboard while testing. 
        //The mapping is as follows between keys and Arcade controls:
        //Joystick movement: w,a,s,d for player one, arrow keys for player two
        //Buttons:  ButtonRed = regular "1" for player 1, keypad "1" for player two
        //          ButtonBlue = regular "2"/keypad "2"
        //          ButtonWhite = regular "3"/keypad "3"
        //          ButtonGreen = regular "4"/keypad "4"
        //          ButtonYellow = regular "5"/keypad "5"
        //          ButtonBlack = regular "6"/keypad "6"
        //          ButtonStart = Space for player one, keypad enter for player two
        //Lastly, if colors are not your strong side (Or you simply can't remember how they are placed, you can use an alphabetic naming approach instead:
        //The alphabet is mapped as follows:    A B C
        //                                      D E F
        //Example: Writing ButtonRed is the same as writing ButtonA, ButtonBlue is ButtonB and so on...

        if (ArcadeInput.InputInitiated(1, ArcadeInputType.ButtonRed, AxisType.Raw, this))
        {
            //Do something when red button is pressed. Is only true for that one frame.
        }
        if (ArcadeInput.InputEnded(1, ArcadeInputType.ButtonRed, AxisType.Raw, this))
        {
            //Do something when red button is released. Is only true for that one frame.
        }
        if (ArcadeInput.InputIsActive(1, ArcadeInputType.ButtonRed, AxisType.Raw, this))
        {
            //Do something while red button is held. Is continuously true while button is held (Or joystick is kept in moving position.
        }

        if (ArcadeInput.AnyInputInitiated(1, AxisType.Raw, this))
        {
            //Do something when any input is pressed/activated.
        }

        if (ArcadeInput.AnyInputEnded(1, AxisType.Raw, this))
        {
            //Do something when any input is released/deactivated.
        }

        if (ArcadeInput.AnyInputIsActive(1, AxisType.Raw, this))
        {
            //Do something while any input is held/kept active.
        }


        if (ArcadeInput.AnyButtonInitiated(1, this))
        {
            //Do something when any button is pressed.
        }

        if (ArcadeInput.AnyButtonEnded(1, this))
        {
            //Do something when any button is released.
        }

        if (ArcadeInput.AnyButtonIsActive(1, this))
        {
            //Do something while any button is held.
        }

        float joystickLeftRightMovement = ArcadeInput.GetAxisX(1, AxisType.Raw, this);
        if (joystickLeftRightMovement < 0)
        {
            //Joystick is left
        } 
        else if (joystickLeftRightMovement > 0)
        {
            //Joystick is right
        }
        else if (joystickLeftRightMovement == 0)
        {
            //Joystick is stationary on the horizontal axis
        }
        //Same for the y axis: GetAxisY
        //A Vector2 describing both x and y axis is aquired by GetAxises(int player, AxisType axisType, object modalObject)
        //As already mentioned the whole AxisType.Smooth is rather buggy due to some internal bugs of Unity's making (Recent bugs). So avoid at all cost, unless really important.
    }
}
