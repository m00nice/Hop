using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Globalization;
using System.Linq;


public class ArcadeGlobals
{
    //( "Mars_HRSC_MOLA_BlendDEM_Global_200mp_v2@1" - (-8528) ) / (31060 - (-8528) )
    //Vi kunne godt ønske os lidt input på det faktiske forløb, og hvordan du oplevede det:

    //-8528.0
    //31060   
    private static string sharedPath = "";
    private static string sharedApplicationPath = "";

    public static string SharedPath
    {
        get
        {
            if (sharedPath == "")
            {
                sharedPath = ReadAllFromFile(Application.dataPath + "\\sharedPath.txt");
                sharedPath = sharedPath.Replace("\n", "").Replace("\r", "");
                Debug.Log("Read Path: " + sharedPath);
                if (!Directory.Exists(sharedPath))
                {
                    Directory.CreateDirectory(Application.dataPath + "\\ArcadeSystems\\" + "SharedData");
                    sharedPath = Application.dataPath + "\\ArcadeSystems\\" + "SharedData";
                    Debug.Log("What: " + sharedPath);
                }
            }
            return sharedPath;
        }
    }

    public static string SharedApplicationPath
    {
        get
        {
            if (sharedApplicationPath == "")
            {
                sharedApplicationPath = SharedPath + "\\" + Application.companyName + "." + Application.productName;
                if (!Directory.Exists(sharedApplicationPath))
                {
                    Directory.CreateDirectory(sharedApplicationPath);
                }
            }
            return sharedApplicationPath;
        }
    }

    public static bool WriteTextToFile(string text, string path)
    {
        if (false && !Uri.IsWellFormedUriString(path.Replace("\\", "/"), UriKind.Absolute))
        {
            return false;
        }
        DateTime lastWrite = DateTime.Now;
        int writeDelay = 20;
        int trials = 0;
        int maxTrials = 50;
        while (trials < maxTrials)
        {

            TimeSpan span = DateTime.Now - lastWrite;
            int ms = (int)span.TotalMilliseconds;

            if (ms > writeDelay)
            {
                try
                {
                    File.WriteAllText(path, text);
                    Debug.Log("Text is Written!");

                    return true;
                }
                catch (Exception e)
                {
                    trials++;
                    lastWrite = DateTime.Now;
                }
            }
        }
        return false;
    }

    public static bool WriteLinesToFile(string[] texts, string path)
    {
        DateTime lastWrite = DateTime.Now;
        int writeDelay = 20;
        int trials = 0;
        int maxTrails = 50;
        while (trials < maxTrails)
        {
            TimeSpan span = DateTime.Now - lastWrite;
            int ms = (int)span.TotalMilliseconds;

            if (ms > writeDelay)
            {
                //waitHandle.WaitOne();
                try
                {
                    File.WriteAllLines(path, texts);
                    lastWrite = DateTime.Now;
                    return true;
                }
                catch (Exception e)
                {
                    
                    lastWrite = DateTime.Now;
                    trials++;
                }
            }
        }
        return false;
    }

    public static string ReadAllFromFile (string path)
    {
        if (File.Exists(path))
        {
            DateTime lastRead = DateTime.MinValue;
            while (true)
            {
                if ((DateTime.Now- lastRead).Seconds >= 1)
                {
                    lastRead = DateTime.Now;
                    try
                    {
                        StreamReader fileReader =
                        new StreamReader(path);
                        string data = fileReader.ReadToEnd();
                        fileReader.Close();
                        return data;
                    }
                    catch
                    {
                        continue;
                    }
                }
                
            }
        }

        return "";
    }

    public static List<string> ReadLinesFromFile(string path, int lineCount = -1)
    {
        if (File.Exists(path))
        {
            DateTime lastRead = DateTime.MinValue;
            while (true)
            {

                if ((DateTime.Now - lastRead).Seconds >= 1)
                {
                    lastRead = DateTime.Now;
                    try
                    {
                        List<string> datas = new List<string>();
                        string nextLine;
                        int linesRead = 0;
                        // Read the file and display it line by line.  
                        StreamReader fileReader =
                            new StreamReader(path);
                        while ((nextLine = fileReader.ReadLine()) != null && (lineCount == -1 || linesRead < lineCount))
                        {
                            string cleaned = nextLine.Replace("\n", "").Replace("\r", "");
                            if (cleaned != "")
                            {
                                datas.Add(cleaned);
                                linesRead++;
                            }

                        }

                        fileReader.Close();
                        return datas;
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
        }
        return null;
    }

    public static Dictionary<string, string> GetPropertiesFromLine (string line)
    {
        string[] propertiesSplit = line.Split(';');
        Dictionary<string, string> result = new Dictionary<string, string>();

        for (int i = 0; i < propertiesSplit.Length; i++)
        {
            string[] property = propertiesSplit[i].Split(':');

            if (property.Length == 2)
            {
                result.Add(property[0], property[1]);
            }
        }

        return result;
    }

    public static string GetFirstPropertyFromFile (string path, string propertyName)
    {
        List<string> lines = ReadLinesFromFile(path);
        if (lines != null)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Contains(propertyName))
                {
                    string[] split = lines[i].Split(':');
                    if (split.Length > 1)
                    {
                        return split[1].Replace(";", "");
                    }
                }
            }

        }
        return "";
    }

