using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StartBattleUI : MonoBehaviour
{
    public TMP_Text player1UI;
    public TMP_Text player2UI;
    public TMP_Text counterUI;

    TournamentBattle battle;
    TournamentTeam player1;
    TournamentTeam player2;

    public void Set (TournamentBattle battle, TournamentTeam player1, TournamentTeam player2)
    {
        this.battle = battle;
        this.player1 = player1;
        this.player2 = player2;

        gameObject.SetActive(true);
        player1UI.color = ActiveTournament.instance.player1Color;
        player1UI.text = player1.name;
        player2UI.color = ActiveTournament.instance.player2Color;
        player2UI.text = player2.name;
        StartCoroutine(StartBattle());
    }

    IEnumerator StartBattle ()
    {
        float startTime = Time.time;

        string loadingText = "LOADING GAME...";
        float charDelay = 0.2f;
        
        float lastCharInsertion = startTime - charDelay;
        string textInProgress = "";
        int currentCharIndex = 0;

        while (ActiveTournament.instance.BattleInProgress == battle && startTime + 20 > Time.time)
        {
            if (lastCharInsertion + charDelay <= Time.time)
            {
                lastCharInsertion = Time.time + (Time.time - (lastCharInsertion + charDelay));
                if (currentCharIndex >= loadingText.Length)
                {
                    currentCharIndex = 0;
                    textInProgress = "";
                } 
                else
                {
                    textInProgress += loadingText.Substring(currentCharIndex, 1);
                    currentCharIndex++;
                }
            }
            counterUI.text = textInProgress;

            yield return null;
        }
        counterUI.text = "AWAITING BATTLE RESULT...";
        while (ActiveTournament.instance.BattleInProgress == battle)
        {
            yield return null;
        }
        gameObject.SetActive(false);

    }

}
