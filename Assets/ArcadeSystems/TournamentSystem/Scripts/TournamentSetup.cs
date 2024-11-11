using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.IO;
using System;
using System.Globalization;

public enum TeamStatus
{
    Open = 0,
    Closed = 1,
}
public enum BattleStatus
{
    Pending = 0,
    Won = 1,
    Draw = 2,
    Cancelled = 3,
}

public enum BattleType
{
    Group = 0,
    Knockout = 1,
}


[System.Serializable]
public class TournamentTeam
{
    //public TournamentSetup setup;
    public int teamID = -1;
    public string name;
    public int pin = -1;

    public int wins = 0;
    public int loses = 0;
    public int draws = 0;
    public int points = 0;
    public float score = 0;


    [NonSerialized]
    private RectTransform teamUI;
    public TeamStatus teamStatus;
    [NonSerialized]
    private List<TournamentBattle> battles = new List<TournamentBattle>();
    //public List<TournamentBattle> groupBattles = new List<TournamentBattle>();
    [NonSerialized]
    private TournamentGroup group = null;

    public RectTransform TeamUI
    {
        get
        {
            return teamUI;
        }
        set
        {
            teamUI = value;
        }
    }

    public List<TournamentBattle> Battles
    {
        get
        {
            return battles;
        }
        set
        {
            battles = value;
        }
    }

    public TournamentGroup Group
    {
        get
        {
            return group;
        }
        set
        {
            group = value;
        }
    }

    public string GetSave ()
    {
        return "teamID:" + teamID + ";name:" + name + ";pin:" + pin + ";wins:" + wins + ";loses:" + loses + ";draws:" + draws + ";points:" + points + ";score:" + score + ";teamStatus:" + teamStatus + ";";
    }

    public static TournamentTeam CreateFromSave (string data)
    {
        TournamentTeam result = new TournamentTeam();
        Dictionary<string, string> lineDatas = ArcadeGlobals.GetPropertiesFromLine(data);
        {
            if (lineDatas.TryGetValue("teamID", out string value))
            {
                if (int.TryParse(value, out int number))
                {
                    result.teamID = number;
                }
            }
        }
        {
            if (lineDatas.TryGetValue("name", out string value))
            {
                result.name = value;
            }
        }
        {
            if (lineDatas.TryGetValue("pin", out string value))
            {
                if (int.TryParse(value, out int number))
                {
                    result.pin = number;
                }
            }
        }
        {
            if (lineDatas.TryGetValue("teamStatus", out string value))
            {
                if (Enum.TryParse(value, out TeamStatus statusResult))
                {
                    result.teamStatus = statusResult;
                }
            }
        }
        {
            if (lineDatas.TryGetValue("wins", out string value))
            {
                if (int.TryParse(value, out int number))
                {
                    result.wins = number;
                }
            }
        }
        {
            if (lineDatas.TryGetValue("loses", out string value))
            {
                if (int.TryParse(value, out int number))
                {
                    result.loses = number;
                }
            }
        }
        {
            if (lineDatas.TryGetValue("draws", out string value))
            {
                if (int.TryParse(value, out int number))
                {
                    result.draws = number;
                }
            }
        }
        {
            if (lineDatas.TryGetValue("points", out string value))
            {
                if (int.TryParse(value, out int number))
                {
                    result.points = number;
                }
            }
        }

        {
            if (lineDatas.TryGetValue("score", out string value))
            {
                if (float.TryParse(value, out float number))
                {
                    result.score = number;
                }
                else if (float.TryParse(value.Replace(",", "."), out float number2))
                {
                    result.score = number2;
                }
            }
        }
        return result;
    }
    public void Delete()
    {
        int index = teamUI.GetSiblingIndex();
        //Debug.Log("Index of Del: " + index + ", parentChildCount: " + teamUI.parent.childCount);
        Transform parent = teamUI.parent;
        EventSystem.current.SetSelectedGameObject(null);

        MonoBehaviour.Destroy(teamUI.gameObject);
        TournamentSetup.instance.teams.Remove(this);
        RectTransform newSelection = parent.GetChild(index+1) as RectTransform;

        if (newSelection.childCount == 1)
        {
            newSelection.GetChild(0).GetComponent<Button>().Select();
        } 
        else
        {
            newSelection.GetChild(1).GetComponent<Button>().Select();
        }
    }

    public void ChangeName ()
    {
        if (JoystickKeyboard.Main != null)
        {
            //Opens the keyboard, and also defines the function that will
            //be called, when "Ok" is being pushed on the keyboard. In this example it is a function called ChangeNameNow()
            JoystickKeyboard.Main.Activate(() => ChangeNameNow());
        }
    }

    public void ChangeNameNow ()
    {
        if (JoystickKeyboard.Main.GetCurrentText() != "")
        {
            if (TournamentSetup.instance.teams.Find(c => c.name == JoystickKeyboard.Main.GetCurrentText()) == null)
            {
                name = JoystickKeyboard.Main.GetCurrentText();
                teamUI.GetChild(0).GetComponentInChildren<TMP_Text>().text = name;
            }
            else
            {
                Debug.Log("Two Players can't share name!");
            }
            
        }
        
    }
}

[System.Serializable]
public class TournamentGroup
{
    public int groupID = -1;
    public int groupIndex = -1;
    public List<int> teamIDs = new List<int>();
    public List<int> battleIDs = new List<int>();
    [NonSerialized]
    private List<TournamentTeam> teams = new List<TournamentTeam>();
    [NonSerialized]
    private List<TournamentBattle> battles = new List<TournamentBattle>();

    public List<TournamentTeam> Teams
    {
        get
        {
            return teams;
        }
        set
        {
            teams = value;
        }
    }

    public List<TournamentBattle> Battles
    {
        get
        {
            return battles;
        }
        set
        {
            battles = value;
        }
    }

