using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using HardwareHelperLib;
using UnityEngine.EventSystems;
//using UnityEngine.InputSystem;
//using UnityEngine.InputSystem.Utilities;

public class JoystickHandler
{
    public bool upIsDown = false;
    public bool upIsDownReported = false;
    public bool upIsUp = false;
    public bool upIsUpReported = false;

    public bool downIsDown = false;
    public bool downIsDownReported = false;
    public bool downIsUp = false;
    public bool downIsUpReported = false;

    public bool leftIsDown = false;
    public bool leftIsDownReported = false;
    public bool leftIsUp = false;
    public bool leftIsUpReported = false;

    public bool rightIsDown = false;
    public bool rightIsDownReported = false;
    public bool rightIsUp = false;
    public bool rightIsUpReported = false;

    public bool upIsDownRaw = false;
    public bool upIsDownReportedRaw = false;
    public bool upIsUpRaw = false;
    public bool upIsUpReportedRaw = false;

    public bool downIsDownRaw = false;
    public bool downIsDownReportedRaw = false;
    public bool downIsUpRaw = false;
    public bool downIsUpReportedRaw = false;

    public bool leftIsDownRaw = false;
    public bool leftIsDownReportedRaw = false;
    public bool leftIsUpRaw = false;
    public bool leftIsUpReportedRaw = false;

    public bool rightIsDownRaw = false;
    public bool rightIsDownReportedRaw = false;
    public bool rightIsUpRaw = false;
    public bool rightIsUpReportedRaw = false;


    public Vector2 axisValues = Vector2.zero;
    public Vector2 rawAxisValues = Vector2.zero;
    public void Clear()
    {
        upIsDown = false;
        downIsDown = false;
        leftIsDown = false;
        rightIsDown = false;

        upIsUp = false;
        downIsUp = false;
        leftIsUp = false;
        rightIsUp = false;

        upIsDownRaw = false;
        downIsDownRaw = false;
        leftIsDownRaw = false;
        rightIsDownRaw = false;

        upIsUpRaw = false;
        downIsUpRaw = false;
        leftIsUpRaw = false;
        rightIsUpRaw = false;
    }
}

[DefaultExecutionOrder(-1000)]
public class ArcadeInputRun : MonoBehaviour
{
    private static ArcadeInputRun instance;
    

    private List<JoystickHandler> joystickHandlers = new List<JoystickHandler>();
    
    private ArcadeInputButtonData arcadeInputButtonData;

    private List<int> playerToInputIndex = new List<int>();
    //private HH_Lib hhl;
    private bool hasReceivedMappingData = false;

    private int numOfPops = 0;
    private Stack<ModalData> modalData;

    float startTime;
    public static void Initialize ()
    {
        if (instance == null)
        {
            GameObject arcadeInputRun = new GameObject("ArcadeInput");
            Object.DontDestroyOnLoad(arcadeInputRun);

            //arcadeInputRun.hideFlags = HideFlags.HideAndDontSave;
            instance = arcadeInputRun.AddComponent<ArcadeInputRun>();
            instance.arcadeInputButtonData = Resources.Load("ArcadeInputButtonData") as ArcadeInputButtonData;
            instance.startTime = Time.time;

            
            instance.Start();
            instance.Update();
            
        }
    }
    public static ArcadeInputRun Instance
    {
        get
        {
            if (instance == null)
            {
                Initialize();
            }
            return instance;
        }
    }

    public string DebugModal ()
    {
        string result = "";
        for (int i = 0; modalData != null && i < modalData.Count; i++)
        {
            if (modalData.Peek().modalObject.GetType() == typeof(GameObject))
            {
                result += (modalData.Peek().modalObject as GameObject).name + "\r\n";
            } 
            else
            {
                result += (modalData.Peek().modalObject as MonoBehaviour).name + "\r\n";
            }
            
        }
        return "Modal stack\r\n" + result;
    }

    public void RequestModalPop (Stack<ModalData> modalData)
    {
        this.modalData = modalData;
        numOfPops++;
    }

