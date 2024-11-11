using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using UnityEngine.EventSystems;
using System.Text;
using System.IO;
using System.Linq;

public enum TournamentState
{
    Undefined = 0,
    Subscription = 1,
    GroupPlay = 2,
    Knockout = 3,
    Finished = 4,
}

public enum ScoreSorting
{
    Undefined = 0,
    HighIsGood = 1,
    LowIsGood = 2,
}
public class NextBattleData
{
    public TournamentBattle battle;
    public TournamentTeam player1;
    public TournamentTeam player2;
    public TournamentTeam winner;
    public TournamentState tournamentState;
    public int player1Points;
    public int player2Points;

    public float player1Score;
    public float player2Score;

    public ScoreSorting scoreSorting;
    public string scoreFormatting = "0";
    public string battlePath;
}

[System.Serializable]
public class TestSort
{
    public string name;
    public int number;
    public float otherNumber;
    public TestSort(int number, float otherNumber)
    {
        this.number = number;
        this.otherNumber = otherNumber;
        name = "int: " + number + ", float: " + otherNumber.ToString("0.0");
    }
}

public class ActiveTournament : MonoBehaviour
{
    public delegate void NextBattle(NextBattleData battleData);

    public static ActiveTournament instance;
    public static int nextBattleInstanceID;
    public TournamentState tournamentState = TournamentState.Undefined;
    public TournamentType tournamentType;
    public RectTransform subscribePanel;
    public RectTransform activePanel;
    public RectTransform subscribedPlayersContainer;
    public SubscribedPlayerUI subscribedPlayerUITemplate;
    public TMP_Text tournamentNameUI;
    public TMP_Text gameNameUI;
    public TMP_Text groupTournamentNameUI;
    public TMP_Text groupGameNameUI;
    public TMP_Text playersTitleUI;
    public TMP_Text tournamentDescriptionUI;
    public TMP_Text subscriptionStateUI;

    public TMP_Text activePlayer1UI;
    public TMP_Text activePlayer2UI;
    public TMP_Text fightCenterTextUI;

    public TMP_Text koNextBattleUI1;
    public TMP_Text koNextBattleUI2;
    public TMP_Text koVsUI;

    public bool requirePin;
    public bool createRandomPlayerNames;
    public bool simulateFightsResults;
    public Button subscribeBtn;

    public Image player1Background;
    public Image player2Background;

    public SelectPlayers player1Selection;
    public SelectPlayers player2Selection;

    public ActiveGroupUI activeGroupUI;
    public BattleResultUI battleResultUI;
    public GroupKOTransitionUI groupKOTransitionUI;

    public CelebrationScreenUI celebrationScreenUI;

    public RectTransform knockoutPanel;
    public RectTransform knockoutStructureContainer;
    public KnockoutStructure knockoutStructureTemplate;

    public StartBattleUI startBattleUI;

    private KnockoutStructure knockoutStructure;



    public Color openPlayerColor;
    public Color closedPlayerColor;

    public Color battleWinnerColor;
    public Color battleLoserColor;
    public Color battleDrawColor;
    public Color battlePendingColor;
    public Color battleCancelledColor;
    public Color battleNextColor;

    public Color player1Color;
    public Color player2Color;
    public Color playerValidColor;
    public Color playerInvalidColor;

    public Color groupEntryDefaultBackground;
    public Color groupEntryWinnerBackground;

    public bool isFull;
    public bool isActive;
    public bool pureData;
    public string tournamentName;
    public string gameName;
    public string gameIdentifier;
    public string path;
    public ScoreSorting scoreSorting;
    public string scoreFormatting = "0";
    //public bool requirePin;
    public bool tournamentLoaded = false;
    public TournamentStatus status;
    public List<TournamentTeam> teams;
    public List<TournamentBattle> battles;
    public List<TournamentGroup> groups;
    public List<TournamentTeam> winners;
    
    public List<int> koBattles;

    public int groupWinners = -1;
    public int teamsPerGroup = -1;
    public int battlesPerGroup = -1;
    public int quals = -1;
    public int qualTeams = -1;
    public Canvas canvas;

    public DateTime creation = DateTime.MinValue;
    public DateTime activation = DateTime.MinValue;
    public DateTime finalization = DateTime.MinValue;

    public TournamentBattle activeBattle;
    public TournamentTeam activePlayer1;
    public TournamentTeam activePlayer2;
    [NonSerialized]
    private TournamentBattle battleInProgress;

    public bool active1Confirmed;
    public bool active2Confirmed;

    public bool lockPlayers = false;
    public int activeBattleInstanceID = 0;
    
    bool battleConfirmed;
    bool battleGo;
    bool groupToKOTransition;
    float lastFileProbe;
    Action callback;
    Action battleInitiate;
    Action tournamentClosing;
    List<SubscribedPlayerUI> subscribedPlayersUIs = new List<SubscribedPlayerUI>();
    int numberOfOpenTeamsLeft;

    int numberOfGroupPlaysLeft;
    int numberOfQualPlaysLeft;


    int simWinP = 3;
    int simLoseP = 0;
    int simDrawP = 1;
    private void Awake()
    {
        canvas.gameObject.SetActive(false);
    }

    void Start ()
    {
        player1Background.color = player1Color;
        player2Background.color = player2Color;
    }

