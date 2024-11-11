using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using UnityEngine.EventSystems;
//using UnityEngine.InputSystem;
public enum AxisType
{
    Smooth = 0,
    Raw = 1,
}
public enum ArcadeInputType
{
    JoystickUp = 0,
    JoystickDown = 1,
    JoystickLeft = 2,
    JoystickRight = 3,
    ButtonA = 4,
    ButtonB = 5,
    ButtonC = 6,
    ButtonD = 7,
    ButtonE = 8,
    ButtonF = 9,
    ButtonG = 10,
    ButtonH = 11,
    ButtonI = 12,
    ButtonStart = 12,
    Undefined = 13,

    ButtonRed = 4,
    ButtonBlue = 5,
    ButtonWhite = 6,
    ButtonGreen = 7,
    ButtonYellow = 8,
    ButtonBlack = 9,
    ExternalA = 4,
    ExternalB = 5,
    ExternalC = 6,
    ExternalD = 7,
    ExternalE = 8,
    ExternalF = 9,
    ExternalG = 10,
    ExternalH = 11,
}

public class ModalData
{
    public object modalObject;
    public bool typeBased = false;
    public StandaloneArcadeInputModule[] inputModules;
    public ModalData(object modalObject, bool typeBased, StandaloneArcadeInputModule[] inputModules = null)
    {
        this.modalObject = modalObject;
        this.typeBased = typeBased;
        this.inputModules = inputModules;
    }
}
public class ArcadeInput
{
    static Stack<ModalData> modalObjects = new Stack<ModalData>();
    static int[] inputTypes = (int[])System.Enum.GetValues(typeof(ArcadeInputType));
    static bool initialized = false;

    public static void Initialize()
    {
        if (!initialized)
        {
            initialized = true;
            ArcadeInputRun.Initialize();
            inputTypes = (int[])System.Enum.GetValues(typeof(ArcadeInputType));
            for (int i = 0; i < inputTypes.Length; i++)
            {
                //Debug.Log("Input: " + i + ":" + (ArcadeInputType)inputTypes[i]);
            }
        }
    }

    public static bool IsReady()
    {
        if (!initialized)
        {
            LogInitError(); return false;
        }
        if (!ArcadeInputRun.Instance.IsReady)
        {
            return false;
        }
        return true;
    }

    public static bool SetModal(object modalObject, bool typeBased = false)
    {
        if (!IsReady())
        {
            return false;
        }
        if (modalObject == null)
        {
            Debug.Log("Modal can't be null");
            return false;
        }

        if (ArcadeInputRun.Instance.PopsRequested())
        {
            Debug.Log("You can't set new modals in the same frame you have ended modal! Wait a frame...");
            return false;
        }
        //Debug.Log("Set Modal", modalObject as UnityEngine.Object);
        modalObjects.Push(new ModalData(modalObject, typeBased));

        return true;
    }

    public static bool SetModal(ModalData modalData)
    {
        if (!IsReady())
        {
            return false;
        }
        if (modalData.modalObject == null)
        {
            Debug.Log("Modal can't be null");
            return false;
        }

        if (ArcadeInputRun.Instance.PopsRequested())
        {
            //Debug.Log("You can't set new modals in the same frame you have ended modal! Wait a frame...");
            return false;
        }

        //Debug.Log("Set Modal", modalData.modalObject as UnityEngine.Object);
        modalObjects.Push(modalData);

        if (modalData.inputModules != null)
        {
            for (int i = 0; i < modalData.inputModules.Length; i++)
            {
                modalData.inputModules[i].modalObject = modalData.modalObject;
            }

        }

        return true;
    }

    public static bool EndModal(object modalObject)
    {
        if (!IsReady())
        {
            return false;
        }
        if (modalObjects.Count == 0)
        {
            //Debug.Log("Was allready not in modal mode");
            return false;
        }
        if ((modalObjects.Peek().modalObject != modalObject && !modalObjects.Peek().typeBased) || (modalObjects.Peek().typeBased && modalObjects.Peek().modalObject.GetType() != modalObject.GetType()))
        {
            //Debug.LogError("Can't End modal for this object, another object is in modal on top", modalObject as UnityEngine.Object);
            //Debug.LogError("Who is on top?", modalObjects.Peek().modalObject as UnityEngine.Object);
            return false;
        }

        //Debug.Log("Ended Modal", modalObject as UnityEngine.Object);
        ArcadeInputRun.Instance.RequestModalPop(modalObjects);

        return true;
    }