    public bool PopsRequested()
    {
        return numOfPops > 0;
    }

    public KeyCode GetComputerKeyCode (ArcadeInputType inputType, int player)
    {
        int playerIndex = GetPlayerIndex(player);
        if (playerIndex > -1)
        {
            return arcadeInputButtonData.inputSetups[(int)inputType].keyCodes[playerIndex].computerKeyCode;
        }
        return KeyCode.None;
    }

    public KeyCode GetExternalKeyCode(ArcadeInputType inputType, int player)
    {
        int playerIndex = GetPlayerIndex(player);
        if (playerIndex > -1)
        {
            return (KeyCode)arcadeInputButtonData.inputSetups[(int)inputType].keyCodes[playerIndex].externalKeyCode;
        }
        return KeyCode.None;
    }

    public string GetVirtualMapping(ArcadeInputType inputType, int player)
    {
        int playerIndex = GetPlayerIndex(player);
        if (playerIndex > -1)
        {
            return arcadeInputButtonData.inputSetups[(int)inputType].keyCodes[playerIndex].inputManagerIdentifier;
        }
        return "";
    }

    public List<ActiveTester> ActiveTesters
    {
        get
        {
            return arcadeInputButtonData.activeTesters;
        }
    }

    public bool IsReady
    {
        get
        {
            return hasReceivedMappingData;
        }
    }

    void MapControllers()
    {
        for (int i = 0; i < arcadeInputButtonData.activeTesters.Count; i++)
        {
            ActiveTester current = arcadeInputButtonData.activeTesters[i];

            for (int j = 0; j < current.testers.Count; j++)
            {
                if (Input.GetKeyDown(current.testers[j]))
                {
                    hasReceivedMappingData = true;
                    playerToInputIndex[i] = j;
                }
            }
        }
    }
    

    // Start is called before the first frame update
    void Start()
    {
        if (joystickHandlers.Count == 0)
        {
            joystickHandlers.Add(new JoystickHandler());
            joystickHandlers.Add(new JoystickHandler());
            joystickHandlers.Add(new JoystickHandler());
            playerToInputIndex.Add(0);
            playerToInputIndex.Add(1);
            playerToInputIndex.Add(2);
            ArcadeInputLog.Instance.WriteToLog(UnixTimestampMinutes(System.DateTime.Now).ToString());
        }

    }

    // Update is called once per frame
    public void Update()
    {
        MapControllers();

        if (startTime + 0.1f < Time.time)
        {
            hasReceivedMappingData = true;
        }

        joystickHandlers[0].Clear();
        joystickHandlers[1].Clear();
        joystickHandlers[2].Clear();

        HandleJoystickAxis(1);
        HandleJoystickAxis(2);
        HandleJoystickAxis(3);

        if (ArcadeInput.AnyInputInitiated(0))
        {
            ArcadeInputLog.Instance.WriteToLog(UnixTimestampMinutes(System.DateTime.Now).ToString());
        }
    }

    public void LateUpdate()
    {
        while (numOfPops > 0)
        {
            if (modalData.Peek().inputModules != null)
            {
                for (int i = 0; i < modalData.Peek().inputModules.Length; i++)
                {
                    modalData.Peek().inputModules[i].modalObject = null;
                }
            }
                

            modalData.Pop();

            if (modalData.Count > 0)
            {
                if (modalData.Peek().inputModules != null)
                {
                    for (int i = 0; i < modalData.Peek().inputModules.Length; i++)
                    {
                        modalData.Peek().inputModules[i].modalObject = modalData.Peek().modalObject;
                    }
                }
            }

            numOfPops--;
        }
    }
    int UnixTimestampMinutes(System.DateTime date)
    {
        System.DateTime origin = new System.DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
        System.TimeSpan diff = date.ToUniversalTime() - origin;
        return DoubleToInt(diff.TotalMinutes);
    }

