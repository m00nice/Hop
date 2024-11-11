using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerSelect : MonoBehaviour
{
    public TMP_Text playerNameUI;
    public TMP_Text teamIndexUI;
    public TMP_Text playerGroupUI;

    public Image indexBG;
    public Image groupBG;


    public Button button;
    public TournamentTeam player;
    public int playerIndex;
    public void Set(TournamentTeam player, int playerIndex, int teamIndex, string playerText, string groupText, ColorBlock colorBlock, Color indexColor, Color groupColor)
    {
        button.colors = colorBlock;
        playerNameUI.text = playerText;
        playerGroupUI.text = groupText;
        this.player = player;
        this.playerIndex = playerIndex;
        teamIndexUI.text = teamIndex.ToString() + ".";
        indexBG.color = indexColor;
        groupBG.color = groupColor;
    }

    public void UpdateColors (ColorBlock colorBlock, Color indexColor, Color groupColor)
    {
        button.colors = colorBlock;
        indexBG.color = indexColor;
        groupBG.color = groupColor;
    }

}