    public static bool EndModalNow(object modalObject)
    {
        if (!IsReady())
        {
            return false;
        }
        if (modalObjects.Count == 0)
        {
            //Debug.Log("Was allready not in modal mode");
            return false;
        }
        if ((modalObjects.Peek().modalObject != modalObject && !modalObjects.Peek().typeBased) || (modalObjects.Peek().typeBased && modalObjects.Peek().modalObject.GetType() != modalObject.GetType()))
        {
            //Debug.Log("Can't End modal for this object, another object is in modal on top");
            return false;
        }
        //Debug.Log("Ended Modal NOW", modalObject as UnityEngine.Object);
        modalObjects.Pop();

        return true;
    }
    static bool ModalFailed(object modalObject)
    {
        if (modalObjects.Count > 0 && modalObject == null)
        {
            return true;
        }


        if (modalObjects.Count > 0 && ((modalObjects.Peek().modalObject != modalObject && !modalObjects.Peek().typeBased) || (modalObjects.Peek().typeBased && modalObjects.Peek().modalObject.GetType() != modalObject.GetType())))
        {
            //Debug.Log("Failed! " + (modalObject == null));

            return true;
        }
        return false;
    }



    public static bool InputInitiated(int player, ArcadeInputType arcadeInputType, AxisType axisType = AxisType.Raw, object modalObject = null)
    {
        if (!IsReady() || arcadeInputType == ArcadeInputType.Undefined)
        {

            return false;
        }

        if (ArcadeInputRun.Instance == null)
        {
            return false;
        }

        if (ModalFailed(modalObject))
        {
            return false;
        }
        switch (arcadeInputType)
        {
            case ArcadeInputType.JoystickUp:
                return ArcadeInputRun.Instance.JoystickMovedUp(player, axisType) || (player == 0 && (ArcadeInputRun.Instance.JoystickMovedUp(1, axisType) || ArcadeInputRun.Instance.JoystickMovedUp(2, axisType)));
            case ArcadeInputType.JoystickDown:
                return ArcadeInputRun.Instance.JoystickMovedDown(player, axisType) || (player == 0 && (ArcadeInputRun.Instance.JoystickMovedDown(1, axisType) || ArcadeInputRun.Instance.JoystickMovedDown(2, axisType)));
            case ArcadeInputType.JoystickLeft:
                return ArcadeInputRun.Instance.JoystickMovedLeft(player, axisType) || (player == 0 && (ArcadeInputRun.Instance.JoystickMovedLeft(1, axisType) || ArcadeInputRun.Instance.JoystickMovedLeft(2, axisType)));
            case ArcadeInputType.JoystickRight:
                return ArcadeInputRun.Instance.JoystickMovedRight(player, axisType) || (player == 0 && (ArcadeInputRun.Instance.JoystickMovedRight(1, axisType) || ArcadeInputRun.Instance.JoystickMovedRight(2, axisType)));
            default:
                return ButtonDown(player, arcadeInputType);
        }
    }