    int DoubleToInt(double d)
    {
        if (d < 0)
        {
            return (int)(d - 0.5);
        }
        return (int)(d + 0.5);
    }

    int GetPlayerIndex (int player)
    {
        if (player == 0 && playerToInputIndex.Count > 0)
        {
            return playerToInputIndex[0];
        } 
        else if (player <= playerToInputIndex.Count && player > 0)
        {
            return playerToInputIndex[player-1];
        }
        return -1;
    }
    public List<int> CurrentMapping
    {
        get
        {
            return playerToInputIndex;
        }
    }
    void HandleJoystickAxis(int player)
    {
        if (player > playerToInputIndex.Count)
        {
            Debug.Log("This player don't exist");
            return;
        }

        if (!arcadeInputButtonData.inputSetups[2].keyCodes[GetPlayerIndex(player)].implemented || !arcadeInputButtonData.inputSetups[0].keyCodes[GetPlayerIndex(player)].implemented)
        {
            return;
        }
        // Debug.Log("Handle for: " + player + ", Transformed: " + GetPlayerIndex(player));
        float threshold = 0f;
        string horizontalTag = arcadeInputButtonData.inputSetups[2].keyCodes[GetPlayerIndex(player)].inputManagerIdentifier;
        string verticalTag = arcadeInputButtonData.inputSetups[0].keyCodes[GetPlayerIndex(player)].inputManagerIdentifier;
        
        JoystickHandler active = joystickHandlers[player - 1];

        active.axisValues.x = Input.GetAxis(horizontalTag);
        active.axisValues.y = Input.GetAxis(verticalTag);


        active.rawAxisValues.x = Input.GetAxisRaw(horizontalTag);
        active.rawAxisValues.y = Input.GetAxisRaw(verticalTag);


        if (Input.GetAxis(horizontalTag) > threshold && !active.rightIsDownReported)
        {
            active.rightIsDown = true;
            active.rightIsDownReported = true;
            active.rightIsUpReported = false;
            active.leftIsUpReported = false;

            active.leftIsDown = false;
            active.leftIsDownReported = false;
        }

        if (Input.GetAxis(horizontalTag) < -threshold && !active.leftIsDownReported)
        {
            active.leftIsDown = true;
            active.leftIsDownReported = true;
            active.leftIsUpReported = false;
            active.rightIsUpReported = false;

            active.rightIsDown = false;
            active.rightIsDownReported = false;
        }

        if (Input.GetAxis(verticalTag) > threshold && !active.upIsDownReported)
        {
            active.upIsDown = true;
            active.upIsDownReported = true;
            active.upIsUpReported = false;
            active.downIsUpReported = false;

            active.downIsDown = false;
            active.downIsDownReported = false;
        }
        if (Input.GetAxis(verticalTag) < -threshold && !active.downIsDownReported)
        {
            active.downIsDown = true;
            active.downIsDownReported = true;
            active.downIsUpReported = false;
            active.upIsUpReported = false;

            active.upIsDown = false;
            active.upIsDownReported = false;
        }

        if (Input.GetAxis(horizontalTag) <= threshold && Input.GetAxis(horizontalTag) >= -threshold)
        {
            if (active.leftIsDownReported && !active.leftIsUpReported)
            {
                active.leftIsUp = true;
                active.leftIsUpReported = true;
            }

            if (active.rightIsDownReported && !active.rightIsUpReported)
            {
                active.rightIsUp = true;
                active.rightIsUpReported = true;
            }

            active.leftIsDown = false;
            active.leftIsDownReported = false;

            active.rightIsDown = false;
            active.rightIsDownReported = false;
        }

        if (Input.GetAxis(verticalTag) <= threshold && Input.GetAxis(verticalTag) >= -threshold)
        {
            if (active.upIsDownReported && !active.upIsUpReported)
            {
                active.upIsUp = true;
                active.upIsUpReported = true;
            }

            if (active.downIsDownReported && !active.downIsUpReported)
            {
                active.downIsUp = true;
                active.downIsUpReported = true;
            }


            active.upIsDown = false;
            active.upIsDownReported = false;

            active.downIsDown = false;
            active.downIsDownReported = false;
        }

        //Raw Axis data:

        if (Input.GetAxisRaw(horizontalTag) > threshold && !active.rightIsDownReportedRaw)
        {
            active.rightIsDownRaw = true;
            active.rightIsDownReportedRaw = true;
            active.rightIsUpReportedRaw = false;
            active.leftIsUpReportedRaw = false;

            active.leftIsDownRaw = false;
            active.leftIsDownReportedRaw = false;
        }

        if (Input.GetAxisRaw(horizontalTag) < -threshold && !active.leftIsDownReportedRaw)
        {
            active.leftIsDownRaw = true;
            active.leftIsDownReportedRaw = true;
            active.leftIsUpReportedRaw = false;
            active.rightIsUpReportedRaw = false;

            active.rightIsDownRaw = false;
            active.rightIsDownReportedRaw = false;
        }

        if (Input.GetAxisRaw(verticalTag) > threshold && !active.upIsDownReportedRaw)
        {
            active.upIsDownRaw = true;
            active.upIsDownReportedRaw = true;
            active.upIsUpReportedRaw = false;
            active.downIsUpReportedRaw = false;

            active.downIsDownRaw = false;
            active.downIsDownReportedRaw = false;
        }
        if (Input.GetAxisRaw(verticalTag) < -threshold && !active.downIsDownReportedRaw)
        {
            active.downIsDownRaw = true;
            active.downIsDownReportedRaw = true;
            active.downIsUpReportedRaw = false;
            active.upIsUpReportedRaw = false;

            active.upIsDownRaw = false;
            active.upIsDownReportedRaw = false;
        }

        if (Input.GetAxisRaw(horizontalTag) <= threshold && Input.GetAxisRaw(horizontalTag) >= -threshold)
        {
            if (active.leftIsDownReportedRaw && !active.leftIsUpReportedRaw)
            {
                active.leftIsUpRaw = true;
                active.leftIsUpReportedRaw = true;
            }

            if (active.rightIsDownReportedRaw && !active.rightIsUpReportedRaw)
            {
                active.rightIsUpRaw = true;
                active.rightIsUpReportedRaw = true;
            }

            active.leftIsDownRaw = false;
            active.leftIsDownReportedRaw = false;

            active.rightIsDownRaw = false;
            active.rightIsDownReportedRaw = false;
        }

        if (Input.GetAxisRaw(verticalTag) <= threshold && Input.GetAxisRaw(verticalTag) >= -threshold)
        {
            if (active.upIsDownReportedRaw && !active.upIsUpReportedRaw)
            {
                active.upIsUpRaw = true;
                active.upIsUpReportedRaw = true;
            }

            if (active.downIsDownReportedRaw && !active.downIsUpReportedRaw)
            {
                active.downIsUpRaw = true;
                active.downIsUpReportedRaw = true;
            }


            active.upIsDownRaw = false;
            active.upIsDownReportedRaw = false;

            active.downIsDownRaw = false;
            active.downIsDownReportedRaw = false;
        }
    }

