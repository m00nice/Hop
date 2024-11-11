using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SubscribedPlayerUI : MonoBehaviour
{
    public TMP_Text nameUI;
    public TMP_Text indexUI;
    public Image backgroundUI;
    TournamentTeam player;
    public void Set (TournamentTeam player)
    {
        this.player = player;
    }

    public void UpdateName (int playerIndex)
    {
        if (player.teamStatus == TeamStatus.Open)
        {
            nameUI.text = "OPEN FOR SUBSCRIPTION...";
            backgroundUI.color = ActiveTournament.instance.openPlayerColor;
        }
        else
        {
            nameUI.text = player.name;
            backgroundUI.color = ActiveTournament.instance.closedPlayerColor;
        }
        indexUI.text = playerIndex + ".";
    }
}