    public TournamentBattle BattleInProgress
    {
        get
        {
            return battleInProgress;
        }
    }
    private void Update()
    {
        if (battleInProgress == null)
        {
            //Debug.Log(ArcadeInputRun.Instance.DebugModal());
            if (ArcadeInput.InputInitiated(0, ArcadeInputType.ButtonYellow, AxisType.Raw, this))
            {
                ArcadeInput.EndModal(this);
                tournamentClosing.Invoke();
                isActive = false;
                instance = null;
                JoystickKeyboard.GetByID(2).registerInputsFromPlayer = 1;
                Destroy(this.gameObject);
            }
        }
        //Debug.Log("Selected: " + (StandaloneArcadeInputModule.GetByID(0).GetComponent<EventSystem>().currentSelectedGameObject == null), StandaloneArcadeInputModule.GetByID(0).GetComponent<EventSystem>().currentSelectedGameObject);
        if (!simulateFightsResults)
        {
            if (activeBattleInstanceID > 0 && !battleConfirmed && lastFileProbe + 1 <= Time.time)
            {
                string pathLocal = ArcadeGlobals.SharedPath + "\\" + gameIdentifier + "\\" + "NextBattle\\battleConfirm" + activeBattleInstanceID + ".txt";
                lastFileProbe = Time.time;
                if (File.Exists(pathLocal))
                {
                    battleConfirmed = true;

                    string pathGoLocal = ArcadeGlobals.SharedPath + "\\" + gameIdentifier + "\\" + "NextBattle\\battleGo" + activeBattleInstanceID + ".txt";
                    List<string> fileDatas = new List<string>();
                    fileDatas.Add("battleGo:" + activeBattleInstanceID + ";");
                    ArcadeGlobals.WriteLinesToFile(fileDatas.ToArray(), pathGoLocal);
                }
            }

            if (activeBattleInstanceID > 0 && battleConfirmed && lastFileProbe + 1 <= Time.time)
            {
                string pathLocal = ArcadeGlobals.SharedPath + "\\" + gameIdentifier + "\\" + "NextBattle\\battleResult" + activeBattleInstanceID + ".txt";
                lastFileProbe = Time.time;
                while (File.Exists(pathLocal))
                {
                    
                    battleConfirmed = false;
                    bool everythingChecksOut = false;
                    List<string> nextLines = ArcadeGlobals.ReadLinesFromFile(pathLocal);
                    int currentLine = 0;
                    Dictionary<string, string> battleResult = ArcadeGlobals.GetPropertiesFromLine(nextLines[currentLine]);
                    {
                        if (battleResult.TryGetValue("battleID", out string value))
                        {
                            activeBattle.FillFromSave(nextLines[currentLine]);

                            currentLine++;
                            bool pointsNoError = false;
                            bool scoresNoError = false;
                            int p1 = 0;
                            int p2 = 0;
                            TournamentTeam winner = null;
                            Dictionary<string, string> points = ArcadeGlobals.GetPropertiesFromLine(nextLines[currentLine]);
                            {
                                if (points.TryGetValue("p1", out string p1Value))
                                {
                                    if (int.TryParse(p1Value, out int number))
                                    {
                                        if (points.TryGetValue("p2", out string p2Value))
                                        {

                                            if (int.TryParse(p2Value, out int number2))
                                            {
                                                
                                                everythingChecksOut = true;
                                                if (activeBattle.winnerID != -1)
                                                {
                                                    if (activePlayer1.teamID == activeBattle.winnerID)
                                                    {
                                                        winner = activePlayer1;
                                                        activePlayer1.wins++;
                                                        activePlayer2.loses++;
                                                        activeBattle.Winner = activePlayer1;
                                                    }
                                                    else if (activePlayer2.teamID == activeBattle.winnerID)
                                                    {
                                                        winner = activePlayer2;
                                                        activePlayer2.wins++;
                                                        activePlayer1.loses++;
                                                        activeBattle.Winner = activePlayer2;
                                                    }
                                                    else
                                                    {
                                                        Debug.Log("Somethings wrong!");
                                                    }
                                                }
                                                else
                                                {
                                                    if (activeBattle.status == BattleStatus.Draw)
                                                    {
                                                        activePlayer1.draws++;
                                                        activePlayer2.draws++;
                                                    }
                                                    else
                                                    {
                                                        Debug.Log("Something wrong");
                                                    }
                                                }

                                                
                                                p1 = number;
                                                p2 = number2;
                                                pointsNoError = true;
                                                
                                            }
                                        }
                                    }
                                }

                                

                            }
                            currentLine++;

                            float score1 = 0;
                            float score2 = 0;

                            Dictionary<string, string> scores = ArcadeGlobals.GetPropertiesFromLine(nextLines[currentLine]);
                            {
                                if (scores.TryGetValue("p1Score", out string p1ScoreValue))
                                {
                                    if (float.TryParse(p1ScoreValue, out float number))
                                    {
                                        score1 += number;
                                    }
                                    else if (float.TryParse(p1ScoreValue.Replace(",", "."), out float number2))
                                    {
                                        score1 += number2;
                                    }
                                }

                                if (scores.TryGetValue("p2Score", out string p2ScoreValue))
                                {
                                    if (float.TryParse(p2ScoreValue, out float number))
                                    {
                                        score2 += number;
                                    }
                                    else if (float.TryParse(p2ScoreValue.Replace(",", "."), out float number2))
                                    {
                                        score2 += number2;
                                    }
                                }

                                if (scores.TryGetValue("scoreSorting", out string scoreSortingValue))
                                {
                                    if (Enum.TryParse(scoreSortingValue, out ScoreSorting scoreSortingResult))
                                    {
                                        scoreSorting = scoreSortingResult;
                                    }
                                }
                                if (scores.TryGetValue("scoreFormatting", out string scoreFormattingValue))
                                {
                                    scoreFormatting = scoreFormattingValue;
                                }
                            }

                            activePlayer1.points += p1;
                            activePlayer2.points += p2;
                            activePlayer1.score += score1;
                            activePlayer2.score += score2;
                            CelebrateResult();

                            string basePath = ArcadeGlobals.SharedPath + "\\" + gameIdentifier + "\\" + "NextBattle\\";

                            File.Delete(basePath + "battleConfirm" + activeBattleInstanceID + ".txt");
                            File.Delete(basePath + "battleGo" + activeBattleInstanceID + ".txt");
                            File.Delete(basePath + "battleResult" + activeBattleInstanceID + ".txt");
                            File.Delete(basePath + "next.txt");
                            activeBattleInstanceID = 0;
                            UpdateUIWithNewResult(winner, activeBattle, p1, p2, score1, score2);
                            break;
                        }
                    }
                }
            }
        }
        else
        {
            if (activeBattleInstanceID > 0)
            {
                //Debug.Log("Testing for sim");
                TournamentTeam winner = null;
                bool finalize = false;
                //Debug.Log(ArcadeInputRun.Instance.DebugModal());
                if (ArcadeInput.InputInitiated(1, ArcadeInputType.ButtonD, AxisType.Raw, this))
                {
                    //1wins
                    Debug.Log("1 wins");
                    finalize = true;
                    winner = activePlayer1;
                    activeBattle.status = BattleStatus.Won;
                    activeBattle.Winner = activePlayer1;
                }
                else if (ArcadeInput.InputInitiated(1, ArcadeInputType.ButtonE, AxisType.Raw, this))
                {
                    //2 wins
                    Debug.Log("2 wins");
                    finalize = true;
                    winner = activePlayer2;
                    activeBattle.status = BattleStatus.Won;
                    activeBattle.Winner = activePlayer2;
                }
                else if (ArcadeInput.InputInitiated(1, ArcadeInputType.ButtonF, AxisType.Raw, this))
                {
                    //draw;
                    Debug.Log("Draw");
                    finalize = true;
                    activeBattle.status = BattleStatus.Draw;
                }

                if (finalize)
                {
                    
                    int number = 0;
                    int number2 = 0;
                    float score = 0;
                    float score2 = 0;
                    if (simulateFightsResults && scoreSorting == ScoreSorting.Undefined)
                    {
                        scoreSorting = ScoreSorting.LowIsGood;
                    }
                    if (activeBattle.winnerID != -1)
                    {
                        if (activePlayer1.teamID == activeBattle.winnerID)
                        {
                            winner = activePlayer1;
                            activePlayer1.wins++;
                            activePlayer2.loses++;
                            number = simWinP;
                            number2 = simLoseP;

                            if (scoreSorting == ScoreSorting.HighIsGood)
                            {
                                score = UnityEngine.Random.Range(100.0f, 200.0f);
                                score2 = UnityEngine.Random.Range(50.0f, 99.0f);
                            }
                            else
                            {
                                score = UnityEngine.Random.Range(50.0f, 99.0f);
                                score2 = UnityEngine.Random.Range(100.0f, 200.0f);
                            }

                            activeBattle.Winner = activePlayer1;
                        }
                        else if (activePlayer2.teamID == activeBattle.winnerID)
                        {
                            winner = activePlayer2;
                            activePlayer2.wins++;
                            activePlayer1.loses++;
                            number2 = simWinP;
                            number = simLoseP;

                            if (scoreSorting == ScoreSorting.HighIsGood)
                            {
                                score2 = UnityEngine.Random.Range(100.0f, 200.0f);
                                score = UnityEngine.Random.Range(50.0f, 99.0f);
                            }
                            else
                            {
                                score2 = UnityEngine.Random.Range(50.0f, 99.0f);
                                score = UnityEngine.Random.Range(100.0f, 200.0f);
                            }

                            activeBattle.Winner = activePlayer2;
                        }
                        else
                        {
                            Debug.Log("Somethings wrong!");
                        }
                    }
                    else
                    {
                        if (activeBattle.status == BattleStatus.Draw)
                        {
                            number2 = simDrawP;
                            number = simDrawP;
                            activePlayer1.draws++;
                            activePlayer2.draws++;
                            float sharedScore = UnityEngine.Random.Range(50.0f, 99.0f);
                            score = sharedScore;
                            score2 = sharedScore;
                        }
                        else
                        {
                            Debug.Log("Something wrong");
                        }
                    }

                    activePlayer1.points += number;
                    activePlayer2.points += number2;
                    activePlayer1.score += score;
                    activePlayer2.score += score2;
                    CelebrateResult();
                    activeBattleInstanceID = 0;
                    UpdateUIWithNewResult(winner, activeBattle, number, number2, score, score2);
                }
            }
        }
        
        
    }

    TournamentBattle FirstOpen ()
    {
        return battles.Find(c => c.status == BattleStatus.Pending && c.TeamA != null && c.TeamB != null);
    }