    public static TournamentGroup CreateFromSave(List<string> datas, ref int currentLine, int teamCount, int battleCount)
    {
        TournamentGroup result = new TournamentGroup();
        Dictionary<string, string> groupBasics = ArcadeGlobals.GetPropertiesFromLine(datas[currentLine]);
        {
            if (groupBasics.TryGetValue("groupID", out string value))
            {
                if (int.TryParse(value, out int number))
                {
                    result.groupID = number;
                }
            }
        }
        currentLine++;

        int max = currentLine + teamCount;

        for (int i = currentLine; i < max; i++)
        {
            Dictionary<string, string> teams = ArcadeGlobals.GetPropertiesFromLine(datas[currentLine]);
            {
                if (teams.TryGetValue("teamID", out string value))
                {
                    if (int.TryParse(value, out int number))
                    {
                        result.teamIDs.Add(number);
                    }
                }
            }
            currentLine++;
        }

        max = currentLine + battleCount;

        for (int i = currentLine; i < max; i++)
        {
            Dictionary<string, string> battles = ArcadeGlobals.GetPropertiesFromLine(datas[currentLine]);
            {
                if (battles.TryGetValue("battleID", out string value))
                {
                    if (int.TryParse(value, out int number))
                    {
                        result.battleIDs.Add(number);
                    }
                }
            }
            currentLine++;
        }
        return result;
    }


    public List<string> GetSave ()
    {
        List<string> result = new List<string>();

        result.Add("groupID:" + groupID + ";");
        for (int j = 0; j < teamIDs.Count; j++)
        {
            result.Add("teamID:" + teamIDs[j] + ";");



        }
        for (int j = 0; j < battleIDs.Count; j++)
        {
            result.Add("battleID:" + battleIDs[j] + ";");
        }

        return result;
    }

}

[System.Serializable]
public class TournamentBattle
{
    public int battleID = -1;
    public int teamAID = -1;
    public int teamBID = -1;
    public BattleStatus status;
    public BattleType battleType;
    public int winnerID = -1;

    public List<int> leadupBattles;
    public int nextBattle = -1;
    [NonSerialized]
    private TournamentTeam teamA = null;
    [NonSerialized]
    private TournamentTeam teamB = null;
    [NonSerialized]
    private TournamentTeam winner = null;

    public TournamentTeam TeamA
    {
        get
        {
            return teamA;
        }
        set
        {
            teamAID = value.teamID;
            teamA = value;

        }
    }
    public TournamentTeam TeamB
    {
        get
        {
            return teamB;
        }
        set
        {
            teamBID = value.teamID;
            teamB = value;
        }
    }
    public TournamentTeam Winner
    {
        get
        {
            return winner;
        }
        set
        {
            winnerID = value.teamID;
            winner = value;
        }
    }

    public void FillFromSave (string data)
    {
        Dictionary<string, string> lineDatas = ArcadeGlobals.GetPropertiesFromLine(data);
        {
            if (lineDatas.TryGetValue("battleID", out string value))
            {

                if (int.TryParse(value, out int number))
                {
                    battleID = number;
                }
            }
        }
        {
            if (lineDatas.TryGetValue("teamAID", out string value))
            {

                if (int.TryParse(value, out int number))
                {
                    teamAID = number;
                }
            }
        }
        {
            if (lineDatas.TryGetValue("teamBID", out string value))
            {

                if (int.TryParse(value, out int number))
                {
                    teamBID = number;
                }
            }
        }

        {
            if (lineDatas.TryGetValue("status", out string value))
            {
                if (Enum.TryParse(value, out BattleStatus statusResult))
                {
                    status = statusResult;
                }
            }
        }

        {
            if (lineDatas.TryGetValue("battleType", out string value))
            {
                if (Enum.TryParse(value, out BattleType statusResult))
                {
                    battleType = statusResult;
                }
            }
        }

        {
            if (lineDatas.TryGetValue("winnerID", out string value))
            {

                if (int.TryParse(value, out int number))
                {
                    winnerID = number;
                }
            }
        }

        {
            if (lineDatas.TryGetValue("nextBattle", out string value))
            {

                if (int.TryParse(value, out int number))
                {
                    nextBattle = number;
                }
            }
        }

        {
            if (lineDatas.TryGetValue("leadupA", out string value))
            {
                if (int.TryParse(value, out int number))
                {
                    if (number >= 0)
                    {
                        leadupBattles = new List<int>();
                        leadupBattles.Add(number);
                    }

                }
            }

        }
        {
            if (lineDatas.TryGetValue("leadupB", out string value))
            {
                if (int.TryParse(value, out int number))
                {
                    if (number >= 0)
                    {
                        leadupBattles.Add(number);
                    }

                }
            }
        }
    }

    public static TournamentBattle CreateFromSave(string data)
    {
        TournamentBattle result = new TournamentBattle();
        result.FillFromSave(data);
        return result;
    }
    public string GetSave ()
    {
        int leadupA = -1;
        int leadupB = -1;
        if (leadupBattles != null && leadupBattles.Count > 0)
        {
            leadupA = leadupBattles[0];
            if (leadupBattles.Count > 1)
            {
                leadupB = leadupBattles[1];
            }
        }
        return "battleID:" + battleID + ";teamAID:" + teamAID + ";teamBID:" + teamBID + ";status:" + status + ";battleType:" + battleType + ";winnerID:" + winnerID + ";leadupA:" + leadupA + ";leadupB:" + leadupB + ";nextBattle:" + nextBattle + ";";
    }
}
public class GameData
{
    public string name;
    public string gameIdentifier;

    public GameData (string name, string gameIdentifier)
    {
        this.name = name;
        this.gameIdentifier = gameIdentifier;
    }
}
public class TournamentSetup : MonoBehaviour
{
    public static TournamentSetup instance;
    public static string lastCreatedIdentifier = "";
    public static string lastCreatedName = "";

    public Button firstButtonSelected;
    public List<TournamentTeam> teams;
    public GridLayoutGroup teamsContainer;
    public GridLayoutGroup typesContainer;
    public RectTransform teamNameTemplate;
    public TournamentTypeUI typeTemplate;
    public TMP_Text playerCountUI;
    public TMP_Text typeChosenUI;
    public Tournament tournamentData;
    public KnockoutStructure structureTemplate;
    public RectTransform seStructureVisualsContainer;
    public RectTransform seStructureParent;
    public RectTransform srrStructureVisualsContainer;
    public RectTransform createPanel;
    public Button createBtn;
    public string tournamentName = "";
    public Button srrTab;
    public Button seTab;
    public TMP_Text statsUI;
    public TMP_Text optimalTipUI;
    public TMP_Text tournamentNameUI;
    public RectTransform choiseContainer;
    public ChoiseUI choiseUITemplate;
    public TMP_Dropdown gameDropdownUI;
    public GroupUI groupUITemplate;
    public GridLayoutGroup groupsContainer;

