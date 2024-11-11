using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class CreatorData
{
    public string name;
    public string role;
}

[CreateAssetMenu(fileName = "InfoFileData", menuName = "Create Arcade Hub Info")]
public class InfoFileData : ScriptableObject
{
    public int accessPin = -1;
    public string season = "";
    public Texture2D gameThumb; //For hub overview
    public Texture2D screenShot; //For details window
    public string gameName = "";
    public string description = "";
    public List<CreatorData> creators = new List<CreatorData>();
    public List<int> playersSupported = new List<int>();
    public bool supportsTournaments = false;


    public string CreateDataFile (string path)
    {
        string infoFile = "";

        infoFile += "pin:" + accessPin + ";" + System.Environment.NewLine;

        if (season != "")
        {
            infoFile += "season:" + season + ";" + System.Environment.NewLine;
        }

        if (gameName != "")
        {
            infoFile += "name:" + gameName + ";" + System.Environment.NewLine;
        }
        if (description != "")
        {
            infoFile += "description:" + description + ";" + System.Environment.NewLine;
        }
        if (creators != null && creators.Count > 0)
        {
            bool keyAdded = false;
            
            for (int i = 0; i < creators.Count; i++)
            {
                if (creators[i].name != "" && creators[i].role != "")
                {
                    if (!keyAdded)
                    {
                        infoFile += "madeby:";
                        keyAdded = true;
                    }
                    infoFile += creators[i].name + ": " + creators[i].role;
                    if (i < creators.Count - 1)
                    {
                        infoFile += System.Environment.NewLine;
                    }
                }
            }
            if (keyAdded)
            {
                infoFile += ";" + System.Environment.NewLine;
            }
        }
        if (playersSupported != null && playersSupported.Count > 0)
        {
            bool keyAdded = false;
            List<int> addedSupports = new List<int>();
            for (int i = 0; i < playersSupported.Count; i++)
            {
                if (!addedSupports.Contains(playersSupported[i]))
                {
                    if (!keyAdded)
                    {
                        infoFile += "players:";
                        keyAdded = true;
                    }
                    infoFile += playersSupported[i] + " " + "PLAYER";
                    if (i < playersSupported.Count - 1)
                    {
                        infoFile += System.Environment.NewLine;
                    }
                    
                }
            }
            if (keyAdded)
            {
                infoFile += ";" + System.Environment.NewLine;
            }
        }
        infoFile += "supportsTournaments:" + supportsTournaments.ToString() + ";";
        if (infoFile != "")
        {
            System.IO.File.WriteAllText(path, infoFile);
        }



        return "";
    }
}
