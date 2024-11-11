using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ActiveGroupUI : MonoBehaviour
{
    public GroupStatsEntryUI groupStatsEntryTemplate;

    public RectTransform statsContainer;

    public TMP_Text groupInfo;
    public TMP_Text groupTitleUI;
    public TMP_Text groupTeamsCountUI;
    public TMP_Text groupBattlesCountUI;

    List<GroupStatsEntryUI> entries = new List<GroupStatsEntryUI>();

    //List<GroupPlayerUI>
    public void SetData(TournamentGroup group, List<PlayerSelect> selections, TournamentBattle bestBattle)
    {
        for (int i = 0; i < entries.Count; i++)
        {
            Destroy(entries[i].gameObject);
        }
        entries.Clear();

        if (ActiveTournament.instance.quals > 0)
        {
            groupInfo.text = "TOP " + ActiveTournament.instance.groupWinners + " PLAYERS PROCEEDS TO KNOCKOUT";
        }
        else if (ActiveTournament.instance.quals == 0 && ActiveTournament.instance.groups.Count == 1)
        {
            groupInfo.text = "TOP " + ActiveTournament.instance.groupWinners + " WINS THE TOURNAMENT";
        }
        else
        {
            groupInfo.text = "TOP " + ActiveTournament.instance.groupWinners + " WINS THE GROUP";
        }

        groupTitleUI.text = "GROUP " + (group.groupIndex + 1);
        groupTeamsCountUI.text = "PLAYERS: " + group.teamIDs.Count;
        groupBattlesCountUI.text = "BATTLES LEFT: " + group.Battles.FindAll(c => c.status == BattleStatus.Pending).Count;
        for (int i = 0; i < group.Teams.Count; i++)
        {
            entries.Add(Instantiate(groupStatsEntryTemplate.gameObject, statsContainer).GetComponent<GroupStatsEntryUI>());
            GroupStatsEntryUI current = entries[entries.Count - 1];
            PlayerSelect selection = selections.Find(c => c.player == group.Teams[i]);
            Color entryBackground = ActiveTournament.instance.groupEntryDefaultBackground;
            if (i < ActiveTournament.instance.groupWinners)
            {
                entryBackground = ActiveTournament.instance.groupEntryWinnerBackground;
            }
            if (selection != null)
            {
                if (selection.playerIndex == 1)
                {
                    current.Populate(group, group.Teams[i], ActiveTournament.instance.player1Color, bestBattle, entryBackground);
                }
                else
                {
                    current.Populate(group, group.Teams[i], ActiveTournament.instance.player2Color, bestBattle, entryBackground);
                } 
            }
            else
            {
                current.Populate(group, group.Teams[i], Color.white, bestBattle, entryBackground);
            }

        }


    }
}