    public static bool InputEnded(int player, ArcadeInputType arcadeInputType, AxisType axisType = AxisType.Raw, object modalObject = null)
    {
        if (!IsReady() || arcadeInputType == ArcadeInputType.Undefined)
        {
            return false;
        }
        if (ModalFailed(modalObject))
        {
            return false;
        }
        switch (arcadeInputType)
        {
            case ArcadeInputType.JoystickUp:
                return ArcadeInputRun.Instance.JoystickEndedUp(player, axisType) || (player == 0 && (ArcadeInputRun.Instance.JoystickEndedUp(1, axisType) || ArcadeInputRun.Instance.JoystickEndedUp(2, axisType)));
            case ArcadeInputType.JoystickDown:
                return ArcadeInputRun.Instance.JoystickEndedDown(player, axisType) || (player == 0 && (ArcadeInputRun.Instance.JoystickEndedDown(1, axisType) || ArcadeInputRun.Instance.JoystickEndedDown(2, axisType)));
            case ArcadeInputType.JoystickLeft:
                return ArcadeInputRun.Instance.JoystickEndedLeft(player, axisType) || (player == 0 && (ArcadeInputRun.Instance.JoystickEndedLeft(1, axisType) || ArcadeInputRun.Instance.JoystickEndedLeft(2, axisType)));
            case ArcadeInputType.JoystickRight:
                return ArcadeInputRun.Instance.JoystickEndedRight(player, axisType) || (player == 0 && (ArcadeInputRun.Instance.JoystickEndedRight(1, axisType) || ArcadeInputRun.Instance.JoystickEndedRight(2, axisType)));
            default:
                return ButtonUp(player, arcadeInputType);
        }
    }

    public static bool InputIsActive(int player, ArcadeInputType arcadeInputType, AxisType axisType = AxisType.Raw, object modalObject = null)
    {
        if (!IsReady() || arcadeInputType == ArcadeInputType.Undefined)
        {
            return false;
        }
        if (ModalFailed(modalObject))
        {
            return false;
        }
        switch (arcadeInputType)
        {

            case ArcadeInputType.JoystickUp:
                return ArcadeInputRun.Instance.JoystickIsUp(player, axisType) || (player == 0 && (ArcadeInputRun.Instance.JoystickIsUp(1, axisType) || ArcadeInputRun.Instance.JoystickIsUp(2, axisType)));
            case ArcadeInputType.JoystickDown:
                return ArcadeInputRun.Instance.JoystickIsDown(player, axisType) || (player == 0 && (ArcadeInputRun.Instance.JoystickIsDown(1, axisType) || ArcadeInputRun.Instance.JoystickIsDown(2, axisType)));
            case ArcadeInputType.JoystickLeft:
                return ArcadeInputRun.Instance.JoystickIsLeft(player, axisType) || (player == 0 && (ArcadeInputRun.Instance.JoystickIsLeft(1, axisType) || ArcadeInputRun.Instance.JoystickIsLeft(2, axisType)));
            case ArcadeInputType.JoystickRight:
                return ArcadeInputRun.Instance.JoystickIsRight(player, axisType) || (player == 0 && (ArcadeInputRun.Instance.JoystickIsRight(1, axisType) || ArcadeInputRun.Instance.JoystickIsRight(2, axisType)));
            default:
                return ButtonIsActive(player, arcadeInputType);
        }
    }

    public static bool AnyInputInitiated(int player, AxisType axisType = AxisType.Raw, object modalObject = null)
    {
        if (!IsReady())
        {
            return false;
        }
        if (ModalFailed(modalObject))
        {
            return false;
        }
        //int[] inputTypes = (int[])System.Enum.GetValues(typeof(ArcadeInputType));

        for (int i = 0; i < inputTypes.Length; i++)
        {
            if (InputInitiated(player, (ArcadeInputType)inputTypes[i], axisType, modalObject))
            {
                return true;
            }
        }
        return false;
    }

    public static bool AnyButtonInitiated(int player, object modalObject = null)
    {
        if (!IsReady())
        {
            return false;
        }
        if (ModalFailed(modalObject))
        {
            return false;
        }
        //int[] inputTypes = (int[])System.Enum.GetValues(typeof(ArcadeInputType));
        int numExternals = 0;
        for (int i = 4; i < inputTypes.Length - numExternals; i++)
        {
            if (InputInitiated(player, (ArcadeInputType)inputTypes[i], AxisType.Raw, modalObject))
            {
                return true;
            }
        }
        return false;
    }