    public void CelebrateResult ()
    {
        if (activePlayer1.Group != null && tournamentState == TournamentState.GroupPlay)
        {
            SortGroupTeams(activePlayer1.Group.Teams, scoreSorting);
            numberOfGroupPlaysLeft = battles.FindAll(c => c.battleType == BattleType.Group && c.status == BattleStatus.Pending).Count;
            int numOfKos = battles.FindAll(c => c.battleType == BattleType.Knockout && c.status == BattleStatus.Pending).Count;
            if (numberOfGroupPlaysLeft == 0 && numOfKos > 0)
            {
                int startBattleKOIndex = battlesPerGroup * groups.Count;
                Debug.Log("startBattleKOIndex: " + startBattleKOIndex);
                List<TournamentTeam> koTeams = new List<TournamentTeam>();
                for (int i = 0; i < groups.Count; i++)
                {
                    for (int j = 0; j < groupWinners; j++)
                    {
                        koTeams.Add(groups[i].Teams[j]);


                    }
                }
                Shuffle(koTeams);

                bool a = true;
                for (int i = 0; i < koTeams.Count; i++)
                {
                    if (a)
                    {
                        battles[startBattleKOIndex].TeamA = koTeams[i];
                        a = false;
                    }
                    else
                    {
                        battles[startBattleKOIndex].TeamB = koTeams[i];
                        a = true;
                        startBattleKOIndex++;
                    }
                }
            }
        }
        
        TournamentState preState = tournamentState;

        if (activeBattle.Winner != null && tournamentState == TournamentState.Knockout)
        {
            if (activeBattle.nextBattle > -1)
            {
                TournamentBattle nextBattle = battles.Find(c => c.battleID == activeBattle.nextBattle);

                if (activeBattle.battleID == nextBattle.leadupBattles[0])
                {
                    nextBattle.TeamA = activeBattle.Winner;
                }
                else if (activeBattle.battleID == nextBattle.leadupBattles[1])
                {
                    nextBattle.TeamB = activeBattle.Winner;
                }
            }
            else
            {
                Debug.Log("No Next battle!");
            }
        }


        SaveTournament();

        if (preState != tournamentState)
        {
            if (preState == TournamentState.GroupPlay && tournamentState == TournamentState.Knockout)
            {
                groupToKOTransition = true;
            }
        }

        battleInProgress = null;
        active1Confirmed = false;
        active2Confirmed = false;
    }

    public void UpdateUIWithNewResult (TournamentTeam winner, TournamentBattle battle, int p1, int p2, float score1, float score2)
    {
        StartCoroutine(LoadBattleResult(winner, battle, p1, p2, score1, score2));
    }

    IEnumerator LoadBattleResult (TournamentTeam winner, TournamentBattle battle, int p1, int p2, float score1, float score2)
    {
        float startTime = Time.time;
        float delay = 1;
        while(startTime+delay> Time.time)
        {
            yield return null;
        }

        ShowGroupBattleResult(winner, battle, p1, p2, score1, score2, tournamentState);
    }

    public void ShowGroupBattleResult (TournamentTeam winner, TournamentBattle battle, int p1, int p2, float score1, float score2, TournamentState state)
    {
        string mainMessage = "";
        string awards = "";

        string colorHex = "";
        Color player1Color = ActiveTournament.instance.player1Color;
        string player1ColorHex = "<color=#" + RGBToHex(Mathf.RoundToInt(player1Color.r * 255.0f)) + RGBToHex(Mathf.RoundToInt(player1Color.g * 255.0f)) + RGBToHex(Mathf.RoundToInt(player1Color.b * 255.0f)) + ">";

        Color player2Color = ActiveTournament.instance.player2Color;
        string player2ColorHex = "<color=#" + RGBToHex(Mathf.RoundToInt(player2Color.r * 255.0f)) + RGBToHex(Mathf.RoundToInt(player2Color.g * 255.0f)) + RGBToHex(Mathf.RoundToInt(player2Color.b * 255.0f)) + ">";

        string whiteHex = "<color=#FFFFFF>";

        string player1Name = player1ColorHex + activePlayer1.name + whiteHex;
        string player2Name = player2ColorHex + activePlayer2.name + whiteHex;
        int p1Placement = -1;
        int p2Placement = -1;
        
        if (groups.Count > 0)
        {
            TournamentGroup group = activePlayer1.Group;
            List<TournamentTeam> gTeams = new List<TournamentTeam>();
            gTeams.AddRange(group.Teams);
            gTeams.Sort((x, y) => y.points.CompareTo(x.points));

            p1Placement = gTeams.IndexOf(activePlayer1);
            p2Placement = gTeams.IndexOf(activePlayer2);
        }
        
        string scoreString = "";
        if (battle.status == BattleStatus.Won)
        {
            if (winner == activePlayer1)
            {
                colorHex = player1ColorHex;
                scoreString = "WITH A SCORE OF " + score1.ToString(scoreFormatting) + " AGAINST " + score2.ToString(scoreFormatting);
            }
            else
            {
                colorHex = player2ColorHex;
                scoreString = "WITH A SCORE OF " + score2.ToString(scoreFormatting) + " AGAINST " + score1.ToString(scoreFormatting);
            }

            mainMessage = "BATTLE FINISHED\r\n<size=150%>" + colorHex + winner.name + whiteHex + " WINS<size=65%>\r\n" + scoreString;
        }
        else if (battle.status == BattleStatus.Draw)
        {
            mainMessage = "BATTLE FINISHED\r\n<size=150%>RESULT WAS A DRAW<size=65%>\r\nBOTH PLAYERS SCORED " + score1.ToString(scoreFormatting);
        }

        if (state == TournamentState.GroupPlay || groupToKOTransition)
        {
            awards = player1Name + " IS AWARDED " + p1 + " POINTS\r\n<size=60%>THAT PUTS " + player1Name + " IN " + (p1Placement + 1) + ". PLACE IN GROUP\r\n<size=100%>" + player2Name + " IS AWARDED " + p2 + " POINTS\r\n<size=60%>THAT PUTS " + player2Name + " IN " + (p2Placement + 1) + ". PLACE IN GROUP";
            battleResultUI.SetResult(mainMessage, awards, battle);
        }
        else if (state == TournamentState.Knockout && !groupToKOTransition)
        {
            if (battle.status != BattleStatus.Won)
            {
                Debug.Log("A KO fight can't end in a draw!!!!");
            }
            else
            {
                if (tournamentState != TournamentState.Finished)
                {
                    if (battle.Winner == activePlayer1)
                    {
                        awards = "\r\n" + player1Name + "\r\nWILL PROCEED TO NEXT ROUND!";
                    }
                    else
                    {
                        awards = "\r\n" + player2Name + "\r\nWILL PROCEED TO NEXT ROUND!";
                    }
                }
                else
                {
                    if (battle.Winner == activePlayer1)
                    {
                        awards = "\r\n" + player1Name + "\r\nHAS WON THE TOURNAMENT!";
                    }
                    else
                    {
                        awards = "\r\n" + player2Name + "\r\nHAS WON THE TOURNAMENT!";
                    }
                }
                
                

                battleResultUI.SetResult(mainMessage, awards, battle);
            }

        }
        else if (state == TournamentState.Finished && state == tournamentState)
        {
            if (battle.battleType == BattleType.Knockout)
            {
                ShowGroupBattleResult(winner, battle, p1, p2, score1, score2, TournamentState.Knockout);

            }
            else if (battle.battleType == BattleType.Group)
            {
                ShowGroupBattleResult(winner, battle, p1, p2, score1, score2, TournamentState.GroupPlay);
            }
        }
        
    }

