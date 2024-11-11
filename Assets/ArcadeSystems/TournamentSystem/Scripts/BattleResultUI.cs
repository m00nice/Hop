using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BattleResultUI : MonoBehaviour
{
    public TMP_Text mainMessageUI;
    public TMP_Text awardsUI;
    TournamentBattle battle;
    public void SetResult (string mainMessage, string award, TournamentBattle battle)
    {
        mainMessageUI.text = mainMessage;

        awardsUI.text = award;
        gameObject.SetActive(true);
        this.battle = battle;
    }

    public void Update()
    {
        if (ArcadeInput.InputInitiated(0, ArcadeInputType.ButtonA, AxisType.Raw, ActiveTournament.instance))
        {
            ActiveTournament.instance.FinalizeAfterBattle(battle);
            gameObject.SetActive(false);
        }
    }

}
