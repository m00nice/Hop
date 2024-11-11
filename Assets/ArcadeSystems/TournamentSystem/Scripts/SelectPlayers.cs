using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SelectPlayers : MonoBehaviour
{
    public static List<SelectPlayers> instances = new List<SelectPlayers>();
    public PlayerSelect playerSelectTemplate;
    public RectTransform playerSelectContainer;
    public ColorBlock baseColors;
    public ColorBlock invalidColors;
    public int player;
    public AnimationCurve blinkCurve;
    EventSystem eventSystem;

    PlayerSelect lastSelect = null;

    List<PlayerSelect> playerUIs = new List<PlayerSelect>();

    public PlayerSelect CurrentlySelected
    {
        get
        {
            return lastSelect;
        }
    }


    public void Awake()
    {
        instances.Add(this);
    }

    private void OnDestroy()
    {
        instances.Remove(this);
    }

    private void Start()
    {
        if (player == 1)
        {
            baseColors.selectedColor = ActiveTournament.instance.player1Color;
            invalidColors.selectedColor = ActiveTournament.instance.player1Color;
        }
        else
        {
            baseColors.selectedColor = ActiveTournament.instance.player2Color;
            invalidColors.selectedColor = ActiveTournament.instance.player2Color;
        }
    }
    public void Populate (List<TournamentTeam> teams, ActiveTournament activeTournament)
    {
        for (int i = 0; i < playerUIs.Count; i++)
        {
            Destroy(playerUIs[i].gameObject);
        }
        playerUIs.Clear();

        if (activeTournament.groups.Count > 0)
        {
            List<TournamentTeam> allTeams = new List<TournamentTeam>();
            allTeams.AddRange(teams);
            allTeams.Sort((x, y) => x.name.CompareTo(y.name));

            for (int i = 0; i < allTeams.Count; i++)
            {
                playerUIs.Add(Instantiate(playerSelectTemplate.gameObject, playerSelectContainer).GetComponent<PlayerSelect>());
                ColorBlock btnColors = baseColors;
                if (allTeams[i].Battles.Find(c => c.status == BattleStatus.Pending) == null)
                {
                    btnColors.normalColor = ActiveTournament.instance.playerInvalidColor;
                }

                playerUIs[playerUIs.Count - 1].Set(allTeams[i], player, (i + 1), allTeams[i].name, "G: " + (allTeams[i].Group.groupIndex+1), btnColors, ActiveTournament.instance.playerInvalidColor, Color.black);
            }

            /*
            for (int i = 0; i < activeTournament.groups.Count; i++)
            {
                


                
                List<TournamentTeam> groupTeams = new List<TournamentTeam>();

                for (int j = 0; j < activeTournament.groups[i].teamIDs.Count; j++)
                {
                    groupTeams.Add(teams.Find(c => c.teamID == activeTournament.groups[i].teamIDs[j]));
                }
                groupTeams.Sort((x, y) => x.name.CompareTo(y.name));

                for (int j = 0; j < groupTeams.Count; j++)
                {
                    playerUIs.Add(Instantiate(playerSelectTemplate.gameObject, playerSelectContainer).GetComponent<PlayerSelect>());

                    playerUIs[playerUIs.Count - 1].Set(groupTeams[j], "Group " + (i + 1) + ": " + groupTeams[j].name, baseColors);
                }
                
            }
            */
        }
        else
        {
            List<TournamentTeam> allTeams = new List<TournamentTeam>();
            allTeams.AddRange(teams);
            allTeams.Sort((x, y) => x.name.CompareTo(y.name));

            for (int i = 0; i < allTeams.Count; i++)
            {
                playerUIs.Add(Instantiate(playerSelectTemplate.gameObject, playerSelectContainer).GetComponent<PlayerSelect>());

                playerUIs[playerUIs.Count - 1].Set(allTeams[i], player, (i + 1), allTeams[i].name, "", baseColors, ActiveTournament.instance.playerInvalidColor, Color.black);
            }
        }


    }

    //Party 10:40-11:30

    //August 11:30-12:30
    //Kasper 10:30-11:30

    public void SelectFirst ()
    {
        PlayerSelect first = playerUIs[0];
        eventSystem = (first.button as PlayerSpecificButton).eventSystem;
        Debug.Log("Is null?" + (eventSystem == null));
        (first.button as PlayerSpecificButton).eventSystem.SetSelectedGameObject(first.button.gameObject);
    }

    public TournamentBattle TestSelections (PlayerSelect other)
    {
        TournamentBattle nextBattle = null;
        for (int i = 0; i < playerUIs.Count; i++)
        {
            TournamentBattle battle = other.player.Battles.Find(c => c.status == BattleStatus.Pending && ((c.teamAID == other.player.teamID && c.teamBID == playerUIs[i].player.teamID) || (c.teamBID == other.player.teamID && c.teamAID == playerUIs[i].player.teamID)));

            if (battle != null && playerUIs[i] == lastSelect)
            {
                nextBattle = battle;
            }

            if (playerUIs[i].player.Group != other.player.Group || playerUIs[i].player == other.player || battle == null)
            {
                ColorBlock btnColors = invalidColors;
                if (playerUIs[i].player.Battles.Find(c => c.status == BattleStatus.Pending) == null)
                {
                    btnColors.normalColor = ActiveTournament.instance.playerInvalidColor;
                }
                playerUIs[i].UpdateColors(btnColors, ActiveTournament.instance.playerInvalidColor, Color.black);
            }
            else
            {
                ColorBlock btnColors = baseColors;
                if (playerUIs[i].player.Battles.Find(c => c.status == BattleStatus.Pending) == null)
                {
                    btnColors.normalColor = ActiveTournament.instance.playerInvalidColor;
                }



                playerUIs[i].UpdateColors(btnColors, ActiveTournament.instance.playerValidColor, Color.black);
            }
        }

        if (lastSelect != null && other != null && nextBattle != null)
        {
            if (player == 1)
            {
                ActiveTournament.instance.activePlayer1UI.text = lastSelect.player.name;
                ActiveTournament.instance.activePlayer2UI.text = other.player.name;
            }
            else
            {
                ActiveTournament.instance.activePlayer2UI.text = lastSelect.player.name;
                ActiveTournament.instance.activePlayer1UI.text = other.player.name;
            }

            ActiveTournament.instance.activePlayer1UI.color = ActiveTournament.instance.player1Color * 1.25f;
            ActiveTournament.instance.activePlayer2UI.color = ActiveTournament.instance.player2Color * 1.25f;
            ActiveTournament.instance.fightCenterTextUI.text = "vs";

            ActiveTournament.instance.activeBattle = nextBattle;
        }
        else
        {
            ActiveTournament.instance.activePlayer1UI.text = "";
            ActiveTournament.instance.activePlayer2UI.text = "";
            ActiveTournament.instance.active1Confirmed = false;
            ActiveTournament.instance.active2Confirmed = false;
            ActiveTournament.instance.fightCenterTextUI.text = "<size=70%>SELECTED PLAYERS HAVE NO FIGHTS";
            ActiveTournament.instance.activeBattle = null;
        }

        return nextBattle;
    }

    private void Update()
    {
        SelectionChange();

        if (lastSelect != null)
        {
            if (ArcadeInput.InputInitiated(player, ArcadeInputType.ButtonStart, AxisType.Raw, ActiveTournament.instance) || (ArcadeInput.InputInitiated(player, ArcadeInputType.ButtonA, AxisType.Raw, ActiveTournament.instance)))
            {
                if ((!ActiveTournament.instance.active1Confirmed || !ActiveTournament.instance.active2Confirmed) && ActiveTournament.instance.activeBattle != null)
                {
                    if (player == 1 && !ActiveTournament.instance.active1Confirmed)
                    {
                        Debug.Log("Confirm 1");
                        ActiveTournament.instance.active1Confirmed = true;
                        StartCoroutine(BlinkName());
                        ActiveTournament.instance.activePlayer1UI.text += "<size=55%><color=#FFFFFF>\r\nCONFIRMED";
                    }
                    else if (player == 2 && !ActiveTournament.instance.active2Confirmed)
                    {
                        Debug.Log("Confirm 2");
                        ActiveTournament.instance.active2Confirmed = true;
                        StartCoroutine(BlinkName());
                        ActiveTournament.instance.activePlayer2UI.text += "<size=55%><color=#FFFFFF>\r\nCONFIRMED";
                    }

                    if (ActiveTournament.instance.active1Confirmed && ActiveTournament.instance.active2Confirmed && ActiveTournament.instance.activeBattle != null)
                    {
                        Debug.Log("Inititate Battle");
                        ActiveTournament.instance.InitiateBattle(ActiveTournament.instance.activeBattle, ActiveTournament.instance.activePlayer1, ActiveTournament.instance.activePlayer2, -1, -1);
                    }
                }
            }
        }

    }

    IEnumerator BlinkName ()
    {
        Color start = Color.white;
        Color target = Color.white;
        if (player == 1)
        {
            start = ActiveTournament.instance.activePlayer1UI.color;
        }
        else
        {
            start = ActiveTournament.instance.activePlayer2UI.color;
        }

        float baseStart = Time.time;
        float startTime = Time.time;
        float maxTime = 1;
        float blinkDuration = .4f;
        while ((player == 1 && ActiveTournament.instance.active1Confirmed) || (player == 2 && ActiveTournament.instance.active2Confirmed))
        {
            float currentDuration = Time.time - startTime;
            if (currentDuration > blinkDuration)
            {
                currentDuration -= blinkDuration;
                startTime = Time.time + currentDuration;
            }
            float factor = blinkCurve.Evaluate(currentDuration / blinkDuration);

            if (player == 1)
            {
                ActiveTournament.instance.activePlayer1UI.color = Color.Lerp(start, target, factor);
            }
            else
            {
                ActiveTournament.instance.activePlayer2UI.color = Color.Lerp(start, target, factor);
            }
            yield return null;
        }
        if (player == 1)
        {
            ActiveTournament.instance.activePlayer1UI.color = start;
        }
        else
        {
            ActiveTournament.instance.activePlayer2UI.color = start;
        }
    }

    void SelectionChange ()
    {
        if (eventSystem != null)
        {
            if (eventSystem.currentSelectedGameObject != null && eventSystem.currentSelectedGameObject.GetComponentInParent<PlayerSelect>() != null)
            {
                if (lastSelect != eventSystem.currentSelectedGameObject.GetComponentInParent<PlayerSelect>())
                {
                    if (ActiveTournament.instance.lockPlayers)
                    {
                        if (lastSelect != null)
                        eventSystem.SetSelectedGameObject(lastSelect.button.gameObject);
                    } 
                    else
                    {
                        TournamentBattle bestBattle = null;
                        for (int i = 0; i < instances.Count; i++)
                        {
                            if (instances[i] != this)
                            {
                                bestBattle = instances[i].TestSelections(eventSystem.currentSelectedGameObject.GetComponentInParent<PlayerSelect>());
                            }
                        }
                        lastSelect = eventSystem.currentSelectedGameObject.GetComponentInParent<PlayerSelect>();

                        if (player == 1)
                        {
                            ActiveTournament.instance.active1Confirmed = false;
                            ActiveTournament.instance.activePlayer1 = lastSelect.player;
                        }
                        else
                        {
                            ActiveTournament.instance.active2Confirmed = false;
                            ActiveTournament.instance.activePlayer2 = lastSelect.player;
                        }


                        ActiveTournament.instance.UpdateActiveGroup(lastSelect, bestBattle);
                    }
                }
            }
        }
    }
}
