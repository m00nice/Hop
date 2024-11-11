using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
[System.Serializable]
public class TournamentStructure
{
    public string name;
    public int teamsAmount;
    public int numberOfQualifications;
    public int numberRemovalsBeforeQuals;
    public int numberOfInitGroups;
    public int teamsPerGroup;
    public int numberOfWinnersPerGroup;
}

public enum TournamentType
{
    Knockout = 0,
    GroupPlay = 1,
    Combination = 2,
}

[System.Serializable]
public class Team
{
    public string name;
    public int rank;
}

[System.Serializable]
public class TournamentTypeData
{
    public bool unlocked;
    public TournamentType tournamentType;
    public TournamentStructure chosenStructure;

    public List<TournamentStructure> sensibleStructures = new List<TournamentStructure>();
    public List<TournamentStructure> unlockedStructures = new List<TournamentStructure>();
    public int lastValid = -1;
    public int nextValid = -1;
}

public enum TournamentStatus
{
    Undefined = 0,
    DesignFase = 1,
    ReadyForSubscription = 2,
    Running = 3,
    Finalized = 4,
}
public class TournamentFileData
{
    public string path;
    public string name;
    public string gameIdentifier;
    public TournamentStatus status;
    public TournamentType tournamentType;
    public DateTime creation;
    public DateTime activation;
    public DateTime finalization;
    public List<string> winners = new List<string>();
}

public class Tournament : MonoBehaviour
{
    
    public List<TournamentTypeData> tournamentTypeDatas = new List<TournamentTypeData>();
    public List<string> infos = new List<string>();