    public static bool AnyColoredButtonInitiated(int player, object modalObject = null)
    {
        if (!IsReady())
        {
            return false;
        }
        if (ModalFailed(modalObject))
        {
            return false;
        }

        if (InputInitiated(player, ArcadeInputType.ButtonA, AxisType.Raw, modalObject))
        {
            return true;
        }
        if (InputInitiated(player, ArcadeInputType.ButtonB, AxisType.Raw, modalObject))
        {
            return true;
        }
        if (InputInitiated(player, ArcadeInputType.ButtonC, AxisType.Raw, modalObject))
        {
            return true;
        }
        if (InputInitiated(player, ArcadeInputType.ButtonD, AxisType.Raw, modalObject))
        {
            return true;
        }
        if (InputInitiated(player, ArcadeInputType.ButtonE, AxisType.Raw, modalObject))
        {
            return true;
        }
        if (InputInitiated(player, ArcadeInputType.ButtonF, AxisType.Raw, modalObject))
        {
            return true;
        }
        return false;
    }

    public static bool AnyColoredButtonInitiated(int player, out ArcadeInputType whichButton, object modalObject = null)
    {
        whichButton = ArcadeInputType.Undefined;
        if (!IsReady())
        {
            return false;
        }
        if (ModalFailed(modalObject))
        {
            return false;
        }


        if (InputInitiated(player, ArcadeInputType.ButtonA, AxisType.Raw, modalObject))
        {
            //Debug.Log("Was init: " + (ArcadeInputType)inputTypes[i]);
            whichButton = ArcadeInputType.ButtonA;
            return true;
        }
        if (InputInitiated(player, ArcadeInputType.ButtonB, AxisType.Raw, modalObject))
        {
            //Debug.Log("Was init: " + (ArcadeInputType)inputTypes[i]);
            whichButton = ArcadeInputType.ButtonB;
            return true;
        }
        if (InputInitiated(player, ArcadeInputType.ButtonC, AxisType.Raw, modalObject))
        {
            //Debug.Log("Was init: " + (ArcadeInputType)inputTypes[i]);
            whichButton = ArcadeInputType.ButtonC;
            return true;
        }
        if (InputInitiated(player, ArcadeInputType.ButtonD, AxisType.Raw, modalObject))
        {
            //Debug.Log("Was init: " + (ArcadeInputType)inputTypes[i]);
            whichButton = ArcadeInputType.ButtonD;
            return true;
        }
        if (InputInitiated(player, ArcadeInputType.ButtonE, AxisType.Raw, modalObject))
        {
            //Debug.Log("Was init: " + (ArcadeInputType)inputTypes[i]);
            whichButton = ArcadeInputType.ButtonE;
            return true;
        }
        if (InputInitiated(player, ArcadeInputType.ButtonF, AxisType.Raw, modalObject))
        {
            //Debug.Log("Was init: " + (ArcadeInputType)inputTypes[i]);
            whichButton = ArcadeInputType.ButtonF;
            return true;
        }

        return false;
    }

    public static bool AnyInputEnded(int player, AxisType axisType = AxisType.Raw, object modalObject = null)
    {
        if (!IsReady())
        {
            return false;
        }
        if (ModalFailed(modalObject))
        {
            return false;
        }
        //int[] inputTypes = (int[])System.Enum.GetValues(typeof(ArcadeInputType));

        for (int i = 0; i < inputTypes.Length; i++)
        {
            if (InputEnded(player, (ArcadeInputType)inputTypes[i], axisType, modalObject))
            {
                return true;
            }
        }
        return false;
    }

