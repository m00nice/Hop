using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GroupKOTransitionUI : MonoBehaviour
{
    public TMP_Text main;
    public TMP_Text players;
    public void Set ()
    {
        gameObject.SetActive(true);

        int numberOfWinnersPerGroup = ActiveTournament.instance.groupWinners;
        players.text = "";
        string comma = ", ";
        for (int i = 0; i < ActiveTournament.instance.groups.Count; i++)
        {

            for (int j = 0; j < numberOfWinnersPerGroup; j++)
            {
                if (i == ActiveTournament.instance.groups.Count - 1 && j == numberOfWinnersPerGroup-1)
                {
                    comma = "";
                }
                players.text += ActiveTournament.instance.groups[i].Teams[j].name + comma;
            }
        }

        

    }
    private void Update()
    {
        if (ArcadeInput.InputInitiated(0, ArcadeInputType.ButtonA, AxisType.Raw, ActiveTournament.instance))
        {
            ActiveTournament.instance.GoToKO();
            gameObject.SetActive(false);
        }
    }
}