    public Color typeUnlockedColor;
    public Color typeLockedColor;
    public TournamentTypeData tournamentTypeChosen = null;
    public Canvas ui;
    public List<TestSort> testSort;
    List<TournamentTypeUI> typesUIs = new List<TournamentTypeUI>();
    List<ChoiseUI> choiseUIs = new List<ChoiseUI>();
    List<GroupUI> groupUIs = new List<GroupUI>();

    int tournamentSeed;
    int lastTeamCount = 0;
    
    KnockoutStructure knockoutStructure;
    float seTabOriginalPosX;
    int srrMatchesPerPlayer = 1;
    bool isActive;
    bool isReady;
    GameData chosenGameData = null;
    List<GameData> gameDatas = null;
    System.Random randomGenerator = null;
    Action onClose;
    int ids = 1000000;
    bool finalizeDeactivation;
    
    

    private void Awake()
    {
        instance = this;
        ui.gameObject.SetActive(false);
        for (int i = 0; i < 20; i++)
        {
            testSort.Add(new TestSort(UnityEngine.Random.Range(0, 4), UnityEngine.Random.Range(0f, 200f)));
        }

        testSort.Sort((x, y) =>
        {
            int result = x.number.CompareTo(y.number);
            return result != 0 ? result : x.otherNumber.CompareTo(y.otherNumber);
        });

        testSort.Sort((x, y) =>
        {
            int result = x.number.CompareTo(y.number);
            return result != 0 ? result : y.otherNumber.CompareTo(x.otherNumber);
        });
    }

    public void AddMultiple ()
    {
        if (JoystickKeyboard.Secondary != null)
        {
            //Opens the keyboard, and also defines the function that will
            //be called, when "Ok" is being pushed on the keyboard. In this example it is a function called ChangeNameNow()
            JoystickKeyboard.Secondary.minTextLength = 0;
            JoystickKeyboard.Secondary.Activate(() => DoAddMultiple());
        }
    }

    public void SetSRRMatches(TMP_Dropdown dropdown)
    {
        if (int.TryParse(dropdown.options[dropdown.value].text, out int result))
        {
            srrMatchesPerPlayer = result;
            UpdateStats();
        }
           

    }

    public void DoAddMultiple()
    {
        if (int.TryParse(JoystickKeyboard.Secondary.GetCurrentText(), out int result)) 
        {
            if (result + teams.Count < 200)
            {
                for (int i = 0; i < result; i++)
                {
                    AddTeam(true);
                }

                
                UpdateShiftingColor();
                lastTeamCount = teams.Count;
                tournamentData.Calculate(teams.Count);
                UpdateTypesUI();
            }
            else
            {
                Debug.LogError("Too many players!");
            }
        }
    }

    int NextID()
    {
        ids++;
        return ids;
    }
    public List<TournamentGroup> CreateSRRGroupsAndBattles (ref List<TournamentBattle> battles)
    {

        List<TournamentGroup> groups = new List<TournamentGroup>();
        
        List<int> rdmIndices = RandomTeamIndices();
        int currentTeam = 0;
        int teamsPerGroup = teams.Count / tournamentTypeChosen.chosenStructure.numberOfInitGroups;

        for (int g = 0; g < tournamentTypeChosen.chosenStructure.numberOfInitGroups; g++)
        {
            TournamentGroup newGroup = new TournamentGroup();
            groups.Add(newGroup);
            newGroup.groupID = NextID();
            for (int i = 0; i < teamsPerGroup; i++)
            {

                newGroup.teamIDs.Add(teams[rdmIndices[currentTeam]].teamID);
                
                currentTeam++;
            }

            for (int m = 0; m < srrMatchesPerPlayer; m++)
            {
                for (int i = 0; i < newGroup.teamIDs.Count - 1; i++)
                {
                    for (int j = i + 1; j < newGroup.teamIDs.Count; j++)
                    {
                        TournamentBattle newBattle = new TournamentBattle();
                        newBattle.battleID = NextID();
                        newBattle.status = BattleStatus.Pending;
                        newBattle.battleType = BattleType.Group;
                        newBattle.teamAID = newGroup.teamIDs[i];
                        newBattle.teamBID = newGroup.teamIDs[j];
                        battles.Add(newBattle);
                        newGroup.battleIDs.Add(newBattle.battleID);
                    }
                }
            }


        }
        return groups;
    }

    public void CreateSEBattles (ref List<TournamentBattle> battles)
    {
        int numberOfTeamsFull = tournamentTypeChosen.chosenStructure.teamsAmount - tournamentTypeChosen.chosenStructure.numberRemovalsBeforeQuals;
        int numberOfTeams = numberOfTeamsFull;

        List<int> rdmIndices = null;
        int currentTeam = 0;
        if (numberOfTeams == tournamentTypeChosen.chosenStructure.teamsAmount)
        {
            rdmIndices = RandomTeamIndices();
        }

        while (numberOfTeams >= 2)
        {
            for (int i = 0; i < numberOfTeams; i += 2)
            {
                TournamentBattle newBattle = new TournamentBattle();
                newBattle.battleID = NextID();
                newBattle.status = BattleStatus.Pending;
                newBattle.battleType = BattleType.Knockout;
                if (numberOfTeams == tournamentTypeChosen.chosenStructure.teamsAmount && numberOfTeams == teams.Count)
                {
                    newBattle.teamAID = teams[rdmIndices[currentTeam]].teamID;
                    currentTeam++;
                    newBattle.teamBID = teams[rdmIndices[currentTeam]].teamID;
                    currentTeam++;
                }

                battles.Add(newBattle);

                

            }

            if (numberOfTeams < numberOfTeamsFull)
            {
                int offset = (numberOfTeams / 2) * 3;
                
                int max = battles.Count-(numberOfTeams / 2);
                int goal = max;

                //Debug.Log("NofT: " + numberOfTeams + ", offset: " + offset + ", max: " + max + ", count: " + battles.Count);

                for (int i = battles.Count - (offset); i < max; i+=2)
                {
                    battles[i].nextBattle = battles[goal].battleID;
                    battles[i+1].nextBattle = battles[goal].battleID;

                    battles[goal].leadupBattles = new List<int>();
                    battles[goal].leadupBattles.Add(battles[i].battleID);
                    battles[goal].leadupBattles.Add(battles[i+1].battleID);

                    goal++;
                }


            }

            numberOfTeams /= 2;
        }
        




    }

