using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class GroupUI : MonoBehaviour
{
    //public GridLayoutGroup testGrid;
    public TMP_Text groupLblUI;
    public GroupPlayerUI playerTemplate;
    public RectTransform playerListContainer;

    private void Start()
    {
        //Generate("GROUP K", 15, 3, testGrid);
    }

    public void Generate (string name, int playerCount, int winners, GridLayoutGroup grid)
    {
        groupLblUI.text = name;
        Rect playerSize = (playerTemplate.transform as RectTransform).rect;
        Rect lblSize = (groupLblUI.transform as RectTransform).rect;

        grid.cellSize = new Vector2(playerSize.width, playerSize.height * playerCount + lblSize.height);


        //(transform as RectTransform).SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, playerSize.width);
        //(transform as RectTransform).SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, playerSize.height * playerCount + lblSize.height);

        Vector2 currentPos = Vector2.zero;
        string winnerText = ":<size=70%> ADVANCES TO KO";

        

        for (int i = 0; i < playerCount; i++)
        {
            GroupPlayerUI newPlayer = Instantiate(playerTemplate.gameObject, playerListContainer.transform).GetComponent<GroupPlayerUI>();
            newPlayer.playerText.text = "PLAYER";
            newPlayer.transform.localPosition = currentPos;
            
            if (i < winners)
            {
                if (TournamentSetup.instance.tournamentTypeChosen.chosenStructure.numberOfQualifications == 0)
                {
                    winnerText = ":<size=70%> " + (i + 1).ToString() + ". PLACE";
                }
                newPlayer.playerText.text += winnerText;

                newPlayer.playerBackground.color = TournamentSetup.instance.typeUnlockedColor;
            }
            currentPos.y -= playerSize.height;
        }
    }


}