    public Vector2 GetAxisValues(int player)
    {
        if (!ValidPlayer(player))
        {
            return Vector2.zero;
        }
        JoystickHandler active = joystickHandlers[player - 1];
        return active.axisValues;
    }

    public Vector2 GetRawAxisValues(int player)
    {
        if (!ValidPlayer(player))
        {
            return Vector2.zero;
        }
        JoystickHandler active = joystickHandlers[player - 1];
        return active.rawAxisValues;
    }

    public bool JoystickIsLeft(int player, AxisType axisType = AxisType.Smooth)
    {
        if (!ValidPlayer(player))
        {
            return false;
        }
        JoystickHandler active = joystickHandlers[player - 1];

        KeyCode keyCode = arcadeInputButtonData.inputSetups[2].keyCodes[GetPlayerIndex(player)].computerKeyCode;

        if (axisType == AxisType.Raw)
        {
            if (active.leftIsDownReportedRaw || Input.GetKey(keyCode))
            {
                return true;
            }
        }
        else
        {
            if (active.leftIsDownReported || Input.GetKey(keyCode))
            {
                return true;
            }
        }

        return false;
    }

    public bool JoystickIsRight(int player, AxisType axisType = AxisType.Smooth)
    {
        if (!ValidPlayer(player))
        {
            return false;
        }
        JoystickHandler active = joystickHandlers[player - 1];
        KeyCode keyCode = arcadeInputButtonData.inputSetups[3].keyCodes[GetPlayerIndex(player)].computerKeyCode;

        if (axisType == AxisType.Raw)
        {
            if (active.rightIsDownReportedRaw || Input.GetKey(keyCode))
            {
                return true;
            }
        }
        else
        {
            if (active.rightIsDownReported || Input.GetKey(keyCode))
            {
                return true;
            }
        }

        return false;
    }