    public static string DateTimeToString(DateTime time)
    {
        const string FMT = "O";
        return time.ToString(FMT).Replace(":", "_");
    }

    public static DateTime DateTimeFromString(string time)
    {
        const string FMT = "O";
        DateTime dt = DateTime.ParseExact(time.Replace("_", ":"), FMT, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
        return dt;
    }

    public string DateTimeDateNice (DateTime dt)
    {
        return "Date: " + dt.Day.ToString("00") + "/" + dt.Month.ToString("00") + " " + dt.Year.ToString("0000");
    }
    public string DateTimeTimeNice(DateTime dt)
    {
        return "Time: " + dt.Hour.ToString("00") + ":" + dt.Minute.ToString("00");
    }

    static string ConvertToBits(short value, int length)
    {
        // 0000000000000000000000000000000000000000000000000000000000000001
        char[] bitArray = System.Convert.ToString((long)value, 2).ToArray();
        ////UnityEngine.Debug.Log("Length: " + bitArray.Length);
        string result = "";

        for (int i = length; i >= 0; i--)
        {
            if (i >= bitArray.Length)
            {
                result += "0";
            }
            else
            {
                result += bitArray[bitArray.Length - 1 - i];
            }
        }
        return "Length: " + result.Length + ", " + result;
    }
    public static void LoadBinary()
    {
        string path = "C:\\Users\\HKragh\\Downloads\\Mars_HRSC_MOLA_BlendDEM_Global_200mp_v2.tif";
        //Stream s = new FileStream(, FileMode.Open);
        short lowest = 32767;
        short highest = -32767;
        ulong byteCount = 0;
        using (Stream s = new FileStream(path, FileMode.Open))
        {
            int read;
            int seq = 0;
            int last = 0;
            
            while ((read = s.ReadByte()) != -1 && byteCount < 1100)
            {
                Console.Write("{0} ", read);
                if (seq == 1)
                {
                    seq = 0;
                    short value = (short)((byte)last << 8 | (byte)read);

                    if (value < lowest)
                    {
                        lowest = value;
                    }
                    if (value > highest)
                    {
                        highest = value;
                    }
                    if (byteCount < 1000)
                    {
                        Debug.Log((byteCount-1) + ": " + ConvertToBits((short)last, 7));
                        Debug.Log((byteCount) + ": " + ConvertToBits((short)read, 7));
                        Debug.Log("Value: " + value + ", bits: " + ConvertToBits((short)value, 15));
                    }
                }
                else
                {
                    last = read;
                    seq++;
                }
                byteCount++;

            }
        }

        /*
        BinaryReader br = new BinaryReader(s);

        short lowest = 32767;
        short highest = -32767;
        
        try
        {
            br.BaseStream.Position = 0;
            int byteCount = 0;
            
            while (true)
            {
                byte next = br.ReadByte();
                byte nextNext = br.ReadByte();

                short value = (short)(next << 8 | nextNext);

                if (value < lowest)
                {
                    lowest = value;
                }
                if (value > highest)
                {
                    highest = value;
                }
                if (byteCount < 100)
                {
                    Debug.Log(byteCount + ": " + next);
                    Debug.Log((byteCount + 1) + ": " + nextNext);
                }
                byteCount+=2;
            }
            
        }
        catch (EndOfStreamException e)
        {
            Console.WriteLine("Error writing the data.\n{0}",
                e.GetType().Name);
        }
        */
        Debug.Log("Total Bytes: " + byteCount);
        Debug.Log("Lowest: " + ConvertToBits(lowest, 15));
        Debug.Log("Highest: " + ConvertToBits(highest, 15));
    }

}