    public static List<TournamentFileData> tournamentFileDatas = new List<TournamentFileData>();
    public static List<TournamentFileData> GetAllTournamentDatas (string gameIdentifier, TournamentStatus status = TournamentStatus.Undefined)
    {
        //gameidentififer Company + . + Projectname
        string path = ArcadeGlobals.SharedPath + "\\" + gameIdentifier + "\\" + "Tournaments";

        if (Directory.Exists(path))
        {
            string[] allTournaments = Directory.GetFiles(path, "Tournament_*.txt");

            if (allTournaments.Length > 0)
            {
                List<TournamentFileData> result = new List<TournamentFileData>();
                for (int i = 0; i < allTournaments.Length; i++)
                {
                    List<string> fileLines = ArcadeGlobals.ReadLinesFromFile(allTournaments[i], 1);
                    TournamentFileData newTournamentFileData = new TournamentFileData();
                    if (fileLines != null)
                    {
                        Dictionary<string, string> lineDatas = ArcadeGlobals.GetPropertiesFromLine(fileLines[0]);
                        {
                            if (lineDatas.TryGetValue("status", out string value))
                            {
                                if (Enum.TryParse(value, out TournamentStatus statusResult))
                                {
                                    if (status == TournamentStatus.Undefined || statusResult == status)
                                    {
                                        newTournamentFileData.status = statusResult;
                                    }
                                    else
                                    {
                                        //This cancels the load of this!
                                        newTournamentFileData.name = "";
                                    }
                                }
                                else
                                {
                                    newTournamentFileData.name = "";
                                }
                            }
                            else
                            {
                                newTournamentFileData.name = "";
                            }
                        }
                        {
                            if (lineDatas.TryGetValue("name", out string value))
                            {
                                newTournamentFileData.name = value;
                            }
                            else
                            {
                                newTournamentFileData.name = "";
                            }
                        }
                        {
                            if (lineDatas.TryGetValue("gameIdentifier", out string value))
                            {
                                newTournamentFileData.gameIdentifier = value;
                            }
                            else
                            {
                                newTournamentFileData.name = "";
                            }
                        }
                        {
                            if (lineDatas.TryGetValue("path", out string value))
                            {
                                newTournamentFileData.path = value.Replace("*", ":");
                            }
                            else
                            {
                                newTournamentFileData.name = "";
                            }
                        }
                        {
                            if (lineDatas.TryGetValue("tournamentType", out string value))
                            {
                                if (Enum.TryParse(value, out TournamentType typeResult))
                                {
                                    newTournamentFileData.tournamentType = typeResult;
                                }
                            }
                        }
                        {
                            if (lineDatas.TryGetValue("creation", out string creationValue))
                            {
                                newTournamentFileData.creation = ArcadeGlobals.DateTimeFromString(creationValue);

                            }
                        }
                        {
                            if (lineDatas.TryGetValue("activation", out string activationValue))
                            {
                                newTournamentFileData.activation = ArcadeGlobals.DateTimeFromString(activationValue);
                            }
                        }
                        {
                            if (lineDatas.TryGetValue("finalization", out string finalizationValue))
                            {
                                newTournamentFileData.finalization = ArcadeGlobals.DateTimeFromString(finalizationValue);
                            }
                        }

                        if (newTournamentFileData.status == TournamentStatus.Finalized)
                        {
                            List<string> allLines = ArcadeGlobals.ReadLinesFromFile(allTournaments[i]);

                            for (int j = allLines.Count-1; j >= 0; j--)
                            {
                                Dictionary<string, string> winnersDatas = ArcadeGlobals.GetPropertiesFromLine(allLines[j]);
                                {
                                    if (winnersDatas.TryGetValue("name", out string value))
                                    {
                                        if (newTournamentFileData.winners.Count > 0)
                                        {
                                            newTournamentFileData.winners.Insert(0, value);
                                        }
                                        else
                                        {
                                            newTournamentFileData.winners.Add(value);
                                        }
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }

                            }


                        }

                        if (newTournamentFileData.name != "")
                        {
                            result.Add(newTournamentFileData);
                        }
                    }

                }
                return result;
            }
            else
            {
                return null;
            }
        }
        else 
        {
            Debug.Log("No Tournaments");
            return null;
        }

    }

    public void Clear()
    {
        tournamentTypeDatas.Clear();
    }
    public void Calculate(int amount)
    {
        //tournamentTypeDatas.Clear();

        int[] tournamentTypes = (int[])System.Enum.GetValues(typeof(TournamentType));

        for (int i = 0; i < tournamentTypes.Length; i++)
        {
            TournamentType current = (TournamentType)tournamentTypes[i];
            TournamentTypeData newData = null;
            
            if (i < tournamentTypeDatas.Count)
            {
                newData = tournamentTypeDatas[i];
                newData.sensibleStructures.Clear();
                newData.unlockedStructures.Clear();
                newData.chosenStructure = null;
                newData.lastValid = -1;
                newData.nextValid = -1;
                newData.unlocked = false;
            } 
            else
            {
                newData = new TournamentTypeData();
                newData.tournamentType = current;
                tournamentTypeDatas.Add(newData);
            }

            int stopAmount = -1;
            for (int j = 2; j < 200; j++)
            {
                List<TournamentStructure> result = SensibleStructures(j, current);
                if (result.Count > 0 && j <= amount)
                {
                    newData.lastValid = j;
                    if (newData.lastValid == amount)
                    {
                        newData.unlocked = true;
                        if (newData.chosenStructure == null)
                        {
                            newData.chosenStructure = result[0];
                        }
                    }
                }
                else if (result.Count > 0 && j > amount && newData.nextValid == -1)
                {
                    newData.nextValid = j;
                    //newData.sensibleStructures.AddRange(result);
                    stopAmount = j + 10;
                }

                newData.sensibleStructures.AddRange(result);
                if (stopAmount > -1 && stopAmount == j)
                {
                    break;
                }
            }

            newData.unlockedStructures.AddRange(newData.sensibleStructures.FindAll(c => c.teamsAmount == amount));
        }
    }


    List<TournamentStructure> SensibleStructures (int teamCount, TournamentType tournamentType)
    {
        List<TournamentStructure> result = new List<TournamentStructure>();
        double log2OfTeams = Math.Log(teamCount, 2);
        //Debug.Log("Log: " + log2OfTeams);

        if (IsPowerOfTwo(teamCount) && teamCount > 2 && tournamentType == TournamentType.Knockout)
        {
            TournamentStructure clean = new TournamentStructure();
            clean.name = "KNOCKOUT: " + teamCount;
            clean.teamsAmount = teamCount;
            clean.numberOfQualifications = Mathf.RoundToInt((float)Math.Log(teamCount, 2));
            result.Add(clean);
        }
        else
        {
            if (tournamentType == TournamentType.Combination)
            {
                int prevPot = 0;

                if (IsPowerOfTwo(teamCount))
                {
                    TournamentStructure clean = new TournamentStructure();
                    clean.name = "KNOCKOUT: " + teamCount;
                    clean.teamsAmount = teamCount;
                    clean.numberOfQualifications = Mathf.RoundToInt((float)Math.Log(teamCount, 2));
                    result.Add(clean);
                }
                prevPot = ToPreviousPOT(teamCount);

                while (prevPot > 2)
                {
                    int numberRemovalsBeforeQuals = teamCount - prevPot;
                    int numberOfQualifications = Mathf.RoundToInt((float)Math.Log(prevPot, 2));

                    List<uint> commonDenomiators = CommonDenominators((uint)numberRemovalsBeforeQuals, (uint)teamCount);

                    if (commonDenomiators != null)
                    {
                        for (int i = 0; i < commonDenomiators.Count; i++)
                        {
                            int numberOfInitGroups = (int)commonDenomiators[i];
                            int teamsPerGroup = teamCount / numberOfInitGroups;
                            int numberOfWinnersPerGroup = teamsPerGroup - (numberRemovalsBeforeQuals / numberOfInitGroups);

                            if (teamsPerGroup >= 2 && teamsPerGroup <= teamCount && (numberOfWinnersPerGroup < (float)teamsPerGroup/2 || teamsPerGroup > 2))
                            {
                                TournamentStructure valid = new TournamentStructure();
                                valid.name = "COMBINATION: " + teamCount;
                                valid.teamsAmount = teamCount;
                                valid.numberOfQualifications = numberOfQualifications;
                                valid.numberRemovalsBeforeQuals = numberRemovalsBeforeQuals;
                                valid.numberOfInitGroups = numberOfInitGroups;
                                valid.teamsPerGroup = teamsPerGroup;
                                valid.numberOfWinnersPerGroup = numberOfWinnersPerGroup;
                                result.Add(valid);
                            }
                        }
                        

                    }

                    

                    prevPot /= 2;
                }
                result.Sort((x, y) => (CalculateSRRBattles(x, 1) + CalculateSEBattles(x)).CompareTo(CalculateSRRBattles(y, 1) + CalculateSEBattles(y)));

            }
            else if (tournamentType == TournamentType.GroupPlay)
            {
                List<int> groupSizes = Factors(teamCount);

                for (int i = 0; i < groupSizes.Count; i++)
                {
                    if (teamCount / groupSizes[i] > 1)
                    {
                        TournamentStructure valid = new TournamentStructure();
                        valid.name = "GROUP PLAY: " + teamCount;
                        valid.teamsAmount = teamCount;
                        valid.numberOfQualifications = 0;
                        valid.numberRemovalsBeforeQuals = teamCount;
                        valid.numberOfInitGroups = groupSizes[i];
                        valid.teamsPerGroup = teamCount / groupSizes[i];
                        valid.numberOfWinnersPerGroup = 1;
                        result.Add(valid);
                    }
                    
                }

            }
            
        }

        
        return result;
    }

    public static int CalculateSRRBattles(TournamentStructure data, int matchesPerPlayer)
    {
        if (data.numberOfInitGroups > 0)
        {
            int playersPerGroup = data.teamsAmount / data.numberOfInitGroups;
            return (playersPerGroup * (playersPerGroup - 1) / 2) * matchesPerPlayer * data.numberOfInitGroups;
        }
        return 0;
    }

    public static int CalculateSEBattles(TournamentStructure data)
    {
        if (data.numberOfQualifications > 0)
        {
            return (data.teamsAmount - data.numberRemovalsBeforeQuals) - 1;
        }
        return 0;
    }

    List<int> Factors (int number)
    {
        int max = number / 2;
        List<int> result = new List<int>();

        for (int i = 1; i <= max; i++)
        {
            if (number % i == 0)
            {
                result.Add(i);
            }
        }
        result.Add(number);

        return result;

    }

    bool IsPowerOfTwo(int x)
    {
        return x > 0 && (x & (x - 1)) == 0;
    }

    int ToNextPOT(int x)
    {
        if (x < 0) { return 0; }
        --x;
        x |= x >> 1;
        x |= x >> 2;
        x |= x >> 4;
        x |= x >> 8;
        x |= x >> 16;
        return x + 1;
    }

    int ToPreviousPOT(int x)
    {
        int next = ToNextPOT(x);
        return next >> 1;
        //return next - x < x - prev ? next : prev;
    }


    private uint GreatestCommonDenominator(uint a, uint b)
    {
        while (a != 0 && b != 0)
        {
            if (a > b)
                a %= b;
            else
                b %= a;
        }

        return a | b;
    }

    private List<uint> CommonDenominators(uint a, uint b)
    {
        List<uint> result = null;
        uint max = a;
        if (b < max)
        {
            max = b;
        }

        for (int i = 1; i <= max; i++)
        {
            if (a%i == 0 && b%i == 0)
            {
                if (result == null)
                {
                    result = new List<uint>();
                }
                result.Add((uint)i);
            }
        }
        return result;

    }

    // Update is called once per frame
    void Update()
    {
        


    }
}