    List<int> RandomTeamIndices ()
    {
        List<int> result = new List<int>();

        List<int> indices = new List<int>();
        for (int i = 0; i < teams.Count; i++)
        {
            indices.Add(i);
        }

        while (indices.Count > 0)
        {
            int rdm = randomGenerator.Next(0, indices.Count);
            result.Add(indices[rdm]);
            indices.RemoveAt(rdm);
        }

        return result;
    }

    public void FindOptimalAmount (out int remove, out int add, out int battleCountRemove, out int battleCountAdd)
    {
        remove = 0;
        add = 0;
        battleCountRemove = 0;
        battleCountAdd = 0;
        if (tournamentTypeChosen != null)
        {
            int scope = Mathf.Max(5, Mathf.RoundToInt((float)teams.Count * .2f));

            int min = Mathf.Max(0, teams.Count - scope);
            int max = Mathf.Min(200, teams.Count + scope);
            List<TournamentStructure> validStructures = tournamentTypeChosen.sensibleStructures.FindAll(c => c.teamsAmount >= min && c.teamsAmount < max && c.teamsAmount != teams.Count && c.numberOfInitGroups > 0);

            int currentBattles = Tournament.CalculateSEBattles(tournamentTypeChosen.chosenStructure) + Tournament.CalculateSRRBattles(tournamentTypeChosen.chosenStructure, srrMatchesPerPlayer);

            validStructures.Sort((x, y) => (Tournament.CalculateSRRBattles(x, srrMatchesPerPlayer) + Tournament.CalculateSEBattles(x)).CompareTo(Tournament.CalculateSRRBattles(y, srrMatchesPerPlayer) + Tournament.CalculateSEBattles(y)));

            
            for (int i = 0; i < validStructures.Count; i++)
            {
                int altBattles = Tournament.CalculateSRRBattles(validStructures[i], srrMatchesPerPlayer) + Tournament.CalculateSEBattles(validStructures[i]);
                if (altBattles < currentBattles)
                {
                    int change = validStructures[i].teamsAmount - teams.Count;
                    if (change < 0 && remove == 0)
                    {
                        remove = Mathf.Abs(change);
                        battleCountRemove = altBattles;
                    }
                    else if (change > 0 && add == 0)
                    {
                        add = change;
                        battleCountAdd = altBattles;
                    }

                    if (remove > 0 && add > 0)
                    {
                        return;
                    }
                }
            }
        }
    }


    public void Activate (List<GameData> gameDatas, Action onClose)
    {
        if (JoystickKeyboard.Main == null)
        {
            Debug.LogError("A Joystick keyboard needs to be present in scene");
            return;
        }

        if (isReady)
        {
            return;
        }

        ui.gameObject.SetActive(true);
        isActive = true;

        if (!isReady)
        {
            PreSetup();
            isReady = true;
        }

        EventSystem.current.SetSelectedGameObject(firstButtonSelected.gameObject);
        CreateGameDropdownData(gameDatas);
        this.gameDatas = gameDatas;

        ArcadeInput.SetModal(new ModalData(this, false, new StandaloneArcadeInputModule[] { StandaloneArcadeInputModule.GetByID(0) }));
        this.onClose = onClose;
    }

    public void Deactivate()
    {
        ui.gameObject.SetActive(false);
        isActive = false;
        isReady = false;
        tournamentData.Clear();

        for (int i = 0; i < teams.Count; i++)
        {
            Destroy(teams[i].TeamUI.gameObject);
        }
        teams.Clear();

        for (int i = 0; i < typesUIs.Count; i++)
        {
            Destroy(typesUIs[i].gameObject);
        }
        typesUIs.Clear();

        for (int i = 0; i < choiseUIs.Count; i++)
        {
            Destroy(choiseUIs[i].gameObject);
        }
        choiseUIs.Clear();

        for (int i = 0; i < groupUIs.Count; i++)
        {
            Destroy(groupUIs[i].gameObject);
        }
        groupUIs.Clear();

        lastTeamCount = 0;

        if (knockoutStructure != null)
        {
            knockoutStructure.Clear();
        }
        knockoutStructure = null;
        
        srrMatchesPerPlayer = 1;
        
        chosenGameData = null;
        gameDatas = null;
        randomGenerator = null;

        ids = 1000000;
        tournamentTypeChosen = null;
        srrTab.gameObject.SetActive(false);
        seTab.gameObject.SetActive(false);
        createPanel.gameObject.SetActive(false);
        srrStructureVisualsContainer.gameObject.SetActive(false);
        seStructureVisualsContainer.gameObject.SetActive(false);
        tournamentName = "";
        tournamentNameUI.text = "TOURNAMENT NAME...";
        ArcadeInput.EndModal(this);
        if (onClose != null)
        {
            onClose.Invoke();
        }
        //if (StandaloneArcadeInputModule.instance.modalObject)
        //StandaloneArcadeInputModule.instance.modalObject = null;
    }

    public bool IsActive
    {
        get
        {
            return isActive;
        }
    }