    public static bool AnyButtonEnded(int player, object modalObject = null)
    {
        if (!IsReady())
        {
            return false;
        }
        if (ModalFailed(modalObject))
        {
            return false;
        }
        //int[] inputTypes = (int[])System.Enum.GetValues(typeof(ArcadeInputType));
        int numExternals = 0;
        for (int i = 4; i < inputTypes.Length - numExternals; i++)
        {
            if (InputEnded(player, (ArcadeInputType)inputTypes[i], AxisType.Raw, modalObject))
            {
                return true;
            }
        }
        return false;
    }
    public static bool AnyColoredButtonEnded(int player, object modalObject = null)
    {
        if (!IsReady())
        {
            return false;
        }
        if (ModalFailed(modalObject))
        {
            return false;
        }

        if (InputEnded(player, ArcadeInputType.ButtonA, AxisType.Raw, modalObject))
        {
            return true;
        }
        if (InputEnded(player, ArcadeInputType.ButtonB, AxisType.Raw, modalObject))
        {
            return true;
        }
        if (InputEnded(player, ArcadeInputType.ButtonC, AxisType.Raw, modalObject))
        {
            return true;
        }
        if (InputEnded(player, ArcadeInputType.ButtonD, AxisType.Raw, modalObject))
        {
            return true;
        }
        if (InputEnded(player, ArcadeInputType.ButtonE, AxisType.Raw, modalObject))
        {
            return true;
        }
        if (InputEnded(player, ArcadeInputType.ButtonF, AxisType.Raw, modalObject))
        {
            return true;
        }
        return false;
    }
    public static bool AnyColoredButtonEnded(int player, out ArcadeInputType whichButton, object modalObject = null)
    {
        whichButton = ArcadeInputType.Undefined;
        if (!IsReady())
        {
            return false;
        }
        if (ModalFailed(modalObject))
        {
            return false;
        }

        if (InputEnded(player, ArcadeInputType.ButtonA, AxisType.Raw, modalObject))
        {
            //Debug.Log("Was init: " + (ArcadeInputType)inputTypes[i]);
            whichButton = ArcadeInputType.ButtonA;
            return true;
        }
        if (InputEnded(player, ArcadeInputType.ButtonB, AxisType.Raw, modalObject))
        {
            //Debug.Log("Was init: " + (ArcadeInputType)inputTypes[i]);
            whichButton = ArcadeInputType.ButtonB;
            return true;
        }
        if (InputEnded(player, ArcadeInputType.ButtonC, AxisType.Raw, modalObject))
        {
            //Debug.Log("Was init: " + (ArcadeInputType)inputTypes[i]);
            whichButton = ArcadeInputType.ButtonC;
            return true;
        }
        if (InputEnded(player, ArcadeInputType.ButtonD, AxisType.Raw, modalObject))
        {
            //Debug.Log("Was init: " + (ArcadeInputType)inputTypes[i]);
            whichButton = ArcadeInputType.ButtonD;
            return true;
        }
        if (InputEnded(player, ArcadeInputType.ButtonE, AxisType.Raw, modalObject))
        {
            //Debug.Log("Was init: " + (ArcadeInputType)inputTypes[i]);
            whichButton = ArcadeInputType.ButtonE;
            return true;
        }
        if (InputEnded(player, ArcadeInputType.ButtonF, AxisType.Raw, modalObject))
        {
            //Debug.Log("Was init: " + (ArcadeInputType)inputTypes[i]);
            whichButton = ArcadeInputType.ButtonF;
            return true;
        }
        return false;
    }

    public static bool AnyInputIsActive(int player, AxisType axisType = AxisType.Raw, object modalObject = null)
    {
        if (!IsReady())
        {
            return false;
        }
        if (ModalFailed(modalObject))
        {
            return false;
        }
        //int[] inputTypes = (int[])System.Enum.GetValues(typeof(ArcadeInputType));

        for (int i = 0; i < inputTypes.Length; i++)
        {
            if (InputIsActive(player, (ArcadeInputType)inputTypes[i], axisType, modalObject))
            {
                return true;
            }
        }
        return false;
    }

    public static bool AnyButtonIsActive(int player, object modalObject = null)
    {
        if (!IsReady())
        {
            return false;
        }
        if (ModalFailed(modalObject))
        {
            return false;
        }
        //int[] inputTypes = (int[])System.Enum.GetValues(typeof(ArcadeInputType));
        int numExternals = 0;
        for (int i = 4; i < inputTypes.Length - numExternals; i++)
        {
            if (InputIsActive(player, (ArcadeInputType)inputTypes[i], AxisType.Raw, modalObject))
            {
                return true;
            }
        }
        return false;
    }

