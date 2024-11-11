using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;
public class KnockoutPlayerUI : MonoBehaviour
{
    // Start is called before the first frame update
    public bool invertInOuts;
    public Image bg;
    public TMP_Text playerLblUI;
    public UILineRenderer connectionRenderer;
    public RectTransform leadInMount;
    public RectTransform leadOutMount;

    public Button player1Btn;
    public Button player2Btn;
    public GameObject player1BtnImage;
    public GameObject player2BtnImage;
    bool isSelectable = false;

    GameObject lastSelect1 = null;
    GameObject lastSelect2 = null;
    [NonSerialized]
    private KnockoutPlayer knockoutPlayerData;

    [NonSerialized]
    PlayerSpecificEventSystem system1 = null;
    [NonSerialized]
    PlayerSpecificEventSystem system2 = null;
    public KnockoutPlayer Data
    {
        set
        {
            knockoutPlayerData = value;
        }
    }

    private void LateUpdate()
    {
        if (isSelectable)
        {
            if (ActiveTournament.instance.lockPlayers)
            {
                if (ActiveTournament.instance.activePlayer1 == knockoutPlayerData.Player && system1.selectedObject != player1Btn.gameObject)
                {
                    system1.SetSelectedGameObject(player1Btn.gameObject);
                }

                if (ActiveTournament.instance.activePlayer2 == knockoutPlayerData.Player && system2.selectedObject != player2Btn.gameObject)
                {
                    system2.SetSelectedGameObject(player2Btn.gameObject);
                }
            } 
            else
            {
                if (system1.lastFrameSelectedObject == system1.selectedObject && system1.selectedObject == player1Btn.gameObject)
                {
                    if (system2.lastFrameSelectedObject != system2.selectedObject && system2.selectedObject == player2Btn.gameObject)
                    {
                        system2.SetSelectedGameObject(system2.lastFrameSelectedObject);
                    }
                    if (system2.currentSelectedGameObject != player2Btn.gameObject)
                    {
                        Navigation none = new Navigation();
                        none.mode = Navigation.Mode.None;
                        player2Btn.navigation = none;
                    }
                }
                else if (system2.lastFrameSelectedObject == system2.selectedObject && system2.selectedObject == player2Btn.gameObject)
                {
                    if (system1.lastFrameSelectedObject != system1.selectedObject && system1.selectedObject == player1Btn.gameObject)
                    {
                        system1.SetSelectedGameObject(system1.lastFrameSelectedObject);
                        //bg.color = player2Btn.colors.selectedColor;
                    }
                    if (system1.currentSelectedGameObject != player1Btn.gameObject)
                    {
                        Navigation none = new Navigation();
                        none.mode = Navigation.Mode.None;
                        player1Btn.navigation = none;
                    }
                        
                }



                if (system1.currentSelectedGameObject == player1Btn.gameObject)
                {
                    lastSelect1 = system1.currentSelectedGameObject;

                    ActiveTournament.instance.activePlayer1 = knockoutPlayerData.Player;
                    KnockoutStructure.instance.p1Selected = knockoutPlayerData;
                }
                else if (system1.currentSelectedGameObject != player1Btn.gameObject && ActiveTournament.instance.activePlayer1 == knockoutPlayerData.Player)
                {
                    //lastSelect1 = system1.currentSelectedGameObject;
                    //ActiveTournament.instance.activePlayer1 = null;
                }

                if (system2.currentSelectedGameObject == player2Btn.gameObject)
                {
                    lastSelect2 = system2.currentSelectedGameObject;
                    ActiveTournament.instance.activePlayer2 = knockoutPlayerData.Player;
                    KnockoutStructure.instance.p2Selected = knockoutPlayerData;
                }
                else if (system2.currentSelectedGameObject != player2Btn.gameObject && ActiveTournament.instance.activePlayer2 == knockoutPlayerData.Player)
                {
                    //lastSelect2 = system2.currentSelectedGameObject;
                    //ActiveTournament.instance.activePlayer2 = null;
                }

                if (system1.selectedObject != player1Btn.gameObject && system2.selectedObject != player2Btn.gameObject)
                {
                    Navigation automatic = new Navigation();
                    automatic.mode = Navigation.Mode.Automatic;
                    player1Btn.navigation = automatic;
                    player2Btn.navigation = automatic;
                    //player2Btn.gameObject.SetActive(true);
                    //player1Btn.gameObject.SetActive(true);
                }
            }


            
            
        }
    }