    void CreateGameDropdownData (List<GameData> gameDatas)
    {
        List<TMP_Dropdown.OptionData> optionDatas = new List<TMP_Dropdown.OptionData>();
        for (int i = 0; i < gameDatas.Count; i++)
        {
            optionDatas.Add(new TMP_Dropdown.OptionData(gameDatas[i].name));
        }
        
        gameDropdownUI.options = optionDatas;
        gameDropdownUI.value = 0;
    }
    // Start is called before the first frame update
    void PreSetup()
    {
        int[] tournamentTypes = (int[])System.Enum.GetValues(typeof(TournamentType));
        seTabOriginalPosX = seTab.transform.position.x;
        for (int i = 0; i < tournamentTypes.Length; i++)
        {
            TournamentType current = (TournamentType)tournamentTypes[i];
            TournamentTypeUI newTypeUI = Instantiate(typeTemplate.gameObject).GetComponent<TournamentTypeUI>();
            newTypeUI.transform.SetParent(typesContainer.transform);
            typesUIs.Add(newTypeUI);
            newTypeUI.typeUI.text = SplitCamelCase(((TournamentType)tournamentTypes[i]).ToString()).ToUpper();
            
            newTypeUI.typeUI.transform.parent.GetComponent<Button>().navigation = Navigation.defaultNavigation;
            newTypeUI.infoUI.text = "INFO: " + tournamentData.infos[i];
            Button selectBtn = newTypeUI.typeUI.transform.parent.GetComponent<Button>();
            selectBtn.onClick.AddListener(() => { ChooseType(selectBtn); });
            selectBtn.interactable = false;
            newTypeUI.GetComponent<Image>().color = typeLockedColor;
        }

        tournamentData.Calculate(teams.Count);
        UpdateTypesUI();
    }

    // Update is called once per frame
    void Update()
    {
        if (isActive)
        {
            if (lastTeamCount != teams.Count)
            {
                UpdateShiftingColor();
                lastTeamCount = teams.Count;

                tournamentData.Calculate(teams.Count);
                UpdateTypesUI();
            }

            playerCountUI.text = "CREATE PLAYERS\r\nPLAYERS: " + teams.Count;
            /*
            if (JoystickKeyboard.Main.IsActive())
            {
                //This is the basic input setup for moving around the keyboard grid
                //To test for player 2 inputs, just change the first parameter to 2
                //To enable both players on the same time, use 0 as player input.
                if (ArcadeInput.InputInitiated(1, ArcadeInputType.JoystickDown, AxisType.Raw, JoystickKeyboard.Main))
                {
                    JoystickKeyboard.Main.MoveDown();
                }
                if (ArcadeInput.InputInitiated(1, ArcadeInputType.JoystickUp, AxisType.Raw, JoystickKeyboard.Main))
                {
                    JoystickKeyboard.Main.MoveUp();
                }
                if (ArcadeInput.InputInitiated(1, ArcadeInputType.JoystickLeft, AxisType.Raw, JoystickKeyboard.Main))
                {
                    JoystickKeyboard.Main.MoveLeft();
                }
                if (ArcadeInput.InputInitiated(1, ArcadeInputType.JoystickRight, AxisType.Raw, JoystickKeyboard.Main))
                {
                    JoystickKeyboard.Main.MoveRight();
                }
                if (ArcadeInput.AnyButtonInitiated(1, JoystickKeyboard.Main))
                {
                    JoystickKeyboard.Main.PushSelected();
                }
            }
            */
        }
    }

    public void AddTeam (bool awaitCalculate = false)
    {
        RectTransform newTeamUI = Instantiate(teamNameTemplate.gameObject).transform as RectTransform;
        TournamentTeam newTeam = new TournamentTeam();
        newTeam.name = "PLAYER";
        newTeam.TeamUI = newTeamUI;
        newTeam.teamID = NextID();
        //newTeam.setup = this;
        newTeamUI.GetChild(0).GetComponent<Button>().onClick.AddListener(newTeam.ChangeName);
        newTeamUI.GetChild(1).GetComponent<Button>().onClick.AddListener(newTeam.Delete);

        newTeamUI.GetChild(0).GetComponent<Button>().navigation = Navigation.defaultNavigation;
        newTeamUI.GetChild(1).GetComponent<Button>().navigation = Navigation.defaultNavigation;
        newTeamUI.GetChild(0).GetComponentInChildren<TMP_Text>().text = newTeam.name;

        newTeamUI.SetParent(teamsContainer.transform);
        
        



        teams.Add(newTeam);

        if (!awaitCalculate)
        {
            newTeamUI.SetSiblingIndex(teamsContainer.transform.childCount - 2);
            UpdateShiftingColor();
            lastTeamCount = teams.Count;
            tournamentData.Calculate(teams.Count);
            UpdateTypesUI();
        }
        else
        {
            newTeamUI.SetAsFirstSibling();
        }
        
    }