    public static bool AnyColoredButtonIsActive(int player, object modalObject = null)
    {
        if (!IsReady())
        {
            return false;
        }
        if (ModalFailed(modalObject))
        {
            return false;
        }

        if (InputIsActive(player, ArcadeInputType.ButtonA, AxisType.Raw, modalObject))
        {
            return true;
        }
        if (InputIsActive(player, ArcadeInputType.ButtonB, AxisType.Raw, modalObject))
        {
            return true;
        }
        if (InputIsActive(player, ArcadeInputType.ButtonC, AxisType.Raw, modalObject))
        {
            return true;
        }
        if (InputIsActive(player, ArcadeInputType.ButtonD, AxisType.Raw, modalObject))
        {
            return true;
        }
        if (InputIsActive(player, ArcadeInputType.ButtonE, AxisType.Raw, modalObject))
        {
            return true;
        }
        if (InputIsActive(player, ArcadeInputType.ButtonF, AxisType.Raw, modalObject))
        {
            return true;
        }
        return false;
    }

    public static bool AnyColoredButtonIsActive(int player, out ArcadeInputType whichButton, object modalObject = null)
    {
        whichButton = ArcadeInputType.Undefined;
        if (!IsReady())
        {
            return false;
        }
        if (ModalFailed(modalObject))
        {
            return false;
        }

        if (InputIsActive(player, ArcadeInputType.ButtonA, AxisType.Raw, modalObject))
        {
            //Debug.Log("Was init: " + (ArcadeInputType)inputTypes[i]);
            whichButton = ArcadeInputType.ButtonA;
            return true;
        }
        if (InputIsActive(player, ArcadeInputType.ButtonB, AxisType.Raw, modalObject))
        {
            //Debug.Log("Was init: " + (ArcadeInputType)inputTypes[i]);
            whichButton = ArcadeInputType.ButtonB;
            return true;
        }
        if (InputIsActive(player, ArcadeInputType.ButtonC, AxisType.Raw, modalObject))
        {
            //Debug.Log("Was init: " + (ArcadeInputType)inputTypes[i]);
            whichButton = ArcadeInputType.ButtonC;
            return true;
        }
        if (InputIsActive(player, ArcadeInputType.ButtonD, AxisType.Raw, modalObject))
        {
            //Debug.Log("Was init: " + (ArcadeInputType)inputTypes[i]);
            whichButton = ArcadeInputType.ButtonD;
            return true;
        }
        if (InputIsActive(player, ArcadeInputType.ButtonE, AxisType.Raw, modalObject))
        {
            //Debug.Log("Was init: " + (ArcadeInputType)inputTypes[i]);
            whichButton = ArcadeInputType.ButtonE;
            return true;
        }
        if (InputIsActive(player, ArcadeInputType.ButtonF, AxisType.Raw, modalObject))
        {
            //Debug.Log("Was init: " + (ArcadeInputType)inputTypes[i]);
            whichButton = ArcadeInputType.ButtonF;
            return true;
        }
        return false;
    }


    [Obsolete("This function is obsolete. Use AnyInputInitiated instead.", false)]
    public static bool AnyInputActivity(int player, AxisType axisType = AxisType.Raw, object modalObject = null)
    {
        if (!IsReady())
        {
            return false;
        }
        if (ModalFailed(modalObject))
        {
            return false;
        }
        //int[] inputTypes = (int[])System.Enum.GetValues(typeof(ArcadeInputType));

        for (int i = 0; i < inputTypes.Length; i++)
        {
            if (InputInitiated(player, (ArcadeInputType)inputTypes[i], axisType, modalObject))
            {
                return true;
            }
        }
        return false;
    }

    [Obsolete("This function is obsolete. Use AnyButtonInitiated instead.", false)]
    public static bool AnyButtonActivity(int player, AxisType axisType = AxisType.Raw, object modalObject = null)
    {
        if (!IsReady())
        {
            return false;
        }
        if (ModalFailed(modalObject))
        {
            return false;
        }
        //int[] inputTypes = (int[])System.Enum.GetValues(typeof(ArcadeInputType));
        int numExternals = 0;
        for (int i = 4; i < inputTypes.Length - numExternals; i++)
        {
            if (InputInitiated(player, (ArcadeInputType)inputTypes[i], axisType, modalObject))
            {
                return true;
            }
        }
        return false;
    }