    public void CreateUI (KnockoutPlayerUI other, string labelText, bool showPlayerBtns)
    {
        
        playerLblUI.text = labelText;
        system1 = StandaloneArcadeInputModule.GetByID(1).GetComponent<PlayerSpecificEventSystem>();
        system2 = StandaloneArcadeInputModule.GetByID(2).GetComponent<PlayerSpecificEventSystem>();
        
        

        if (knockoutPlayerData.Player != null)
        {
            if (KnockoutStructure.instance.shiftCount > 1)
            {
                KnockoutStructure.instance.lastShift = Mathf.Abs(KnockoutStructure.instance.lastShift - 1);
                KnockoutStructure.instance.shiftCount = 0;
            }
            playerLblUI.text = knockoutPlayerData.Player.name;
            player1Btn.gameObject.SetActive(showPlayerBtns && knockoutPlayerData.Battle.status == BattleStatus.Pending && knockoutPlayerData.Battle.TeamA != null && knockoutPlayerData.Battle.TeamB != null);
            player2Btn.gameObject.SetActive(showPlayerBtns && knockoutPlayerData.Battle.status == BattleStatus.Pending && knockoutPlayerData.Battle.TeamA != null && knockoutPlayerData.Battle.TeamB != null);
            player1BtnImage.gameObject.SetActive(showPlayerBtns && knockoutPlayerData.Battle.status == BattleStatus.Pending && knockoutPlayerData.Battle.TeamA != null && knockoutPlayerData.Battle.TeamB != null);
            player2BtnImage.gameObject.SetActive(showPlayerBtns && knockoutPlayerData.Battle.status == BattleStatus.Pending && knockoutPlayerData.Battle.TeamA != null && knockoutPlayerData.Battle.TeamB != null);
            isSelectable = showPlayerBtns && knockoutPlayerData.Battle.status == BattleStatus.Pending && knockoutPlayerData.Battle.TeamA != null && knockoutPlayerData.Battle.TeamB != null;

            if (showPlayerBtns && knockoutPlayerData.Battle.status == BattleStatus.Pending && knockoutPlayerData.Battle.TeamA != null && knockoutPlayerData.Battle.TeamB != null)
            {
                ColorBlock p1Colors = player1Btn.colors;
                p1Colors.selectedColor = ActiveTournament.instance.player1Color;
                player1Btn.colors = p1Colors;

                ColorBlock p2Colors = player2Btn.colors;
                p2Colors.selectedColor = ActiveTournament.instance.player2Color;
                player2Btn.colors = p2Colors;
                

                bg.color = KnockoutStructure.instance.shiftingColorsPlayers[KnockoutStructure.instance.lastShift];
                
            }
            else
            {
                if (knockoutPlayerData.Battle.Winner != null && knockoutPlayerData.Battle.Winner == knockoutPlayerData.Player)
                {
                    bg.color = ActiveTournament.instance.battleWinnerColor;
                }
                else if (knockoutPlayerData.Battle.Winner != null && knockoutPlayerData.Battle.Winner != knockoutPlayerData.Player)
                {
                    bg.color = ActiveTournament.instance.battleLoserColor;
                    Color textColor = playerLblUI.color;
                    textColor.a = 0.4f;
                    playerLblUI.color = textColor;
                }

            }
            KnockoutStructure.instance.shiftCount++;
        }
        else
        {
            
            player1Btn.gameObject.SetActive(false);
            player2Btn.gameObject.SetActive(false);
            player1BtnImage.gameObject.SetActive(false);
            player2BtnImage.gameObject.SetActive(false);
            isSelectable = false;
            if (knockoutPlayerData.Battle != null)
            {
                playerLblUI.text = "TO BE ANOUNCED";
                bg.color = ActiveTournament.instance.closedPlayerColor;
            }
            else
            {
                
                playerLblUI.text = "";
                bg.color = Color.black;
            }
        }

        if (other != null)
        {
            RectTransform leadOut = leadOutMount;
            RectTransform otherLeadIn = other.leadInMount;

            Rect size = (other.transform as RectTransform).rect;

            Vector2 leadInDir = transform.position - other.transform.position;
            leadInDir.x = 0;
            leadInDir.Normalize();

            Vector2 leadIn = (Vector2)other.transform.position + leadInDir * size.height * .5f;

            if (invertInOuts)
            {
                leadOut = leadInMount;
                otherLeadIn = other.leadOutMount;
            }

            

            Vector2 otherInLocal = transform.worldToLocalMatrix.MultiplyPoint3x4(leadIn);
            Vector2 p1 = new Vector2(leadOut.localPosition.x, leadOut.localPosition.y);
            Vector2 p2 = new Vector2(otherInLocal.x, leadOut.localPosition.y);
            Vector2 p3 = new Vector2(otherInLocal.x, otherInLocal.y);
            //Vector2 p4 = new Vector2(otherInLocal.x, otherInLocal.y);

            connectionRenderer.Points = new Vector2[] { p1, p2, p3 };
            connectionRenderer.lineThickness = 2.5f;
            if (knockoutPlayerData.Battle != null)
            {

                if (knockoutPlayerData.Battle.Winner != null && knockoutPlayerData.Battle.Winner == knockoutPlayerData.Player)
                {
                    connectionRenderer.color = ActiveTournament.instance.battleWinnerColor;
                    connectionRenderer.lineThickness = 5;
                }
                else if (knockoutPlayerData.Battle.Winner != null && knockoutPlayerData.Battle.Winner != knockoutPlayerData.Player)
                {
                    connectionRenderer.color = ActiveTournament.instance.battleLoserColor;
                }
                else
                {
                    connectionRenderer.color = Color.white;
                }
            }
            
        }
        
    }
}