    public bool JoystickIsUp(int player, AxisType axisType = AxisType.Smooth)
    {
        if (!ValidPlayer(player))
        {
            return false;
        }
        JoystickHandler active = joystickHandlers[player - 1];
        KeyCode keyCode = arcadeInputButtonData.inputSetups[0].keyCodes[GetPlayerIndex(player)].computerKeyCode;
        if (axisType == AxisType.Raw)
        {
            if (active.upIsDownReportedRaw || Input.GetKey(keyCode))
            {
                return true;
            }
        }
        else
        {
            if (active.upIsDownReported || Input.GetKey(keyCode))
            {
                return true;
            }
        }

        return false;
    }

    public bool JoystickIsDown(int player, AxisType axisType = AxisType.Smooth)
    {
        if (!ValidPlayer(player))
        {
            return false;
        }
        JoystickHandler active = joystickHandlers[player - 1];
        KeyCode keyCode = arcadeInputButtonData.inputSetups[1].keyCodes[GetPlayerIndex(player)].computerKeyCode;
        if (axisType == AxisType.Raw)
        {
            if (active.downIsDownReportedRaw || Input.GetKey(keyCode))
            {
                return true;
            }
        }
        else
        {
            if (active.downIsDownReported || Input.GetKey(keyCode))
            {
                return true;
            }
        }

        return false;
    }


    public bool JoystickMovedLeft(int player, AxisType axisType = AxisType.Smooth)
    {
        if (!ValidPlayer(player))
        {
            return false;
        }
        JoystickHandler active = joystickHandlers[player - 1];
        KeyCode keyCode = arcadeInputButtonData.inputSetups[2].keyCodes[GetPlayerIndex(player)].computerKeyCode;
        if (axisType == AxisType.Raw)
        {
            if (active.leftIsDownRaw || Input.GetKeyDown(keyCode))
            {
                return true;
            }
        }
        else
        {
            if (active.leftIsDown || Input.GetKeyDown(keyCode))
            {
                return true;
            }
        }

        return false;
    }

    public bool JoystickMovedRight(int player, AxisType axisType = AxisType.Smooth)
    {
        if (!ValidPlayer(player))
        {
            return false;
        }

        JoystickHandler active = joystickHandlers[player - 1];
        KeyCode keyCode = arcadeInputButtonData.inputSetups[3].keyCodes[GetPlayerIndex(player)].computerKeyCode;
        if (axisType == AxisType.Raw)
        {
            if (active.rightIsDownRaw || Input.GetKeyDown(keyCode))
            {
                return true;
            }
        }
        else
        {
            if (active.rightIsDown || Input.GetKeyDown(keyCode))
            {
                return true;
            }
        }

        return false;
    }