    void UpdateTypesUI ()
    {
        int[] tournamentTypes = (int[])System.Enum.GetValues(typeof(TournamentType));
        
        if (tournamentTypeChosen != null && !tournamentTypeChosen.unlocked)
        {
            tournamentTypeChosen = null;
            UpdateStats();
            GenerateSetupChoices();
            if (knockoutStructure != null)
            {
                Destroy(knockoutStructure.gameObject);
                knockoutStructure = null;
            }
            srrTab.gameObject.SetActive(false);
            seTab.gameObject.SetActive(false);
            createPanel.gameObject.SetActive(false);
            srrStructureVisualsContainer.gameObject.SetActive(false);
            seStructureVisualsContainer.gameObject.SetActive(false);
            
            
        }
        
        if (tournamentTypeChosen != null && tournamentTypeChosen.unlocked)
        {
            GenerateSetupChoices();
            srrTab.gameObject.SetActive(true);
            seTab.gameObject.SetActive(true);
            createPanel.gameObject.SetActive(true);
            if (tournamentTypeChosen.tournamentType != TournamentType.Knockout)
            {
                srrTab.interactable = true;

                if (tournamentTypeChosen.tournamentType == TournamentType.Combination)
                {
                    FindOptimalAmount(out int remove, out int add, out int battlesRemove, out int battlesAdd);
                    if (remove > 0 || add > 0)
                    {
                        optimalTipUI.text = "";
                        if (remove > 0)
                        {
                            optimalTipUI.text = "REMOVE " + remove + " PLAYERS FOR LESS BATTLES (" + battlesRemove + ")\r\n";
                        }

                        if (add > 0)
                        {
                            optimalTipUI.text += "ADD " + add + " PLAYERS FOR LESS BATTLES (" + battlesAdd + ")";
                        }
                    }
                    else
                    {
                        optimalTipUI.text = "";
                    }
                }
                else
                {
                    optimalTipUI.text = "";
                }
            } 
            else
            {
                srrTab.interactable = false;
            }

            if (tournamentTypeChosen.chosenStructure.numberOfQualifications > 0)
            {
                
                seTab.interactable = true;

                GenerateKnockoutStructure();
            }
            else
            {
                seTab.interactable = false;
                if (knockoutStructure != null)
                {
                    Destroy(knockoutStructure.gameObject);
                    knockoutStructure = null;
                }
            }
            UpdateStats();
        }

        /*
        if (seTab.gameObject.activeSelf && !srrTab.gameObject.activeSelf)
        {
            float xPos = srrTab.transform.position.x - ((srrTab.transform as RectTransform).rect.width * .5f) + (seTab.transform as RectTransform).rect.width * .5f;

            seTab.transform.position = new Vector3(xPos, seTab.transform.position.y, seTab.transform.position.z);
        }
        else if (seTab.gameObject.activeSelf && srrTab.gameObject.activeSelf)
        {
            seTab.transform.position = new Vector3(seTabOriginalPosX, seTab.transform.position.y, seTab.transform.position.z);
        }
        */

        for (int i = 0; i < tournamentTypes.Length; i++)
        {
            typesUIs[i].tournamentTypeData = tournamentData.tournamentTypeDatas[i];
            TournamentType current = (TournamentType)tournamentTypes[i];
            string message = "";
            int remAmount = 0;
            int addAmount = 0;


            if (tournamentData.tournamentTypeDatas[i].lastValid > -1 && tournamentData.tournamentTypeDatas[i].lastValid != teams.Count)
            {
                remAmount = (teams.Count - tournamentData.tournamentTypeDatas[i].lastValid);
                message = "REMOVE " + remAmount;
            }

            if (tournamentData.tournamentTypeDatas[i].nextValid > -1 && tournamentData.tournamentTypeDatas[i].nextValid != teams.Count && tournamentData.tournamentTypeDatas[i].lastValid != teams.Count)
            {
                addAmount = (tournamentData.tournamentTypeDatas[i].nextValid - teams.Count);
                if (message != "")
                {
                    message += " OR ADD " + addAmount;
                }
                else
                {
                    message += "ADD " + addAmount;
                }
            }

            if (message != "")
            {
                string sAdd = "";
                if (remAmount > 1 || addAmount > 1)
                {
                    sAdd = "S";
                }
                message += " PLAYER" + sAdd + " TO UNLOCK THIS TOURNAMENT TYPE";
            }

            Button selectBtn = typesUIs[i].typeUI.transform.parent.GetComponent<Button>();
            if (message == "")
            {
                selectBtn.interactable = true;
                typesUIs[i].lockedUnlocked.sprite = typesUIs[i].unlockedSprite;
                message = "UNLOCKED";
                
                if (tournamentTypeChosen == null || tournamentTypeChosen == typesUIs[i].tournamentTypeData)
                {
                    if (tournamentTypeChosen == null)
                    {
                        typesUIs[i].GetComponent<Image>().color = Color.gray;
                    }
                    else
                    {
                        typesUIs[i].GetComponent<Image>().color = typeUnlockedColor;
                    }
                }
                else
                {
                    typesUIs[i].GetComponent<Image>().color = Color.gray;
                }
                    
            }
            else
            {
                selectBtn.interactable = false;
                typesUIs[i].lockedUnlocked.sprite = typesUIs[i].lockedSprite;
                if (tournamentTypeChosen == null)
                {
                    //typesUIs[i].GetComponent<Image>().color = typeLockedColor;
                    typesUIs[i].GetComponent<Image>().color = Color.gray;
                }
                else
                {
                    typesUIs[i].GetComponent<Image>().color = Color.gray;
                }
                
            }
            
            
            typesUIs[i].messageUI.text = message;
        }

        if (tournamentTypeChosen != null && tournamentTypeChosen.unlocked)
        {
            typeChosenUI.text = "TYPE SELECTED:\r\n" + SplitCamelCase(tournamentTypeChosen.tournamentType.ToString()).ToUpper();
        }
        else
        {
            typeChosenUI.text = "CHOOSE TYPE...";
        }
    }

    
    public void CreateTournament ()
    {
        if (randomGenerator == null)
        {
            tournamentSeed = UnityEngine.Random.Range(0, 1000000);
            randomGenerator = new System.Random(tournamentSeed);

        }

        List<TournamentBattle> srrBattles = null;
        List<TournamentBattle> seBattles = null;
        List<TournamentGroup> groups = null;
        int battleCount = 0;
        if (tournamentTypeChosen.chosenStructure.numberOfInitGroups > 0)
        {
            srrBattles = new List<TournamentBattle>();
            groups = CreateSRRGroupsAndBattles(ref srrBattles);
            battleCount += srrBattles.Count;
            //Debug.Log("Number of srr: " + srrBattles.Count);
        }

        if (tournamentTypeChosen.chosenStructure.numberOfQualifications > 0)
        {
            seBattles = new List<TournamentBattle>();
            CreateSEBattles(ref seBattles);
            battleCount += seBattles.Count;
            //Debug.Log("Number of se: " + seBattles.Count);
        }


        GameData chosenData = gameDatas.Find(c => c.name == gameDropdownUI.options[gameDropdownUI.value].text);
        string path = ArcadeGlobals.SharedPath + "\\" + chosenData.gameIdentifier + "\\" + "Tournaments";
        //Debug.Log("Prepath: " + path);
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        path += "\\Tournament_" + tournamentName + ".txt";

        if (File.Exists(path))
        {
            Debug.Log("There already exists a tournament with that name!");
            return;
        }


        List<string> fileDatas = new List<string>();
        fileDatas.Add("name:" + tournamentName + ";gameName:" + chosenData.name + ";gameIdentifier:" + chosenData.gameIdentifier + ";path:" + path.Replace(":", "*") + ";status:" + TournamentStatus.ReadyForSubscription + ";tournamentType:" + tournamentTypeChosen.tournamentType + ";scoreSorting:Undefined;requirePin:True;creation:" + ArcadeGlobals.DateTimeToString(DateTime.Now) + ";activation:" + ArcadeGlobals.DateTimeToString(DateTime.MinValue) + ";finalization:" + ArcadeGlobals.DateTimeToString(DateTime.MinValue) + ";");
        
        fileDatas.Add("teams:" + teams.Count + ";");

        for (int i = 0; i < teams.Count; i++)
        {
            fileDatas.Add(teams[i].GetSave());
        }


        fileDatas.Add("battles:" + battleCount + ";");
        /*
        public int battleID = -1;
        public int teamAID = -1;
        public int teamBID = -1;
        public BattleStatus status;
        public int winner = -1;

        public List<int> leadupBattles;
        public int nextBattle;
        */
        if (srrBattles != null)
        {
            for (int i = 0; i < srrBattles.Count; i++)
            {
                fileDatas.Add(srrBattles[i].GetSave());
            }
        }

        if (seBattles != null)
        {
            int initBattles = ((tournamentTypeChosen.chosenStructure.teamsAmount - tournamentTypeChosen.chosenStructure.numberRemovalsBeforeQuals) / 2);

            for (int i = 0; i < seBattles.Count; i++)
            {
                fileDatas.Add(seBattles[i].GetSave());

            }
        }



        if (groups != null)
        {
            fileDatas.Add("groups:" + groups.Count + ";winners:" + tournamentTypeChosen.chosenStructure.numberOfWinnersPerGroup + ";teams:" + tournamentTypeChosen.chosenStructure.teamsPerGroup + ";battles:" + (Tournament.CalculateSRRBattles(tournamentTypeChosen.chosenStructure, srrMatchesPerPlayer)/ tournamentTypeChosen.chosenStructure.numberOfInitGroups) + ";");

            for (int i = 0; i < groups.Count; i++)
            {
                fileDatas.AddRange(groups[i].GetSave());
                /*
                public int groupID = -1;
                public int winners;
                public List<int> teamIDs = new List<int>();
                public List<int> battleIDs = new List<int>();
                */

            }


        }

        if (seBattles != null)
        {
            int initBattles = ((tournamentTypeChosen.chosenStructure.teamsAmount - tournamentTypeChosen.chosenStructure.numberRemovalsBeforeQuals) / 2);

            fileDatas.Add("knockout:" + tournamentTypeChosen.chosenStructure.numberOfQualifications + ";initBattles:" + initBattles + ";");

            for (int i = 0; i < seBattles.Count; i++)
            {
                fileDatas.Add("battleID:" + seBattles[i].battleID + ";");
            }
        }





        
        ArcadeGlobals.WriteLinesToFile(fileDatas.ToArray(), path);
        Debug.Log("Written to: " + path);

        TournamentSetup.lastCreatedIdentifier = chosenData.gameIdentifier;
        TournamentSetup.lastCreatedName = tournamentName;
        Deactivate();
        
    }

