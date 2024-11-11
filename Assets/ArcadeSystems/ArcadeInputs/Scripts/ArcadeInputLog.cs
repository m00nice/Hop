using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using System;
using System.IO;

public class ArcadeInputLog : MonoBehaviour
{
    private static ArcadeInputLog instance;

    EventWaitHandle waitHandle;
    EventWaitHandle waitHandleExternal;

    private Thread logThread;
    private Thread logThreadExternal;

    bool run = true;
    bool runExternal = true;

    DateTime lastWrite;
    DateTime lastRead;
    int writeDelay = 3569; //Milliseconds
    int readDelay = 5734; //Milliseconds
    // Start is called before the first frame update
    string nextData = "";
    string path = "";
    string pathExternal = "";
    int lastExternalInput = -1;

    public static ArcadeInputLog Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject arcadeInputLog = new GameObject();
                arcadeInputLog.hideFlags = HideFlags.HideAndDontSave;
                instance = arcadeInputLog.AddComponent<ArcadeInputLog>();
                instance.Start();
            }
            return instance;

        }

    }

    public int LastExternalInput
    {
        get
        {
            if (waitHandleExternal == null)
            {
                Debug.Log("You need to Start External Probing, before you can ask for data!");
                return -1;
            }
            return lastExternalInput;
        }
    }

    public DateTime LastInternalInput
    {
        get
        {
            return lastWrite;
        }
    }

    public void StartExternalInputProbing(string path)
    {
        if (instance == null)
        {
            GameObject arcadeInputLog = new GameObject();
            arcadeInputLog.hideFlags = HideFlags.HideAndDontSave;
            instance = arcadeInputLog.AddComponent<ArcadeInputLog>();
            instance.Start();
        }
        if (waitHandleExternal != null)
        {
            waitHandleExternal.Dispose();
            logThreadExternal.Abort();

        }
        runExternal = true;
        lastRead = DateTime.Now;
        pathExternal = path;
        waitHandleExternal = new EventWaitHandle(true, EventResetMode.AutoReset, "ArcadeInputLog");
        logThreadExternal = new Thread(ReadExternalInput);
        logThreadExternal.Start();

    }

    public void StopExternalInputProbing()
    {
        if (instance != null)
        {
            if (waitHandleExternal != null)
            {
                runExternal = false;
                waitHandleExternal.Dispose();
                logThreadExternal.Abort();

            }
        }

    }

    public void WriteToLog(string data)
    {
        nextData = data;
    }

    void Start()
    {
        if (waitHandle == null)
        {
            lastWrite = DateTime.Now;
            lastExternalInput = UnixTimestampMinutes(DateTime.Now);
            path = Application.dataPath + "\\" + "arcadeInputLog.txt";
            waitHandle = new EventWaitHandle(true, EventResetMode.AutoReset, "ArcadeInputLog");
            logThread = new Thread(LogInputs);
            logThread.Start();
        }
    }

    public void LogInputs()
    {
        while (run)
        {
            TimeSpan span = DateTime.Now - lastWrite;
            int ms = (int)span.TotalMilliseconds;
            if (ms > writeDelay)
            {
                if (nextData != "")
                {
                    waitHandle.WaitOne();
                    try
                    {
                        System.IO.File.WriteAllText(path, nextData);
                        nextData = "";
                        lastWrite = DateTime.Now;
                    }
                    catch
                    {
                        lastWrite = DateTime.Now;
                    }

                    /* process file*/
                    waitHandle.Set();
                }
            }
        }
    }

    public void ReadExternalInput()
    {
        while (runExternal)
        {
            TimeSpan span = DateTime.Now - lastRead;
            int ms = (int)span.TotalMilliseconds;

            if (ms > readDelay)
            {
                if (File.Exists(pathExternal))
                {
                    waitHandleExternal.WaitOne();
                    try
                    {
                        string lastInputString = File.ReadAllText(pathExternal);
                        int lastInput;
                        bool success = Int32.TryParse(lastInputString, out lastInput);
                        if (success)
                        {
                            lastExternalInput = lastInput;
                        }
                        lastRead = DateTime.Now;
                    }
                    catch
                    {
                        lastRead = DateTime.Now;
                    }

                    /* process file*/
                    waitHandleExternal.Set();

                }
            }
        }
    }
    
    private void OnDestroy()
    {
        waitHandle.Dispose();
        logThread.Abort();
        run = false;

        waitHandleExternal.Dispose();
        logThreadExternal.Abort();
        runExternal = false;
    }


    int UnixTimestampMinutes(DateTime date)
    {
        DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        TimeSpan diff = date.ToUniversalTime() - origin;
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
}