    public static float GetAxisX(int player, AxisType axisType = AxisType.Raw, object modalObject = null)
    {
        if (!IsReady())
        {
            return 0;
        }
        if (ModalFailed(modalObject))
        {
            return 0;
        }
        if (axisType == AxisType.Raw)
        {
            return ArcadeInputRun.Instance.GetRawAxisValues(player).x;
        }
        else
        {
            return ArcadeInputRun.Instance.GetAxisValues(player).x;
        }

    }

    public static float GetAxisY(int player, AxisType axisType = AxisType.Raw, object modalObject = null)
    {
        if (!IsReady())
        {
            return 0;
        }
        if (ModalFailed(modalObject))
        {
            return 0;
        }
        if (axisType == AxisType.Raw)
        {
            return ArcadeInputRun.Instance.GetRawAxisValues(player).y;
        }
        else
        {
            return ArcadeInputRun.Instance.GetAxisValues(player).y;
        }

    }

    public static Vector2 GetAxises(int player, AxisType axisType = AxisType.Raw, object modalObject = null)
    {
        if (!IsReady())
        {
            return Vector2.zero;
        }
        if (ModalFailed(modalObject))
        {
            return Vector2.zero;
        }
        if (axisType == AxisType.Raw)
        {
            return ArcadeInputRun.Instance.GetRawAxisValues(player);
        }
        else
        {
            return ArcadeInputRun.Instance.GetAxisValues(player);
        }
    }

    public static float InputAsClampedValue(int player, ArcadeInputType arcadeInputType, AxisType axisType = AxisType.Raw, object modalObject = null)
    {
        if (!IsReady() || arcadeInputType == ArcadeInputType.Undefined)
        {
            return 0;
        }

        if (ArcadeInputRun.Instance == null)
        {
            return 0;
        }



        if (ModalFailed(modalObject))
        {
            return 0;
        }
        switch (arcadeInputType)
        {
            case ArcadeInputType.JoystickUp:

                return Mathf.Max(0, GetAxises(player, AxisType.Raw).y);
            case ArcadeInputType.JoystickDown:
                return Mathf.Min(0, GetAxises(player, AxisType.Raw).y);
            case ArcadeInputType.JoystickLeft:
                return Mathf.Min(0, GetAxises(player, AxisType.Raw).x);
            case ArcadeInputType.JoystickRight:
                return Mathf.Max(0, GetAxises(player, AxisType.Raw).x);
            default:
                if (ButtonIsActive(player, arcadeInputType))
                {
                    return 1;
                }
                return 0;
        }
    }

    public static float InputAsFullValue(int player, ArcadeInputType arcadeInputType, AxisType axisType = AxisType.Raw, object modalObject = null)
    {
        if (!IsReady() || arcadeInputType == ArcadeInputType.Undefined)
        {
            //Debug.Log("!ready");
            return 0;
        }

        if (ArcadeInputRun.Instance == null)
        {
            //Debug.Log("Null");
            return 0;
        }

        if (ModalFailed(modalObject))
        {
            //Debug.Log("Modal failed");
            return 0;
        }
        //Debug.Log("Hrmf");
        switch (arcadeInputType)
        {
            case ArcadeInputType.JoystickUp:

                return GetAxises(player, axisType, modalObject).y;
            case ArcadeInputType.JoystickDown:
                return GetAxises(player, axisType, modalObject).y;
            case ArcadeInputType.JoystickLeft:
                return GetAxises(player, axisType, modalObject).x;
            case ArcadeInputType.JoystickRight:
                return GetAxises(player, axisType, modalObject).x;
            default:
                if (ButtonIsActive(player, arcadeInputType))
                {
                    return 1;
                }
                return 0;
        }
    }