    public void ChangeTournamentName(TMP_Text textField)
    {
        if (JoystickKeyboard.Main != null)
        {
            //Opens the keyboard, and also defines the function that will
            //be called, when "Ok" is being pushed on the keyboard. In this example it is a function called ChangeNameNow()
            JoystickKeyboard.Main.Activate(() => ChangeTournamentNameNow(textField));
        }
    }

    public void ChangeTournamentNameNow(TMP_Text textField)
    {
        if (JoystickKeyboard.Main.GetCurrentText() != "" && !JoystickKeyboard.Main.GetCurrentText().Contains(";") && !JoystickKeyboard.Main.GetCurrentText().Contains(":"))
        {
            tournamentName = JoystickKeyboard.Main.GetCurrentText();
            textField.text = JoystickKeyboard.Main.GetCurrentText();
        }

    }


    void GenerateKnockoutStructure ()
    {
        KnockoutStructure newKnockoutStructure = null;
        
        if (knockoutStructure == null)
        {
            newKnockoutStructure = Instantiate(structureTemplate.gameObject, seStructureParent).GetComponent<KnockoutStructure>();
            newKnockoutStructure.transform.localPosition = Vector2.zero;
            Rect containerSize = seStructureParent.rect;
            newKnockoutStructure.knockoutArea.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, containerSize.width);
            newKnockoutStructure.knockoutArea.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, containerSize.height);
            knockoutStructure = newKnockoutStructure;
        }
        else
        {
            newKnockoutStructure = knockoutStructure;
        }

        if (tournamentTypeChosen.chosenStructure.teamsAmount - tournamentTypeChosen.chosenStructure.numberRemovalsBeforeQuals != newKnockoutStructure.players)
        {
            newKnockoutStructure.BuildKnockoutStructure(tournamentTypeChosen.chosenStructure.teamsAmount - tournamentTypeChosen.chosenStructure.numberRemovalsBeforeQuals);
            if (!srrTab.gameObject.activeSelf)
            {
                ShowSE();
            }
        }
    }

    void GenerateSetupChoices()
    {
        for (int i = 0; i < choiseUIs.Count; i++)
        {
            choiseUIs[i].transform.SetParent(null);
            Destroy(choiseUIs[i].gameObject);
        }
        choiseUIs.Clear();
        if (tournamentTypeChosen != null && tournamentTypeChosen.unlocked && tournamentTypeChosen.tournamentType != TournamentType.Knockout)
        {
            for (int i = 0; i < tournamentTypeChosen.unlockedStructures.Count; i++)
            {
                ChoiseUI newChoiseUI = Instantiate(choiseUITemplate.gameObject, choiseContainer).GetComponent<ChoiseUI>();
                choiseUIs.Add(newChoiseUI);

                newChoiseUI.groupCountUI.text = tournamentTypeChosen.unlockedStructures[i].numberOfInitGroups.ToString();
                newChoiseUI.groupWinnersUI.text = tournamentTypeChosen.unlockedStructures[i].numberOfWinnersPerGroup.ToString();

                newChoiseUI.structure = tournamentTypeChosen.unlockedStructures[i];
                Button choiseBtn = newChoiseUI.GetComponent<Button>();

                choiseBtn.onClick.AddListener(() => { ChooseSetup(newChoiseUI); });
                UpdateChoiseColors();
                GenerateGroupUIs();
            }
        }
    }

    void GenerateGroupUIs ()
    {
        for (int i = 0; i < groupUIs.Count; i++)
        {
            groupUIs[i].transform.SetParent(null);
            Destroy(groupUIs[i].gameObject);
        }
        groupUIs.Clear();

        if (tournamentTypeChosen != null && tournamentTypeChosen.chosenStructure != null && tournamentTypeChosen.chosenStructure.numberOfInitGroups > 0)
        {
            int playersPerGroup = tournamentTypeChosen.chosenStructure.teamsAmount / tournamentTypeChosen.chosenStructure.numberOfInitGroups;
            int winnersPerGroup = tournamentTypeChosen.chosenStructure.numberOfWinnersPerGroup;

            for (int i = 0; i < tournamentTypeChosen.chosenStructure.numberOfInitGroups; i++)
            {
                GroupUI newGroup = Instantiate(groupUITemplate.gameObject, groupsContainer.transform).GetComponent<GroupUI>();
                newGroup.Generate("GROUP " + (i+1), playersPerGroup, winnersPerGroup, groupsContainer);
                groupUIs.Add(newGroup);
            }


        }

    }

    
    public void ShowSRR()
    {
        srrStructureVisualsContainer.gameObject.SetActive(true);
        seStructureVisualsContainer.gameObject.SetActive(false);
        
    }

    public void ShowSE()
    {
        srrStructureVisualsContainer.gameObject.SetActive(false);
        seStructureVisualsContainer.gameObject.SetActive(true);
        
    }

   
    public void ChooseType (Button btn)
    {
        tournamentTypeChosen = btn.transform.parent.GetComponent<TournamentTypeUI>().tournamentTypeData;
        UpdateStats();
        if (tournamentTypeChosen.chosenStructure.numberOfInitGroups == 0)
        {
            srrStructureVisualsContainer.gameObject.SetActive(false);
        }
        if (tournamentTypeChosen.chosenStructure.numberOfQualifications == 0)
        {
            seStructureVisualsContainer.gameObject.SetActive(false);
        }

        UpdateTypesUI();

        if (tournamentTypeChosen.tournamentType == TournamentType.Combination)
        {
            ShowSRR();

            


        }
        else
        {
            optimalTipUI.text = "";

            if (tournamentTypeChosen.chosenStructure.numberOfInitGroups == 0 && tournamentTypeChosen.chosenStructure.numberOfQualifications > 0 && srrStructureVisualsContainer.gameObject.activeSelf)
            {
                ShowSE();
            }
            else if (tournamentTypeChosen.chosenStructure.numberOfInitGroups > 0 && tournamentTypeChosen.chosenStructure.numberOfQualifications == 0 && seStructureVisualsContainer.gameObject.activeSelf)
            {
                ShowSRR();
            }
            else if (tournamentTypeChosen.chosenStructure.numberOfInitGroups == 0 && tournamentTypeChosen.chosenStructure.numberOfQualifications > 0 && !seStructureVisualsContainer.gameObject.activeSelf)
            {
                ShowSE();
            }
            else if (tournamentTypeChosen.chosenStructure.numberOfInitGroups > 0 && tournamentTypeChosen.chosenStructure.numberOfQualifications == 0 && !srrStructureVisualsContainer.gameObject.activeSelf)
            {
                ShowSRR();
            }
        }
    }

    void UpdateStats ()
    {
        if (tournamentTypeChosen != null)
        {
            statsUI.text = "<size=65%>\r\n";// BATTLE COUNT<size=70%>\r\n";
            int allBattles = 0;
            if (tournamentTypeChosen.chosenStructure.numberOfInitGroups > 0)
            {
                int srrMatches = Tournament.CalculateSRRBattles(tournamentTypeChosen.chosenStructure, srrMatchesPerPlayer);
                allBattles += srrMatches;
                statsUI.text += "GROUP PLAY: " + srrMatches + "\r\n";
            }
            if (tournamentTypeChosen.chosenStructure.numberOfQualifications > 0)
            {
                int seMatches = Tournament.CalculateSEBattles(tournamentTypeChosen.chosenStructure);
                allBattles += seMatches;
                statsUI.text += "KNOCKOUT: " + seMatches;
            }
            statsUI.text = "BATTLE COUNT: " + allBattles + statsUI.text;
        }
        else
        {
            statsUI.text = "BATTLE COUNT";
        }
    }

    

    public void ChooseSetup (ChoiseUI choiseUI)
    {
        if (tournamentTypeChosen != null && tournamentTypeChosen.unlocked)
        {
            tournamentTypeChosen.chosenStructure = choiseUI.structure;
            UpdateStats();
            UpdateChoiseColors();
            GenerateKnockoutStructure();
            GenerateGroupUIs();
        }
    }

    void UpdateChoiseColors ()
    {
        if (tournamentTypeChosen != null && tournamentTypeChosen.chosenStructure != null)
        {
            for (int i = 0; i < choiseUIs.Count; i++)
            {
                Button choiseBtn = choiseUIs[i].GetComponent<Button>();
                if (tournamentTypeChosen.chosenStructure == choiseUIs[i].structure)
                {
                    ColorBlock choiseColors = choiseBtn.colors;
                    choiseColors.normalColor = typeUnlockedColor;

                    choiseBtn.colors = choiseColors;
                }
                else
                {
                    ColorBlock choiseColors = choiseBtn.colors;
                    choiseColors.normalColor = Color.white;

                    choiseBtn.colors = choiseColors;
                }
            }
        }
    }

    public void UpdateShiftingColor ()
    {
        for (int i = 0; i < teams.Count; i++)
        {
            if (i % 2 == 0)
            {
                teams[i].TeamUI.GetComponent<Image>().enabled = true;
            } 
            else
            {
                teams[i].TeamUI.GetComponent<Image>().enabled = false;
            }
        }
    }

    public string SplitCamelCase(string input)
    {
        return System.Text.RegularExpressions.Regex.Replace(input, "([A-Z])", " $1", System.Text.RegularExpressions.RegexOptions.Compiled).Trim();
    }

    


}
