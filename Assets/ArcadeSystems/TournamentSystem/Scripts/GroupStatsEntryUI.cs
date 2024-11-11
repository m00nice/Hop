using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GroupStatsEntryUI : MonoBehaviour
{
    public Image background;
    public GroupBattleUI groupBattleTemplate;
    public RectTransform battlesContainer;
    public TMP_Text playerUI;
    public TMP_Text statsUI;
    public TMP_Text stats2UI;
    public Image playerBackground;
    public Button player1Btn;
    public Button player2Btn;

    List<GroupBattleUI> battlesUI = new List<GroupBattleUI>();

    public void Populate (TournamentGroup group, TournamentTeam team, Color backgroundColor, TournamentBattle bestBattle, Color entryBackground)
    {
        playerUI.text = team.name;
        statsUI.text = "POINTS: " + team.points + "\r\nSCORE: " + team.score.ToString(ActiveTournament.instance.scoreFormatting);
        stats2UI.text = "W: " + team.wins + "\r\nL: " + team.loses + "\r\nD: " + team.draws;
        playerBackground.color = backgroundColor;
        background.color = entryBackground;
        GridLayoutGroup grid = battlesContainer.GetComponent<GridLayoutGroup>();

        grid.cellSize = new Vector2(633.8977f / team.Battles.Count, grid.cellSize.y);

        for (int i = 0; i < team.Battles.Count; i++)
        {
            GroupBattleUI newBattle = Instantiate(groupBattleTemplate.gameObject, battlesContainer).GetComponent<GroupBattleUI>();
            
            if (team.Battles[i].TeamA == team)
            {
                newBattle.SetData(team.Battles[i], bestBattle, team.Battles[i].TeamA, team.Battles[i].TeamB);
            }
            else
            {
                newBattle.SetData(team.Battles[i], bestBattle, team.Battles[i].TeamB, team.Battles[i].TeamA);
            }
            battlesUI.Add(newBattle);
        }


    }

}