    public bool JoystickMovedUp(int player, AxisType axisType = AxisType.Smooth)
    {
        if (!ValidPlayer(player))
        {
            return false;
        }

        JoystickHandler active = joystickHandlers[player - 1];
        KeyCode keyCode = arcadeInputButtonData.inputSetups[0].keyCodes[GetPlayerIndex(player)].computerKeyCode;
        if (axisType == AxisType.Raw)
        {
            if (active.upIsDownRaw || Input.GetKeyDown(keyCode))
            {
                return true;
            }
        }
        else
        {
            if (active.upIsDown || Input.GetKeyDown(keyCode))
            {
                return true;
            }
        }

        return false;
    }

    public bool JoystickMovedDown(int player, AxisType axisType = AxisType.Smooth)
    {
        if (!ValidPlayer(player))
        {
            return false;
        }

        JoystickHandler active = joystickHandlers[player - 1];
        KeyCode keyCode = arcadeInputButtonData.inputSetups[1].keyCodes[GetPlayerIndex(player)].computerKeyCode;

        if (axisType == AxisType.Raw)
        {
            if (active.downIsDownRaw || Input.GetKeyDown(keyCode))
            {
                return true;
            }
        }
        else
        {
            if (active.downIsDown || Input.GetKeyDown(keyCode))
            {
                return true;
            }
        }

        return false;
    }

    public bool JoystickEndedLeft(int player, AxisType axisType = AxisType.Smooth)
    {
        if (!ValidPlayer(player))
        {
            return false;
        }
        JoystickHandler active = joystickHandlers[player - 1];
        KeyCode keyCode = arcadeInputButtonData.inputSetups[2].keyCodes[GetPlayerIndex(player)].computerKeyCode;

        if (axisType == AxisType.Raw)
        {
            if (active.leftIsUpRaw || Input.GetKeyUp(keyCode))
            {
                return true;
            }
        }
        else
        {
            if (active.leftIsUp || Input.GetKeyUp(keyCode))
            {
                return true;
            }
        }

        return false;
    }

    public bool JoystickEndedRight(int player, AxisType axisType = AxisType.Smooth)
    {
        if (!ValidPlayer(player))
        {
            return false;
        }
        JoystickHandler active = joystickHandlers[player - 1];
        KeyCode keyCode = arcadeInputButtonData.inputSetups[3].keyCodes[GetPlayerIndex(player)].computerKeyCode;

        if (axisType == AxisType.Raw)
        {
            if (active.rightIsUpRaw || Input.GetKeyUp(keyCode))
            {
                return true;
            }
        }
        else
        {
            if (active.rightIsUp || Input.GetKeyUp(keyCode))
            {
                return true;
            }
        }

        return false;
    }

    public bool JoystickEndedUp(int player, AxisType axisType = AxisType.Smooth)
    {
        if (!ValidPlayer(player))
        {
            return false;
        }
        JoystickHandler active = joystickHandlers[player - 1];
        KeyCode keyCode = arcadeInputButtonData.inputSetups[0].keyCodes[GetPlayerIndex(player)].computerKeyCode;

        if (axisType == AxisType.Raw)
        {
            if (active.upIsUpRaw || Input.GetKeyUp(keyCode))
            {
                return true;
            }
        }
        else
        {
            if (active.upIsUp || Input.GetKeyUp(keyCode))
            {
                return true;
            }
        }

        return false;
    }

    public bool JoystickEndedDown(int player, AxisType axisType = AxisType.Smooth)
    {
        if (!ValidPlayer(player))
        {
            return false;
        }
        JoystickHandler active = joystickHandlers[player - 1];
        KeyCode keyCode = arcadeInputButtonData.inputSetups[1].keyCodes[GetPlayerIndex(player)].computerKeyCode;

        if (axisType == AxisType.Raw)
        {
            if (active.downIsUpRaw || Input.GetKeyUp(keyCode))
            {
                return true;
            }
        }
        else
        {
            if (active.downIsUp || Input.GetKeyUp(keyCode))
            {
                return true;
            }
        }


        return false;
    }

    bool ValidPlayer(int player)
    {
        if (player < 1 || player > 3)
        {
            return false;
        }
        return true;
    }
}