    [Obsolete("This function is obsolete. Use GetAxises instead.", false)]
    public static Vector2 GetAxis(int player, AxisType axisType = AxisType.Raw, object modalObject = null)
    {
        if (!IsReady())
        {
            return Vector2.zero;
        }
        if (ModalFailed(modalObject))
        {
            return Vector2.zero;
        }
        if (axisType == AxisType.Raw)
        {
            return ArcadeInputRun.Instance.GetRawAxisValues(player);
        }
        else
        {
            return ArcadeInputRun.Instance.GetAxisValues(player);
        }
    }

    static bool ButtonDown(int player, ArcadeInputType arcadeInputType)
    {
        if (!IsReady() || arcadeInputType == ArcadeInputType.Undefined)
        {
            return false;
        }
        if ((player == 1 || player == 0) && (Input.GetKeyDown(ArcadeInputRun.Instance.GetExternalKeyCode(arcadeInputType, 1)) || Input.GetKeyDown(ArcadeInputRun.Instance.GetComputerKeyCode(arcadeInputType, 1))))
        {
            return true;
        }
        else if ((player == 2 || player == 0) && (Input.GetKeyDown(ArcadeInputRun.Instance.GetExternalKeyCode(arcadeInputType, 2)) || Input.GetKeyDown(ArcadeInputRun.Instance.GetComputerKeyCode(arcadeInputType, 2))))
        {
            return true;
        }
        else if ((player == 3 || player == 0) && (Input.GetKeyDown(ArcadeInputRun.Instance.GetExternalKeyCode(arcadeInputType, 3)) || Input.GetKeyDown(ArcadeInputRun.Instance.GetComputerKeyCode(arcadeInputType, 3))))
        {
            return true;
        }

        return false;
    }

    static bool ButtonUp(int player, ArcadeInputType arcadeInputType)
    {
        if (!IsReady() || arcadeInputType == ArcadeInputType.Undefined)
        {
            return false;
        }
        if ((player == 1 || player == 0) && (Input.GetKeyUp(ArcadeInputRun.Instance.GetExternalKeyCode(arcadeInputType, 1)) || Input.GetKeyUp(ArcadeInputRun.Instance.GetComputerKeyCode(arcadeInputType, 1))))
        {
            return true;
        }
        else if ((player == 2 || player == 0) && (Input.GetKeyUp(ArcadeInputRun.Instance.GetExternalKeyCode(arcadeInputType, 2)) || Input.GetKeyUp(ArcadeInputRun.Instance.GetComputerKeyCode(arcadeInputType, 2))))
        {
            return true;
        }
        else if ((player == 3 || player == 0) && (Input.GetKeyUp(ArcadeInputRun.Instance.GetExternalKeyCode(arcadeInputType, 3)) || Input.GetKeyUp(ArcadeInputRun.Instance.GetComputerKeyCode(arcadeInputType, 3))))
        {
            return true;
        }
        return false;
    }

    static bool ButtonIsActive(int player, ArcadeInputType arcadeInputType)
    {
        if (!IsReady() || arcadeInputType == ArcadeInputType.Undefined)
        {
            return false;
        }
        if ((player == 1 || player == 0) && (Input.GetKey(ArcadeInputRun.Instance.GetExternalKeyCode(arcadeInputType, 1)) || Input.GetKey(ArcadeInputRun.Instance.GetComputerKeyCode(arcadeInputType, 1))))
        {
            return true;
        }
        else if ((player == 2 || player == 0) && (Input.GetKey(ArcadeInputRun.Instance.GetExternalKeyCode(arcadeInputType, 2)) || Input.GetKey(ArcadeInputRun.Instance.GetComputerKeyCode(arcadeInputType, 2))))
        {
            return true;
        }
        else if ((player == 3 || player == 0) && (Input.GetKey(ArcadeInputRun.Instance.GetExternalKeyCode(arcadeInputType, 3)) || Input.GetKey(ArcadeInputRun.Instance.GetComputerKeyCode(arcadeInputType, 3))))
        {
            return true;
        }

        return false;
    }

    static void LogInitError()
    {
        Debug.LogError("You have to call ArcadeInput.Initialize() at least once, preferable at the very start of you game, before using the system!");

    }
}