    public void FinalizeAfterBattle (TournamentBattle lastBattle)
    {
        if (tournamentState == TournamentState.GroupPlay || groupToKOTransition)
        {
            UpdateActiveGroup(activePlayer1, activeBattle);
            SelectPlayers.instances[1].TestSelections(SelectPlayers.instances[0].CurrentlySelected);
            SelectPlayers.instances[0].TestSelections(SelectPlayers.instances[1].CurrentlySelected);
        }
        else if (tournamentState == TournamentState.Knockout && !groupToKOTransition)
        {
            GenerateKnockoutStructure();

            StandaloneArcadeInputModule.GetByID(1).GetComponent<PlayerSpecificEventSystem>().SetSelectedGameObject(null);
            StandaloneArcadeInputModule.GetByID(2).GetComponent<PlayerSpecificEventSystem>().SetSelectedGameObject(null);
            StandaloneArcadeInputModule.GetByID(1).GetComponent<PlayerSpecificEventSystem>().lastFrameSelectedObject = null;
            StandaloneArcadeInputModule.GetByID(1).GetComponent<PlayerSpecificEventSystem>().lastSelectedObject = null;
            StandaloneArcadeInputModule.GetByID(1).GetComponent<PlayerSpecificEventSystem>().selectedObject = null;
            StandaloneArcadeInputModule.GetByID(2).GetComponent<PlayerSpecificEventSystem>().lastFrameSelectedObject = null;
            StandaloneArcadeInputModule.GetByID(2).GetComponent<PlayerSpecificEventSystem>().lastSelectedObject = null;
            StandaloneArcadeInputModule.GetByID(2).GetComponent<PlayerSpecificEventSystem>().selectedObject = null;
            SelectNextKO();
        }
        else if (tournamentState == TournamentState.Finished)
        {


        }
        
        Invoke("ReactivateUI", 2);
    }

    public void GoToKO ()
    {
        Invoke("ActivateKO", 1);
    }

    public void ActivateKO ()
    {
        
        SaveTournament();

        UpdateSystemUIStatus();

        //SelectNextKO();
        lockPlayers = false;
    }

    void SelectNextKO ()
    {
        TournamentBattle firstKOBattle = FirstOpen();
        if (firstKOBattle != null && knockoutStructure != null)
        {
            Debug.Log("First open: " + firstKOBattle.battleID);
            List<KnockoutPlayer> koPlayers = knockoutStructure.GetKOByBattle(firstKOBattle);

            if (koPlayers.Count == 2)
            {
                Debug.Log("Select btn1", koPlayers[1].UI.player1Btn);
                Debug.Log("Select btn1", koPlayers[0].UI.player2Btn);
                activePlayer1 = null;
                activePlayer2 = null;
                StandaloneArcadeInputModule.GetByID(1).GetComponent<PlayerSpecificEventSystem>().SetSelectedGameObject(koPlayers[1].UI.player1Btn.gameObject);
                StandaloneArcadeInputModule.GetByID(2).GetComponent<PlayerSpecificEventSystem>().SetSelectedGameObject(koPlayers[0].UI.player2Btn.gameObject);
                //StandaloneArcadeInputModule.GetByID(1).modalObject = this;
                //StandaloneArcadeInputModule.GetByID(2).modalObject = this;
                Debug.Log(ArcadeInputRun.Instance.DebugModal());
                Debug.Log("Module1: " + StandaloneArcadeInputModule.GetByID(1).modalObject, (StandaloneArcadeInputModule.GetByID(1).modalObject as MonoBehaviour));
                Debug.Log("Module2: " + StandaloneArcadeInputModule.GetByID(2).modalObject, (StandaloneArcadeInputModule.GetByID(2).modalObject as MonoBehaviour));
            }

        }
    }

    public void Shuffle<T>(List<T> list)
    {
        System.Random rng = new System.Random(Mathf.RoundToInt(Time.time));
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    private string RGBToHex(int input)
    {
        if (input <= 0)
            return "00";

        int hex = input;
        string hexStr = string.Empty;

        while (input > 0)
        {
            hex = input % 16;

            if (hex < 10)
                hexStr = hexStr.Insert(0, Convert.ToChar(hex + 48).ToString());
            else
                hexStr = hexStr.Insert(0, Convert.ToChar(hex + 55).ToString());

            input /= 16;
        }

        if (hexStr.Length == 1)
        {
            hexStr = "0" + hexStr;
        }

        return hexStr;
    }

    void ReactivateUI ()
    {
        
        UpdateTournamentState();

        if (groupToKOTransition)
        {
            groupKOTransitionUI.Set();
            groupToKOTransition = false;
        }
        else
        {
            lockPlayers = false;
        }

        if (tournamentState == TournamentState.Finished)
        {

            SetFinalCelebration();


        }
    }

    void SetFinalCelebration ()
    {
        if (battles[battles.Count - 1].battleType == BattleType.Group)
        {
            string main = "TOURNAMENT HAS FINISHED!";
            string secondary = "";

            if (groups.Count > 1 || groupWinners > 1)
            {
                secondary = "WINNERS OF GROUP PLAY\r\n<size=120%>";
            }
            else
            {
                secondary = "WINNER OF GROUP PLAY\r\n<size=120%>";
            }

            for (int i = 0; i < groups.Count; i++)
            {
                SortGroupTeams(groups[i].Teams, scoreSorting);

                for (int j = 0; j < groupWinners; j++)
                {
                    secondary += groups[i].Teams[j].name;
                    winners.Add(groups[i].Teams[j]);
                    if (i < groups.Count-1 || j < groupWinners-1)
                    {
                        secondary += ", ";
                    }
                }
            }

            if (groups.Count > 1 || groupWinners > 1)
            {
                secondary += "<size=100%>\r\nCONGRATULATIONS TO ALL WINNERS...";
            }
            else
            {
                secondary += "<size=100%>\r\nCONGRATULATION TO THE WINNER...";
            }
            

            celebrationScreenUI.Set(main, secondary);
        }
        else
        {
            string main = "TOURNAMENT HAS FINISHED!";
            string secondary = "WINNER\r\n" + battles[battles.Count - 1].Winner.name;
            celebrationScreenUI.Set(main, secondary);
            winners.Add(battles[battles.Count - 1].Winner);
        }
        SaveTournament();
    }

   
    public void CancelFight ()
    {
        active1Confirmed = false;
        active2Confirmed = false;
        lockPlayers = false;
    }

    public void InitiateBattle (TournamentBattle battle, TournamentTeam player1, TournamentTeam player2, int pin1, int pin2)
    {
        if ((battle != null && battle.status != BattleStatus.Pending) || battle == battleInProgress)
        {
            Debug.Log("This battle is already done or is in progress!");
            return;
        }
        lockPlayers = true;
        if (requirePin && (pin1 != activePlayer1.pin || pin2 != activePlayer2.pin))
        {
            if (JoystickKeyboard.GetByID(2) != null)
            {
                JoystickKeyboard activePinKeyboard = JoystickKeyboard.GetByID(2);
                if (pin1 != activePlayer1.pin)
                {
                    string dots = "";
                    for (int i = 0; i < activePlayer1.pin.ToString().Length-2; i++)
                    {
                        dots += ".";
                    }
                    activePinKeyboard.automaticAcceptWhenValid = true;
                    activePinKeyboard.Title = activePlayer1.name + "\r\nINPUT YOUR 4 DIGIT PIN";
                    activePinKeyboard.HelpText = "USE BUTTONS TO INPUT DIGITS\r\nJOYSTICK LEFT TO DELETE\r\n<color=#FF0000><b>JOYSTICK DOWN TO CANCEL BATTLE";
                    activePinKeyboard.registerInputsFromPlayer = 1;
                    activePinKeyboard.validation = activePlayer1.pin.ToString();
                    activePinKeyboard.BackgroundColor = player1Color;
                    activePinKeyboard.cancelAction = (() => CancelFight());
                    activePinKeyboard.validationFailedString = "WRONG PIN! HINT: " + activePlayer1.pin.ToString().Substring(0, 1) + dots + activePlayer1.pin.ToString().Substring(activePlayer1.pin.ToString().Length-1, 1);
                    activePinKeyboard.Activate(() => InitiateBattle(battle, player1, player2, int.Parse(activePinKeyboard.GetCurrentText()), pin2));
                }
                else if (pin2 != activePlayer2.pin)
                {
                    
                    Debug.Log("Now for PIN 2!");
                    string dots = "";
                    for (int i = 0; i < activePlayer2.pin.ToString().Length - 2; i++)
                    {
                        dots += ".";
                    }
                    activePinKeyboard.automaticAcceptWhenValid = true;
                    activePinKeyboard.Title = activePlayer2.name + "\r\nINPUT YOUR 4 DIGIT PIN";
                    activePinKeyboard.HelpText = "USE BUTTONS TO INPUT DIGITS\r\nJOYSTICK LEFT TO DELETE\r\n<color=#FF0000><b>JOYSTICK DOWN TO CANCEL BATTLE";
                    activePinKeyboard.registerInputsFromPlayer = 2;
                    activePinKeyboard.BackgroundColor = player2Color;
                    activePinKeyboard.validation = activePlayer2.pin.ToString();
                    activePinKeyboard.cancelAction = (() => CancelFight());
                    activePinKeyboard.validationFailedString = "WRONG PIN! HINT: " + activePlayer2.pin.ToString().Substring(0, 1) + dots + activePlayer2.pin.ToString().Substring(activePlayer2.pin.ToString().Length - 1, 1);
                    activePinKeyboard.Activate(() => InitiateBattle(battle, player1, player2, pin1, int.Parse(activePinKeyboard.GetCurrentText())));
                }
            }
            return;
        }
        activeBattleInstanceID = UnityEngine.Random.Range(1000000, 100000000);
        battleInProgress = battle;

        startBattleUI.Set(battle, player1, player2);

        if (!simulateFightsResults)
        {
            string pathLocal = ArcadeGlobals.SharedPath + "\\" + gameIdentifier + "\\" + "NextBattle";

            if (!Directory.Exists(pathLocal))
            {
                Directory.CreateDirectory(pathLocal);
            }
            pathLocal += "\\next.txt";
            List<string> fileDatas = new List<string>();
            fileDatas.Add("instanceID:" + activeBattleInstanceID + ";");

            fileDatas.Add(battle.GetSave());
            fileDatas.Add(player1.GetSave());
            fileDatas.Add(player2.GetSave());
            fileDatas.Add("tournamentState:" + tournamentState + ";");
            ArcadeGlobals.WriteLinesToFile(fileDatas.ToArray(), pathLocal);
        }
        if (battleInitiate != null)
        {
            battleInitiate.Invoke();
        }

    }


    void GenerateKnockoutStructure()
    {
        KnockoutStructure newKnockoutStructure = null;

        if (knockoutStructure == null)
        {
            newKnockoutStructure = Instantiate(knockoutStructureTemplate.gameObject, knockoutStructureContainer).GetComponent<KnockoutStructure>();
            newKnockoutStructure.transform.localPosition = Vector2.zero;
            Rect containerSize = knockoutStructureContainer.rect;
            newKnockoutStructure.knockoutArea.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, containerSize.width);
            newKnockoutStructure.knockoutArea.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, containerSize.height);
            knockoutStructure = newKnockoutStructure;
        }
        else
        {
            newKnockoutStructure = knockoutStructure;
        }

