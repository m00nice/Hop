using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class BattleHandShake : MonoBehaviour
{
    public IEnumerator ShakeHands (ActiveTournament.NextBattle callback, string battlePath, List<string> nextBattleFile)
    {
        string localGameIdentifier = Application.companyName + "." + Application.productName;
        string directory = ArcadeGlobals.SharedPath + "\\" + localGameIdentifier + "\\" + "NextBattle";


        string pathConfirmLocal = ArcadeGlobals.SharedPath + "\\" + localGameIdentifier + "\\" + "NextBattle\\battleConfirm" + ActiveTournament.nextBattleInstanceID + ".txt";

        
        List<string> fileDatas = new List<string>();
        fileDatas.Add("battleConfirm:" + ActiveTournament.nextBattleInstanceID + ";");
        ArcadeGlobals.WriteLinesToFile(fileDatas.ToArray(), pathConfirmLocal);

        string pathGoLocal = ArcadeGlobals.SharedPath + "\\" + localGameIdentifier + "\\" + "NextBattle\\battleGo" + ActiveTournament.nextBattleInstanceID + ".txt";

        float probeMaxTime = 5;
        float startTime = Time.time;
        float probeDelay = 1;
        float lastProbe = 0;

        while (startTime + probeMaxTime > Time.time)
        {
            if (lastProbe + probeDelay <= Time.time)
            {
                lastProbe = Time.time;
                if (File.Exists(pathGoLocal))
                {
                    TournamentBattle battle = null;
                    TournamentTeam player1 = null;
                    TournamentTeam player2 = null;
                    TournamentState tournamentState = TournamentState.Undefined;
                    int currentLine = 1;
                    Dictionary<string, string> battleData = ArcadeGlobals.GetPropertiesFromLine(nextBattleFile[currentLine]);
                    {
                        if (battleData.TryGetValue("battleID", out string value))
                        {
                            battle = TournamentBattle.CreateFromSave(nextBattleFile[currentLine]);
                        }
                    }
                    currentLine++;
                    Dictionary<string, string> player1Data = ArcadeGlobals.GetPropertiesFromLine(nextBattleFile[currentLine]);
                    {
                        if (player1Data.TryGetValue("teamID", out string value))
                        {
                            player1 = TournamentTeam.CreateFromSave(nextBattleFile[currentLine]);
                        }
                    }
                    currentLine++;
                    Dictionary<string, string> player2Data = ArcadeGlobals.GetPropertiesFromLine(nextBattleFile[currentLine]);
                    {
                        if (player2Data.TryGetValue("teamID", out string value))
                        {
                            player2 = TournamentTeam.CreateFromSave(nextBattleFile[currentLine]);
                        }
                    }
                    currentLine++;
                    Dictionary<string, string> state = ArcadeGlobals.GetPropertiesFromLine(nextBattleFile[currentLine]);
                    {
                        if (state.TryGetValue("tournamentState", out string value))
                        {
                            if (Enum.TryParse(value, out TournamentState statusResult))
                            {
                                tournamentState = statusResult;
                            }
                        }
                    }

                    //fileDatas.Add("tournamentState:" + tournamentState + ";");

                    if (battle != null && player1 != null && player2 != null)
                    {
                        NextBattleData newBattleData = new NextBattleData();
                        newBattleData.battle = battle;
                        newBattleData.player1 = player1;
                        newBattleData.player2 = player2;
                        newBattleData.battlePath = battlePath;
                        newBattleData.tournamentState = tournamentState;
                        callback(newBattleData);
                        yield break;
                    }
                    break;
                }
            }
            yield return null;
        }

        callback(null);
        Destroy(this.gameObject);
    }
}