        newKnockoutStructure.BuildKnockoutStructure(qualTeams);
    }
    public void AddSubscribedPlayer (SubscribedPlayerUI player)
    {
        subscribedPlayersUIs.Add(player);
    }
    
    public void UpdateSubscribedPlayers ()
    {
        for (int i = 0; i < subscribedPlayersUIs.Count; i++)
        {
            subscribedPlayersUIs[i].UpdateName(i + 1);
        }
    }

    public void UpdateTournamentState ()
    {
        numberOfOpenTeamsLeft = teams.FindAll(c => c.teamStatus == TeamStatus.Open).Count;
        numberOfGroupPlaysLeft = battles.FindAll(c => c.battleType == BattleType.Group && c.status == BattleStatus.Pending).Count;
        numberOfQualPlaysLeft = battles.FindAll(c => c.battleType == BattleType.Knockout && c.status == BattleStatus.Pending).Count;

        if (numberOfOpenTeamsLeft > 0)
        {
            tournamentState = TournamentState.Subscription;
        }
        else if (numberOfGroupPlaysLeft > 0 && numberOfOpenTeamsLeft == 0)
        {
            if (tournamentState == TournamentState.Subscription)
            {
                activation = DateTime.Now;
            }
            tournamentState = TournamentState.GroupPlay;
        }
        else if (numberOfQualPlaysLeft > 0 && numberOfGroupPlaysLeft == 0 && numberOfOpenTeamsLeft == 0)
        {
            tournamentState = TournamentState.Knockout;
        }
        else if (numberOfQualPlaysLeft == 0 && numberOfGroupPlaysLeft == 0 && numberOfOpenTeamsLeft == 0)
        {
            if (tournamentState == TournamentState.GroupPlay || tournamentState == TournamentState.Knockout)
            {
                finalization = DateTime.Now;
            }

            tournamentState = TournamentState.Finished;
        }
        //Debug.Log("Current State: " + tournamentState);
    }

    public static bool TryActivate (Action callback, Action battleInitiate, Action tournamentClosing, string gameIdentifier, string name, TournamentStatus status, bool pureData)
    {
        if (instance != null)
        {
            Destroy(instance.gameObject);
            instance = null;
        }


        if (instance == null)
        {
            instance = (Instantiate(Resources.Load("ActiveTournament")) as GameObject).GetComponent<ActiveTournament>();
            UnityEngine.Object.DontDestroyOnLoad(instance.gameObject);

            //instance.gameObject.hideFlags = HideFlags.HideAndDontSave;
        }
        instance.subscribePanel.gameObject.SetActive(false);


        List<TournamentFileData> validTournaments = Tournament.GetAllTournamentDatas(gameIdentifier, status);
        Debug.Log("valids: " + validTournaments.Count);
        if (validTournaments != null)
        {
            for (int i = 0; i < validTournaments.Count; i++)
            {
                if (validTournaments[i].name == name)
                {
                    instance.pureData = pureData;
                    if (!pureData)
                    {
                        instance.canvas.gameObject.SetActive(true);
                    }
                    
                    instance.isActive = true;
                    
                    bool succededLoad = instance.LoadTournament(validTournaments[i].path);

                    if (succededLoad && !pureData)
                    {
                        

                        ArcadeInput.SetModal(new ModalData(instance, false, new StandaloneArcadeInputModule[] { StandaloneArcadeInputModule.GetByID(0), StandaloneArcadeInputModule.GetByID(1), StandaloneArcadeInputModule.GetByID(2) }));
                        if (instance.tournamentState == TournamentState.Subscription)
                        {
                            if (instance.createRandomPlayerNames)
                            {
                                instance.CreateRandomPlayerNames();
                            }
                            instance.CreatePlayersUI();
                        }
                        
                        instance.UpdateSystemUIStatus();
                        instance.battleInitiate = battleInitiate;
                        instance.callback = callback;
                        instance.tournamentClosing = tournamentClosing;
                        if (instance.tournamentState == TournamentState.Finished)
                        {
                            instance.SetFinalCelebration();
                        }
                    }
                    return succededLoad;
                }
            }
        }

        return false;
    }

    public static bool GetBattle (NextBattle callback)
    {
        string localGameIdentifier = Application.companyName + "." + Application.productName;
        string directory = ArcadeGlobals.SharedPath + "\\" + localGameIdentifier + "\\" + "NextBattle";

        if (Directory.Exists(directory))
        {
            if (File.Exists(directory + "\\next.txt"))
            {
                List<string> nextLines = ArcadeGlobals.ReadLinesFromFile(directory + "\\next.txt");
                int currentLine = 0;
                Dictionary<string, string> instanceIDLine = ArcadeGlobals.GetPropertiesFromLine(nextLines[currentLine]);
                {
                    if (instanceIDLine.TryGetValue("instanceID", out string value))
                    {
                        if (int.TryParse(value, out int number))
                        {
                            nextBattleInstanceID = number;

                            GameObject battleHandShake = new GameObject("BattleHandShake");
                            UnityEngine.Object.DontDestroyOnLoad(battleHandShake);
                            battleHandShake.hideFlags = HideFlags.HideAndDontSave;
                            BattleHandShake newHandShake = battleHandShake.AddComponent<BattleHandShake>();
                            newHandShake.StartCoroutine(newHandShake.ShakeHands(callback, directory + "\\next.txt", nextLines));
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    public static void CreateBattleResult (NextBattleData battleData)
    {
        string localGameIdentifier = Application.companyName + "." + Application.productName;
        string pathLocal = ArcadeGlobals.SharedPath + "\\" + localGameIdentifier + "\\" + "NextBattle";

        if (Directory.Exists(pathLocal))
        {
            pathLocal += "\\battleResult" + nextBattleInstanceID + ".txt";

            List<string> fileDatas = new List<string>();

            if (battleData.winner != null)
            {
                battleData.battle.winnerID = battleData.winner.teamID;
                battleData.battle.status = BattleStatus.Won;
            }
            else
            {
                battleData.battle.status = BattleStatus.Draw;
            }
            fileDatas.Add(battleData.battle.GetSave());

            fileDatas.Add("p1:" + battleData.player1Points + ";p2:" + battleData.player2Points + ";");
            fileDatas.Add("p1Score:" + battleData.player1Score + ";p2Score:" + battleData.player2Score + ";scoreSorting:" + battleData.scoreSorting + ";scoreFormatting:" + battleData.scoreFormatting + ";");

            ArcadeGlobals.WriteLinesToFile(fileDatas.ToArray(), pathLocal);
            //instance.SaveTournament();
        }
    }

    public void CreateRandomPlayerNames ()
    {
        System.Random rdm = new System.Random();
        for (int i = 0; i < teams.Count; i++)
        {
            teams[i].name = RandomString(rdm);
            if (requirePin)
            {
                teams[i].pin = UnityEngine.Random.Range(1000, 10000);
            } 
            else
            {
                teams[i].pin = -1;
            }

            
            teams[i].teamStatus = TeamStatus.Closed;
            
        }
        UpdateSystemUIStatus();
        SaveTournament();
    }

    string RandomString(System.Random rdm)
    {
        int length = UnityEngine.Random.Range(3, 10);

        // creating a StringBuilder object()
        StringBuilder str_build = new StringBuilder();
        
        char letter;

        for (int i = 0; i < length; i++)
        {
            double flt = rdm.NextDouble();
            int shift = Convert.ToInt32(Math.Floor(25 * flt));
            letter = Convert.ToChar(shift + 65);
            str_build.Append(letter);
        }
        return str_build.ToString();
    }

    public void CreatePlayersUI ()
    {
        for (int j = 0; j < teams.Count; j++)
        {
            SubscribedPlayerUI subscribedPlayer = Instantiate(subscribedPlayerUITemplate.gameObject, subscribedPlayersContainer).GetComponent<SubscribedPlayerUI>();
            subscribedPlayer.Set(teams[j]);
            AddSubscribedPlayer(subscribedPlayer);

        }
    }

    public void UpdateSystemUIStatus ()
    {
        subscribePanel.gameObject.SetActive(false);
        activePanel.gameObject.SetActive(false);
        knockoutPanel.gameObject.SetActive(false);
        StandaloneArcadeInputModule.GetByID(0).GetComponent<EventSystem>().SetSelectedGameObject(null);
        if (tournamentState == TournamentState.Subscription)
        {
            UpdateSubscribedPlayers();
            playersTitleUI.text = "PLAYERS: " + teams.Count;
            tournamentNameUI.text = "\"" + tournamentName + "\"";
            gameNameUI.text = "GAME: " + gameName;
            subscribePanel.gameObject.SetActive(true);
            if (StandaloneArcadeInputModule.GetByID(0).GetComponent<EventSystem>().currentSelectedGameObject != subscribeBtn.gameObject)
            {
                StandaloneArcadeInputModule.GetByID(0).GetComponent<EventSystem>().SetSelectedGameObject(subscribeBtn.gameObject);
            }
            
            subscriptionStateUI.text = "TOURNAMENT IS OPEN FOR SUBSCRIPTION...<size=70%>\r\n" + numberOfOpenTeamsLeft + " SPOTS LEFT!";
        }
        else if (tournamentState == TournamentState.GroupPlay)
        {
            groupTournamentNameUI.text = "\"" + tournamentName + "\"";
            groupGameNameUI.text = "GAME: " + gameName;
            activePanel.gameObject.SetActive(true);
            player1Selection.Populate(teams, this);
            player1Selection.SelectFirst();
            player2Selection.Populate(teams, this);
            player2Selection.SelectFirst();
            StandaloneArcadeInputModule.GetByID(0).modalObject = null;
        }
        else if (tournamentState == TournamentState.Knockout)
        {
            knockoutPanel.gameObject.SetActive(true);
            GenerateKnockoutStructure();
            SelectNextKO();
        }

    }

    public void SubscribePlayer()
    {
        if (JoystickKeyboard.Main != null)
        {
            //Opens the keyboard, and also defines the function that will
            //be called, when "Ok" is being pushed on the keyboard. In this example it is a function called ChangeNameNow()
            JoystickKeyboard.Main.minTextLength = 3;
            
            JoystickKeyboard.Main.Activate(() => SavePlayerName());
        }
    }

    public void RecreatePin ()
    {

    }

    public void SavePlayerName()
    {
        if (JoystickKeyboard.Main.GetCurrentText() != "" && JoystickKeyboard.Main.GetCurrentText().Length <= 16)
        {
            int playerIndex = -1;
            TournamentTeam lastSubscribedTeam = teams.FindLast(c => c.teamStatus == TeamStatus.Closed);

            if (lastSubscribedTeam != null)
            {
                playerIndex = teams.IndexOf(lastSubscribedTeam);
            }
            playerIndex++;
            string playerName = JoystickKeyboard.Main.GetCurrentText();
            if (requirePin)
            {
                if (JoystickKeyboard.GetByID(2) != null)
                {
                    JoystickKeyboard.GetByID(2).automaticAcceptWhenValid = false;
                    JoystickKeyboard.GetByID(2).Title = "CREATE YOUR 4 DIGIT PIN\r\n<size=80%>USE JOYSTICK RIGHT WHEN FINISHED!";
                    JoystickKeyboard.GetByID(2).cancelAction = null;
                    JoystickKeyboard.GetByID(2).registerInputsFromPlayer = 1;
                    JoystickKeyboard.GetByID(2).HelpText = "USE BUTTONS TO INPUT DIGITS\r\nJOYSTICK LEFT TO DELETE\r\nJOYSTICK RIGHT TO ACCEPT";
                    //Opens the keyboard, and also defines the function that will
                    //be called, when "Ok" is being pushed on the keyboard. In this example it is a function called ChangeNameNow()
                    JoystickKeyboard.GetByID(2).Activate(() => FinalizeSubscribePlayer(playerIndex, playerName, int.Parse(JoystickKeyboard.GetByID(2).GetCurrentText()), true));
                }
            }
            else
            {
                FinalizeSubscribePlayer(playerIndex, playerName, -1, false);
            }

            
        }
    }

    public void FinalizeSubscribePlayer (int index, string name, int pin, bool verify)
    {
        if (verify)
        {
            if (JoystickKeyboard.GetByID(2) != null)
            {
                string dots = "";
                for (int i = 0; i < pin.ToString().Length - 2; i++)
                {
                    dots += ".";
                }

                JoystickKeyboard.GetByID(2).Title = "VERIFY YOUR 4 DIGIT PIN";
                JoystickKeyboard.GetByID(2).cancelAction = null;
                JoystickKeyboard.GetByID(2).registerInputsFromPlayer = 1;
                JoystickKeyboard.GetByID(2).validation = pin.ToString();
                JoystickKeyboard.GetByID(2).automaticAcceptWhenValid = true;
                //JoystickKeyboard.GetByID(2).validationFailedString = "VERIFICATION FAILED!\r\nHINT: " + pin.ToString().Substring(0, 1) + dots + pin.ToString().Substring(pin.ToString().Length - 1, 1);
                JoystickKeyboard.GetByID(2).HelpText = "USE BUTTONS TO INPUT DIGITS\r\nJOYSTICK LEFT TO DELETE\r\n<color=#FF0000><b>JOYSTICK DOWN TO CREATE NEW";
                //Opens the keyboard, and also defines the function that will
                JoystickKeyboard.GetByID(2).cancelAction = () => SavePlayerName();
                //be called, when "Ok" is being pushed on the keyboard. In this example it is a function called ChangeNameNow()
                JoystickKeyboard.GetByID(2).Activate(() => FinalizeSubscribePlayer(index, name, int.Parse(JoystickKeyboard.GetByID(2).GetCurrentText()), false));
                return;
            }
        }
        JoystickKeyboard.GetByID(2).cancelAction = null;
        teams[index].name = name;
        teams[index].pin = pin;
        teams[index].teamStatus = TeamStatus.Closed;
        SaveTournament();
        UpdateSystemUIStatus();
    }

    public void UpdateActiveGroup (PlayerSelect playerSelect, TournamentBattle bestBattle)
    {
        List<PlayerSelect> allSelected = new List<PlayerSelect>();
        //SelectPlayers.instances.Sort((x, y))
        for (int i = 0; i < SelectPlayers.instances.Count; i++)
        {
            if (SelectPlayers.instances[i].CurrentlySelected != null)
            {
                allSelected.Add(SelectPlayers.instances[i].CurrentlySelected);
            }
        }

        activeGroupUI.SetData(playerSelect.player.Group, allSelected, bestBattle);
    }

    public void UpdateActiveGroup(TournamentTeam player, TournamentBattle battle)
    {
        List<PlayerSelect> allSelected = new List<PlayerSelect>();
        //SelectPlayers.instances.Sort((x, y))
        for (int i = 0; i < SelectPlayers.instances.Count; i++)
        {
            if (SelectPlayers.instances[i].CurrentlySelected != null)
            {
                allSelected.Add(SelectPlayers.instances[i].CurrentlySelected);
            }
        }

        activeGroupUI.SetData(player.Group, allSelected, battle);
    }

    public bool SaveTournament ()
    {
        //name: XYZ; gameIdentifier: Comp.GameName4; path: D *\Dropbox\UnityProjects\SharedData\Comp.GameName4\Tournaments\Tournament_XYZ.txt; status: ReadyForSubscription;
        
        UpdateTournamentState();

        status = TournamentStatus.Undefined;

        if (tournamentState == TournamentState.Subscription)
        {
            status = TournamentStatus.ReadyForSubscription;
        }
        else if (tournamentState == TournamentState.Finished)
        {
            status = TournamentStatus.Finalized;
        }
        else if (tournamentState == TournamentState.GroupPlay || tournamentState == TournamentState.Knockout)
        {
            status = TournamentStatus.Running;
        }
        else
        {
            Debug.Log("State is Undefined");
            return false;
        }
        List<string> fileDatas = new List<string>();
        fileDatas.Add("name:" + tournamentName + ";gameName:" + gameName + ";gameIdentifier:" + gameIdentifier + ";path:" + path.Replace(":", "*") + ";status:" + status + ";tournamentType:" + tournamentType + ";scoreSorting:"+scoreSorting+";scoreFormatting:" + scoreFormatting + ";requirePin:" + requirePin + ";creation:" + ArcadeGlobals.DateTimeToString(creation) + ";activation:" + ArcadeGlobals.DateTimeToString(activation) + ";finalization:" + ArcadeGlobals.DateTimeToString(finalization) + ";");
        fileDatas.Add("teams:" + teams.Count + ";");
        for (int i = 0; i < teams.Count; i++)
        {
            fileDatas.Add(teams[i].GetSave());
        }

        fileDatas.Add("battles:" + battles.Count + ";");
        for (int i = 0; i < battles.Count; i++)
        {
            fileDatas.Add(battles[i].GetSave());
        }

        if (groups.Count > 0)
        {
            //groups:1;winners:1;teams:48;battles:1128;
            fileDatas.Add("groups:" + groups.Count + ";winners:" + groupWinners + ";teams:" + teamsPerGroup + ";battles:" + battlesPerGroup + ";");

            for (int i = 0; i < groups.Count; i++)
            {
                fileDatas.AddRange(groups[i].GetSave());
            }
        }

        if (quals != -1)
        {
            //knockout:4;initBattles:8;
            fileDatas.Add("knockout:" + quals + ";initBattles:" + (qualTeams / 2) + ";");
            for (int i = 0; i < koBattles.Count; i++)
            {
                fileDatas.Add("battleID:" + koBattles[i] + ";");
            }
        }

        if (winners.Count > 0)
        {
            fileDatas.Add("winners:" + winners.Count + ";");
        }
        for (int i = 0; i < winners.Count; i++)
        {
            fileDatas.Add(winners[i].GetSave());
        }

        ArcadeGlobals.WriteLinesToFile(fileDatas.ToArray(), path);

        if (callback != null)
        {
            callback.Invoke();
        }

        return true;
    }

    void SortGroupTeams (List<TournamentTeam> teams, ScoreSorting sorting)
    {
        if (sorting != ScoreSorting.Undefined)
        {
            if (sorting == ScoreSorting.HighIsGood)
            {
                teams.Sort((x, y) =>
                {
                    int result = y.points.CompareTo(x.points);
                    return result != 0 ? result : y.score.CompareTo(x.score);
                });
            }
            else
            {
                teams.Sort((x, y) =>
                {
                    int result = y.points.CompareTo(x.points);
                    return result != 0 ? result : x.score.CompareTo(y.score);
                });
            }
        }
    }
    public bool LoadTournament (string path)
    {
        List<string> tournamentLines = ArcadeGlobals.ReadLinesFromFile(path);
        int currentLine = 0;
        if (!LoadBasics(tournamentLines, ref currentLine, 1))
        {
            Debug.LogError("Error loading basics");
            return false;
        }

        //Teams
        int teamCount = -1;
        Dictionary<string, string> teamsBasics = ArcadeGlobals.GetPropertiesFromLine(tournamentLines[currentLine]);
        {
            if (teamsBasics.TryGetValue("teams", out string value))
            {
                if (int.TryParse(value, out int number))
                {
                    teamCount = number;
                }
            }
        }
        currentLine++;
        if (teamCount < 0)
        {
            Debug.LogError("Error loading teams");
            return false;
        }

        if (!LoadTeams(tournamentLines, teams, ref currentLine, teamCount))
        {
            Debug.LogError("Error loading teams");
            return false;
        }


        int battleCount = -1;
        Dictionary<string, string> battles = ArcadeGlobals.GetPropertiesFromLine(tournamentLines[currentLine]);
        {
            if (battles.TryGetValue("battles", out string value))
            {
                if (int.TryParse(value, out int number))
                {
                    battleCount = number;
                }
            }
        }
        currentLine++;
        if (battleCount < 0)
        {
            Debug.LogError("Error loading battles");
            return false;
        }


        if (!LoadBattles(tournamentLines, ref currentLine, battleCount))
        {
            Debug.LogError("Error loading battles");
            return false;
        }
        else
        {
            for (int i = 0; i < this.teams.Count; i++)
            {
                List<TournamentBattle> teamBattles = this.battles.FindAll(c => c.teamAID == this.teams[i].teamID || c.teamBID == this.teams[i].teamID);
                this.teams[i].Battles.AddRange(teamBattles);

                for (int j = 0; j < teamBattles.Count; j++)
                {
                    if (teamBattles[j].teamAID == this.teams[i].teamID)
                    {
                        teamBattles[j].TeamA = this.teams[i];
                    }
                    else
                    {
                        teamBattles[j].TeamB = this.teams[i];
                    }

                    if (teamBattles[j].winnerID == this.teams[i].teamID)
                    {
                        teamBattles[j].Winner = this.teams[i];
                    }
                }
            
            }


        }

        int groupCount = -1;

        Dictionary<string, string> groups = ArcadeGlobals.GetPropertiesFromLine(tournamentLines[currentLine]);
        {
            //groups:16;winners:1;teams:3;battles:3;
            if (groups.TryGetValue("groups", out string value))
            {
                if (int.TryParse(value, out int number))
                {
                    groupCount = number;
                }
            }
        }

        if (groupCount >= 0)
        {
            int teamSavedCount = -1;
            int battleSavedCount = -1;
            Dictionary<string, string> winnersData = ArcadeGlobals.GetPropertiesFromLine(tournamentLines[currentLine]);
            {
                //groups:16;winners:1;teams:3;battles:3;
                if (winnersData.TryGetValue("winners", out string value))
                {
                    if (int.TryParse(value, out int number))
                    {
                        groupWinners = number;
                    }
                }
            }
            Dictionary<string, string> teamsData = ArcadeGlobals.GetPropertiesFromLine(tournamentLines[currentLine]);
            {
                //groups:16;winners:1;teams:3;battles:3;
                if (teamsData.TryGetValue("teams", out string value))
                {
                    if (int.TryParse(value, out int number))
                    {
                        teamSavedCount = number;
                        teamsPerGroup = number;
                    }
                }
            }
            Dictionary<string, string> battlesData = ArcadeGlobals.GetPropertiesFromLine(tournamentLines[currentLine]);
            {
                //groups:16;winners:1;teams:3;battles:3;
                if (battlesData.TryGetValue("battles", out string value))
                {
                    if (int.TryParse(value, out int number))
                    {
                        battleSavedCount = number;
                        battlesPerGroup = number;
                    }
                }
            }

            currentLine++;
            LoadGroups(tournamentLines, ref currentLine, groupCount, teamSavedCount, battleSavedCount);


            for (int i = 0; i < this.groups.Count; i++)
            {
                for (int j = 0; j < this.groups[i].teamIDs.Count; j++)
                {
                    TournamentTeam team = this.teams.Find(c => c.teamID == this.groups[i].teamIDs[j]);
                    if (team != null)
                    {
                        team.Group = this.groups[i];
                        this.groups[i].Teams.Add(team);
                    }
                }

                SortGroupTeams(this.groups[i].Teams, scoreSorting);

                for (int j = 0; j < this.groups[i].battleIDs.Count; j++)
                {
                    TournamentBattle battle = this.battles.Find(c => c.battleID == this.groups[i].battleIDs[j]);
                    if (battle != null)
                    {
                        this.groups[i].Battles.Add(battle);
                    }
                }
            }

        }
        
        int qualCount = -1;
        if (currentLine < tournamentLines.Count)
        {
            Dictionary<string, string> koData = ArcadeGlobals.GetPropertiesFromLine(tournamentLines[currentLine]);
            {
                //groups:16;winners:1;teams:3;battles:3;
                if (koData.TryGetValue("knockout", out string value))
                {
                    if (int.TryParse(value, out int number))
                    {
                        qualCount = number;
                    }
                }
            }

            if (qualCount > 0)
            {
                quals = qualCount;
                if (groupCount >= 0)
                {
                    qualTeams = groupCount * groupWinners;
                }
                else
                {
                    qualTeams = teams.Count;
                }

                int qualBattles = qualTeams - 1;
                currentLine++;
                int max = currentLine + qualBattles;
                for (int i = currentLine; i < max; i++)
                {
                    Dictionary<string, string> koBattleDatas = ArcadeGlobals.GetPropertiesFromLine(tournamentLines[currentLine]);
                    {
                        //groups:16;winners:1;teams:3;battles:3;
                        if (koBattleDatas.TryGetValue("battleID", out string value))
                        {
                            if (int.TryParse(value, out int number))
                            {
                                koBattles.Add(number);
                            }
                        }
                    }
                    currentLine++;
                }
            }
        }
        currentLine++;
        if (currentLine < tournamentLines.Count)
        {
            Dictionary<string, string> winnersData = ArcadeGlobals.GetPropertiesFromLine(tournamentLines[currentLine]);
            {
                //groups:16;winners:1;teams:3;battles:3;
                if (winnersData.TryGetValue("winners", out string value))
                {
                    if (int.TryParse(value, out int number))
                    {
                        currentLine++;
                        LoadTeams(tournamentLines, winners, ref currentLine, number);
                    }
                }
            }
        }


        tournamentLoaded = true;
        UpdateTournamentState();
        return true;
    }

    bool LoadBasics (List<string> data, ref int currentLine, int lineCount)
    {
        int max = currentLine + lineCount;
        for (int i = currentLine; i < max; i++)
        {
            Dictionary<string, string> lineDatas = ArcadeGlobals.GetPropertiesFromLine(data[i]);
            {
                if (lineDatas.TryGetValue("name", out string value))
                {
                    tournamentName = value;
                }
                else
                {
                    return false;
                }
            }
            {
                if (lineDatas.TryGetValue("gameName", out string value))
                {
                    gameName = value;
                }
                else
                {
                    return false;
                }
            }
            {
                if (lineDatas.TryGetValue("gameIdentifier", out string value))
                {
                    gameIdentifier = value;
                }
                else
                {
                    return false;
                }
            }
            {
                if (lineDatas.TryGetValue("path", out string value))
                {
                    this.path = value.Replace("*", ":");
                }
                else
                {
                    return false;
                }
            }
            {
                if (lineDatas.TryGetValue("status", out string value))
                {
                    if (Enum.TryParse(value, out TournamentStatus statusResult))
                    {
                        status = statusResult;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            {
                if (lineDatas.TryGetValue("tournamentType", out string value))
                {
                    if (Enum.TryParse(value, out TournamentType typeResult))
                    {
                        tournamentType = typeResult;
                    }
                }
                else
                {
                    return false;
                }
            }
            {
                if (lineDatas.TryGetValue("scoreSorting", out string scoreSortingValue))
                {
                    if (Enum.TryParse(scoreSortingValue, out ScoreSorting scoreSortingResult))
                    {
                        scoreSorting = scoreSortingResult;
                    }
                }
            }
            {
                if (lineDatas.TryGetValue("scoreFormatting", out string scoreFormattingValue))
                {
                    scoreFormatting = scoreFormattingValue;
                }
            }
            {
                if (lineDatas.TryGetValue("requirePin", out string requirePinValue))
                {
                    if (bool.TryParse(requirePinValue.ToLower(), out bool requirePinResult))
                    {
                        requirePin = requirePinResult;
                    }
                    else if (bool.TryParse(requirePinValue, out bool requirePinResultUpper))
                    {
                        requirePin = requirePinResultUpper;
                    }
                }
            }
            {
                if (lineDatas.TryGetValue("creation", out string creationValue))
                {
                    creation = ArcadeGlobals.DateTimeFromString(creationValue);
                    
                }
            }
            {
                if (lineDatas.TryGetValue("activation", out string activationValue))
                {
                    activation = ArcadeGlobals.DateTimeFromString(activationValue);
                }
            }
            {
                if (lineDatas.TryGetValue("finalization", out string finalizationValue))
                {
                    finalization = ArcadeGlobals.DateTimeFromString(finalizationValue);
                }
            }
            currentLine++;
        }
        return true;
    }

    bool LoadTeams (List<string> datas, List<TournamentTeam> target, ref int currentLine, int lineCount)
    {
        int max = currentLine + lineCount;
        for (int i = currentLine; i < max; i++)
        {
            target.Add(TournamentTeam.CreateFromSave(datas[i]));
            currentLine++;
        }
        return true;
    }

    bool LoadBattles(List<string> datas, ref int currentLine, int lineCount)
    {
        int max = currentLine + lineCount;
        for (int i = currentLine; i < max; i++)
        {
            battles.Add(TournamentBattle.CreateFromSave(datas[i]));
            currentLine++;
        }
        return true;
    }

    bool LoadGroups (List<string> datas, ref int currentLine, int lineCount, int teamsCount, int battlesCount)
    {
        int max = currentLine + lineCount + (lineCount * (teamsCount + battlesCount));
        while (currentLine < max)
        {
            groups.Add(TournamentGroup.CreateFromSave(datas, ref currentLine, teamsCount, battlesCount));
            groups[groups.Count - 1].groupIndex = groups.Count - 1;
        }
        return true;
    }


}
