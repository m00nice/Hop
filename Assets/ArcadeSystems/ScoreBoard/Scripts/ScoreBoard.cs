using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using System.Collections;
using System.Threading;
using UnityEngine.EventSystems;

//Enum used to set sorting of highscores from either low->high or high->low (in the inspector)
public enum SortingType
{
    LowIsWin = 0,
    HighIsWin = 1,
}

[System.Serializable] //Makes the class usable somehow in the inspector (difficult to understand)
public class ScoreData
{
    public string name;
    public float score;
    public string season;
    public bool newData = false;
    public ScoreData(string name, float score, string season)
    {
        this.name = name;
        this.score = score;
        this.season = season;
    }
}

public class ScoreBoard : MonoBehaviour
{
    public static ScoreBoard instance;
    public static List<ScoreBoard> showCases = new List<ScoreBoard>();
    public static List<ScoreBoard> visibleScoreBoards = new List<ScoreBoard>();
    public bool pureShowCase = false;

    public float showDuration = 8;
    public float hideDuration = 8;

    public TMP_FontAsset font;
    public float globalFontScale = 1;
    public Color globalFontColor = Color.white;
    public bool useGlobalFontColor;
    public string scoreBoardID = "R"; //ID for the reaction game
    public int entryCount = 10; //The amount of listings in the highscore table (and that it records)

    public SortingType sortingType; //reference to the above Enum
    public bool deletePrefs = false; //So that you can delete all the saved highscore data from the inspector

    public float cursorBlinkTime = 0.5f;

    public string scoreFormatter = "0.000";

    public RectTransform scoreBoardPanelUI;
    public GridLayoutGroup column1Container;
    public GridLayoutGroup column2Container;
    public HighscoreEntry entryTemplateUI;

    public TMP_Text scoreBoardTitleUI;
    public TMP_Text scoreBoardSeasonUI;
    //public TMP_Dropdown seasonOptionsUI;

    public string scoreBoardTitle = "SCORE BOARD";
    public string returnToStartScreenMethod = "PRESS SPACE TO RETURN TO START SCREEN";
    public string newEntryMethodText = "PRESS SPACE TO ENTER NAME";

    public bool testFullScoreBoard = false;
    public float minTestScore = 1000;
    public float maxTestScore = 10000;

    ScoreData allTimeHigh = null;
    List<ScoreData> scores = new List<ScoreData>();
    List<HighscoreEntry> highscoreEntries = new List<HighscoreEntry>();
    ScoreData newHighscoreData;
    string applicationIdentifier;
    bool isActive = false;
    bool scoreBoardSettingsInvalid = false;
    bool endShowCase = false;
    bool resetShowCase = false;

    public List<string> seasons = new List<string>();
    public string activeSeason = "";
    public string shownSeason = "";
    public Image seasonTabTemplate;
    public Sprite seasonOnSprite;
    public Sprite seasonOffSprite;
    
    public RectTransform seasonTabsContainer;
    public RectTransform seasonTabsText;
    public Action cancelNewScore;
    List<Image> seasonTabs = new List<Image>();
    string dataPath;

    EventWaitHandle waitHandle;
    Thread writeScoresThread;
    bool awaitWriting = false;
    bool showcaseStayActive = false;
    public static bool ShowCasesActive()
    {
        for (int i = 0; i < showCases.Count; i++)
        {
            if (showCases[i].IsActive())
            {
                return true;
            }
        }
        return false;
    }

    public static void EndShowCases()
    {
        for (int i = 0; i < showCases.Count; i++)
        {
            showCases[i].EndShowCase();
        }
    }

    public static void ResetShowCases()
    {
        for (int i = 0; i < showCases.Count; i++)
        {
            showCases[i].ResetShowCase();
        }
    }

    public static bool BlockOtherInputs()
    {
        if ((instance == null || !instance.IsActive()))
        {
            return false;
        }
        return true;
    }

    private void Update()
    {
        if (visibleScoreBoards.Count == 0 && ArcadeInput.AnyInputInitiated(0, AxisType.Raw, this))
        {
            ResetShowCases();
        }

        if (IsActive() && !ReadyForName())
        {
            if (ArcadeInput.InputInitiated(0, ArcadeInputType.JoystickRight, AxisType.Raw, this))
            {
                ChangeSeasonDown();
            }

            if (ArcadeInput.InputInitiated(0, ArcadeInputType.JoystickLeft, AxisType.Raw, this))
            {
                ChangeSeasonUp();
            }

            if (ArcadeInput.InputInitiated(0, ArcadeInputType.JoystickUp, AxisType.Raw, this))
            {
                ResetShowCases();
            }
            if (ArcadeInput.InputInitiated(0, ArcadeInputType.JoystickDown, AxisType.Raw, this))
            {
                ResetShowCases();
            }
        }

        if (ArcadeInput.AnyButtonInitiated(0, this))
        {
            ResetShowCases();
        }
    }

    public void ChangeSeasonUp ()
    {
        int nextShownSeason = seasons.IndexOf(shownSeason) + 1;
        if (nextShownSeason >= seasons.Count)
        {
            nextShownSeason = 0;
        }
        SetNextSeason(nextShownSeason);
        if (pureShowCase)
        {
            showcaseStayActive = true;
        }
    }

    public void ChangeSeasonDown()
    {
        int nextShownSeason = seasons.IndexOf(shownSeason) - 1;
        if (nextShownSeason < 0)
        {
            nextShownSeason = seasons.Count - 1;
        }
        SetNextSeason(nextShownSeason);
        if (pureShowCase)
        {
            showcaseStayActive = true;
        }
    }

    void SetNextSeason (int index)
    {
        //Debug.Log("Updating season: " + Time.time);
        int lastShownSeason = seasons.IndexOf(shownSeason);
        shownSeason = seasons[index];

        seasonTabs[(seasons.Count - 1) - lastShownSeason].sprite = seasonOffSprite;
        seasonTabs[(seasons.Count - 1) - index].sprite = seasonOnSprite;

        if (index != lastShownSeason)
        {
            RetrieveScores(ScoreBoardID(), entryCount, shownSeason);
            GenerateHighscoreUI();

            
        }
        scoreBoardSeasonUI.text = shownSeason;
        if (shownSeason == "Old")
        {
            scoreBoardSeasonUI.text = "BEFORE F21";
        }
    }

    public void ShowScoreBoard(bool retreiveScores = false)
    {
        if (!pureShowCase)
        {
            for (int i = 0; i < visibleScoreBoards.Count; i++)
            {
                if (visibleScoreBoards[i].pureShowCase)
                {
                    visibleScoreBoards[i].ResetShowCase();
                }
            }
        }
        

        if (retreiveScores)
        {
            RetrieveScores(ScoreBoardID(), entryCount);
        }
        if (scores.Count > 1)
        {
            if ((scores[0].score > scores[scores.Count - 1].score && sortingType == SortingType.LowIsWin) || (scores[0].score < scores[scores.Count - 1].score && sortingType == SortingType.HighIsWin))
            {
                ResortAndSave();
            }
        }
        isActive = true;
        //This will just show whatever is stored, with no way of entering a new state
        GenerateHighscoreUI();
        scoreBoardPanelUI.gameObject.SetActive(true);
        scoreBoardTitleUI.text = "<size=80%><line-height=60%>" + scoreBoardTitle;
        visibleScoreBoards.Add(this);
        ArcadeInput.SetModal(this, true);
        if (ReadyForName())
        {
            seasonTabsText.gameObject.SetActive(false);
            seasonTabsContainer.gameObject.SetActive(false);

        }
        else
        {
            seasonTabsText.gameObject.SetActive(true);
            seasonTabsContainer.gameObject.SetActive(true);
        }
    }



    public bool IsActive()
    {
        return isActive;
    }

    public void EndScoreBoard()
    {
        newHighscoreData = null;
        for (int i = 0; i < scores.Count; i++)
        {
            if (scores[i].newData)
            {
                scores.RemoveAt(i);
                i--;
            }
        }
        scoreBoardPanelUI.gameObject.SetActive(false);
        isActive = false;
        visibleScoreBoards.Remove(this);
        ArcadeInput.EndModal(this);
    }

    public bool PrepareNewScore(float score)
    {
        
        RetrieveScores(ScoreBoardID(), entryCount);
        if (scores.Count > 1)
        {
            if ((scores[0].score > scores[scores.Count - 1].score && sortingType == SortingType.LowIsWin) || (scores[0].score < scores[scores.Count - 1].score && sortingType == SortingType.HighIsWin))
            {
                ResortAndSave();
            }
        }

        if (TestScore(score) && !scoreBoardSettingsInvalid)
        {
            //New score is valid
            scoreBoardTitleUI.text = "<size=80%><line-height=60%>" + scoreBoardTitle;
            bool added = false;
            string addString = newEntryMethodText;
            for (int i = 0; i < scores.Count; i++)
            {
                if ((sortingType == SortingType.LowIsWin && score < scores[i].score) || (sortingType == SortingType.HighIsWin && score > scores[i].score))
                {
                    ScoreData newData = new ScoreData(addString, score, activeSeason);
                    newData.newData = true;

                    scores.Insert(i, newData);
                    newHighscoreData = newData;
                    added = true;
                    break;

                }
            }
            if (!added)
            {
                ScoreData newData = new ScoreData(addString, score, activeSeason);
                newData.newData = true;
                scores.Add(newData);
                newHighscoreData = newData;
            }
            return true;
        }
        else
        {
            //New Score is not good enough!
            scoreBoardTitleUI.text = "<size=80%><line-height=60%>" + scoreBoardTitle + "\r\n<size=45%><color=#F5B909>" + returnToStartScreenMethod;
            return false;
        }


    }

    public void ClearScoreBoard()
    {
        for (int i = 0; i < entryCount; i++)
        {
            PlayerPrefs.DeleteKey(ScoreBoardID() + (i).ToString() + "name");
            PlayerPrefs.DeleteKey(ScoreBoardID() + (i).ToString() + "score");
        }

        scores.Clear();

        UpdatePlayerPrefs();

        newHighscoreData = null;
    }

    public bool SaveNewScore(string name)
    {
        
        if (scoreBoardSettingsInvalid)
        {
            return false;
        }
        if (newHighscoreData != null)
        {
            scores.Remove(newHighscoreData);
            AddHighscore(name, newHighscoreData.score, false, true);
            newHighscoreData = null;
            scoreBoardTitleUI.text = "<size=80%><line-height=60%>" + scoreBoardTitle + "\r\n<size=45%><color=#F5B909>" + returnToStartScreenMethod;
            seasonTabsText.gameObject.SetActive(true);
            seasonTabsContainer.gameObject.SetActive(true);
            return true;
        }
        
        return false;
    }

    public bool ReadyForName()
    {
        if (newHighscoreData != null)
        {
            return true;
        }
        return false;
    }

    public void SetFont(TMP_FontAsset font)
    {
        if (font != null)
        {
            TextMeshProUGUI[] allText = GetComponentsInChildren<TextMeshProUGUI>(true);

            for (int i = 0; i < allText.Length; i++)
            {
                allText[i].font = font;
                allText[i].fontSize *= globalFontScale;
                if (useGlobalFontColor)
                {
                    allText[i].color = globalFontColor;
                }
            }
        }
    }

    void Awake()
    {
        dataPath = Application.dataPath;
        waitHandle = new EventWaitHandle(true, EventResetMode.AutoReset, "ScoreBoardWrite");
        if (!pureShowCase)
        {
            instance = this;
        }
        else
        {
            showCases.Add(this);
            StartCoroutine(ShowCase());
        }

        if (deletePrefs)
        {
            ClearScoreBoard();
            deletePrefs = false;
        }

        GetApplicationIdentifier();
        Debug.Log("ScoreBoard ID: " + ScoreBoardID());
    }

    private void OnDestroy()
    {
        if (IsActive())
        {
            EndScoreBoard();
        }
        showCases.Remove(this);
        visibleScoreBoards.Remove(this);
    }

    public void EndShowCase()
    {
        endShowCase = true;
    }

    public void ResetShowCase()
    {
        showcaseStayActive = false;
        resetShowCase = true;
    }

    IEnumerator ShowCase()
    {
        float currentTime = Time.time;
        float delay = 0;
        if (IsActive())
        {
            delay = showDuration;

        }
        else
        {
            delay = hideDuration;
        }

        while ((currentTime + delay > Time.time || showcaseStayActive) && !endShowCase && !resetShowCase)
        {
            
            yield return null;
        }

        if (!endShowCase)
        {
            if (IsActive())
            {
                EndScoreBoard();
            }
            else
            {
                if (!resetShowCase)
                {
                    if (ScoreBoard.visibleScoreBoards.Find(c => !c.pureShowCase) == null)
                    ShowScoreBoard(true);
                }

            }
            resetShowCase = false;

            StartCoroutine(ShowCase());
        }
        else
        {
            resetShowCase = false;
            if (IsActive())
            {
                EndScoreBoard();
            }
        }

    }

    void GetApplicationIdentifier()
    {
        applicationIdentifier = Application.companyName + "." + Application.productName;
        if (applicationIdentifier == ".")
        {
            scoreBoardSettingsInvalid = true;
            scoreBoardTitle = "YOU HAVE TO FILL OUT APPLICATION ID!";
        }

        return;
        string upCode = "/../../";

        if (Application.isEditor)
        {
            upCode = "/../";
        }

        applicationIdentifier = Application.dataPath + upCode;

        applicationIdentifier.Replace("/", "\\");

        applicationIdentifier = Path.GetFullPath(applicationIdentifier);

    }

    string ScoreBoardID()
    {
        return applicationIdentifier + scoreBoardID;
    }
    // Start is called before the first frame update
    void Start()
    {
        if (scoreBoardID != "")
            RetrieveScores(ScoreBoardID(), entryCount); //Calls the function with the variables defined in the start of this class
    }

    void GenerateHighscoreUI()
    {
        ClearHighscoreUI();

        if (font != null)
        {
            SetFont(font);
        }

        int columnStart = 0;

        if (allTimeHigh != null && allTimeHigh.season != shownSeason)
        {
            HighscoreEntry newEntryElement = Instantiate(entryTemplateUI.gameObject, column1Container.transform).GetComponent<HighscoreEntry>();

            newEntryElement.AddData(1, allTimeHigh.name, allTimeHigh.score, this);
            highscoreEntries.Add(newEntryElement);
            string allTimeBest = "HIGH";
            string seasonTxt = allTimeHigh.season;
            if (sortingType == SortingType.LowIsWin)
            {
                allTimeBest = "LOW";
            }
            if (seasonTxt == "Old")
            {
                seasonTxt = "BEFORE F21";
            }

            newEntryElement.allTimeBest.GetComponentInChildren<TMP_Text>().text = "ALL TIME " + allTimeBest + ": " + seasonTxt;

            columnStart = 1;
        }

        for (int i = 0; i < 5 - columnStart && i < scores.Count; i++)
        {
            HighscoreEntry newEntryElement = Instantiate(entryTemplateUI.gameObject, column1Container.transform).GetComponent<HighscoreEntry>();

            if (scores[i].newData)
            {
                newEntryElement.GetComponent<Image>().enabled = true;
                StartCoroutine(newEntryElement.Blink(.5f, 0.2f, 1));

            }
            newEntryElement.AddData(i + 1 + columnStart, scores[i].name, scores[i].score, this);

            string allTimeBest = "HIGH";
            string seasonTxt = shownSeason;
            if (sortingType == SortingType.LowIsWin)
            {
                allTimeBest = "LOW";
            }
            if (seasonTxt == "Old")
            {
                seasonTxt = "BEFORE F21";
            }
            if (i == 0 && !scores[i].newData)
            {
                newEntryElement.allTimeBest.GetComponentInChildren<TMP_Text>().text = "ALL TIME " + allTimeBest + ": " + seasonTxt;
            }
            else
            {
                newEntryElement.allTimeBest.GetComponentInChildren<TMP_Text>().text = "";
            }
            highscoreEntries.Add(newEntryElement);
        }
        for (int i = 5 - columnStart; i < scores.Count - columnStart && i < entryCount - columnStart; i++)
        {
            HighscoreEntry newEntryElement = Instantiate(entryTemplateUI.gameObject, column2Container.transform).GetComponent<HighscoreEntry>();
            if (scores[i].newData)
            {
                newEntryElement.GetComponent<Image>().enabled = true;
                StartCoroutine(newEntryElement.Blink(.5f, 0.2f, 1));

            }

            newEntryElement.AddData(i + 1 + columnStart, scores[i].name, scores[i].score, this);
            highscoreEntries.Add(newEntryElement);
        }
    }

    List<string> GetAllSeasons ()
    {
        for (int i = 0; i < seasonTabs.Count; i++)
        {
            Destroy(seasonTabs[i].gameObject);
        }
        seasonTabs.Clear();

        string globalPath = ArcadeGlobals.SharedPath;
        seasons.Clear();
        if (File.Exists(globalPath + "\\" + "seasons.txt"))
        {
            seasons = ArcadeGlobals.ReadLinesFromFile(globalPath + "\\" + "seasons.txt");
        }

        return seasons;
    }

    void GetAllScoresFromText (string season = "")
    {
        GetAllSeasons();

        if (seasons.Count > 0)
        {
            activeSeason = seasons[seasons.Count - 1];
        }

        if (season != "" && seasons.Contains(season))
        {
            shownSeason = season;
        }
        else
        {
            shownSeason = activeSeason;
        }

        string sharedApplicationPath = ArcadeGlobals.SharedApplicationPath;

        //First find the all time high!

        string bestName = "";
        float bestScore = -1;
        string bestSeason = "";

        for (int s = 0; s < seasons.Count; s++)
        {

            string scoresPath = sharedApplicationPath + "\\scores_" + scoreBoardID + seasons[s] + ".txt";

            if (File.Exists(scoresPath))
            {
                List<string> allScores = ArcadeGlobals.ReadLinesFromFile(scoresPath);

                for (int i = 0; i < allScores.Count; i++)
                {
                    string[] scoreParts = allScores[i].Split(';');

                    if (scoreParts.Length == 2)
                    {
                        string name = "";
                        float score = -1;

                        for (int j = 0; j < scoreParts.Length; j++)
                        {
                            string[] valueParts = scoreParts[j].Split(':');

                            if (valueParts[0] == "name")
                            {
                                name = valueParts[1];
                            }
                            else if (valueParts[0] == "score")
                            {
                                if (float.TryParse(valueParts[1], out float result))
                                {
                                    score = result;
                                }
                            }
                        }

                        if (score > -1)
                        {
                            if (sortingType == SortingType.HighIsWin && (score > bestScore || bestScore == -1))
                            {
                                bestScore = score;
                                bestName = name;
                                bestSeason = seasons[s];
                            }
                            else if (sortingType == SortingType.LowIsWin && (score < bestScore || bestScore == -1))
                            {
                                bestScore = score;
                                bestName = name;
                                bestSeason = seasons[s];
                            }
                        }
                    }
                }
            }
        }

        if (bestScore > -1)
        {
            allTimeHigh = new ScoreData(bestName, bestScore, bestSeason);
        }

        {
            

            string scoresPath = sharedApplicationPath + "\\scores_" + scoreBoardID + shownSeason + ".txt";

            
            if (File.Exists(scoresPath))
            {
                List<string> allScores = ArcadeGlobals.ReadLinesFromFile(scoresPath);

                for (int i = 0; i < allScores.Count; i++)
                {
                    string[] scoreParts = allScores[i].Split(';');

                    if (scoreParts.Length == 2)
                    {
                        string name = "";
                        float score = -1;

                        for (int j = 0; j < scoreParts.Length; j++)
                        {
                            string[] valueParts = scoreParts[j].Split(':');

                            if (valueParts[0] == "name")
                            {
                                name = valueParts[1];
                            }
                            else if (valueParts[0] == "score")
                            {
                                if (float.TryParse(valueParts[1], out float result))
                                {
                                    score = result;
                                }
                            }
                        }
                        scores.Add(new ScoreData(name, score, shownSeason));
                    }
                }
            }
        }

        
    }

    void RetrieveScores(string highscoreID, int highscoreCount, string season = "")
    {
        //Debug.Log("Retreiving: " + shownSeason);
        allTimeHigh = null;
        scores.Clear();
        string keyBase = highscoreID;

        GetAllScoresFromText(season);

        bool saveIt = false;
        if (scores.Count == 0)
        {
            if (activeSeason == "")
            {
                activeSeason = "Old";
                shownSeason = "Old";
                for (int i = 0; i < highscoreCount; i++)
                {
                    string name = PlayerPrefs.GetString(keyBase + i.ToString() + "name", "");
                    if (name != "")
                    {
                        float score = PlayerPrefs.GetFloat(keyBase + i.ToString() + "score", -1);
                        if (score != -1)
                        {
                            scores.Add(new ScoreData(name, score, shownSeason));

                            PlayerPrefs.DeleteKey(keyBase + i.ToString() + "name");
                            PlayerPrefs.DeleteKey(keyBase + i.ToString() + "score");
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                saveIt = true;
            }
            
            

            if (!seasons.Contains(activeSeason))
            {
                seasons.Insert(0, activeSeason);
                /*
                string sharedPath = ArcadeGlobals.SharedPath;
                Debug.Log("Shared Path: " + sharedPath);
                if (!File.Exists(sharedPath + "\\" + "seasons.txt"))
                {
                    ArcadeGlobals.WriteTextToFile("Old", sharedPath + "\\" + "seasons.txt");
                }
                else
                {
                    seasons.Insert(0, activeSeason);
                    ArcadeGlobals.WriteLinesToFile(seasons.ToArray(), sharedPath + "\\" + "seasons.txt");
                }
                */
            }
        }

        for (int i = seasons.Count-1; i >= 0; i--)
        {
            seasonTabs.Add(Instantiate(seasonTabTemplate.gameObject, seasonTabsContainer).GetComponent<Image>());
            seasonTabs[seasonTabs.Count - 1].GetComponentInChildren<TMP_Text>().text = seasons[i];
            seasonTabs[seasonTabs.Count - 1].gameObject.SetActive(true);
        }

        SetNextSeason(seasons.IndexOf(shownSeason));

        if (testFullScoreBoard && scores.Count < highscoreCount)
        {
            while (scores.Count < highscoreCount)
            {
                float randomScore = UnityEngine.Random.Range(minTestScore, maxTestScore);

                if (TestScore(randomScore))
                {
                    string[] randomNameElements = new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "X", "Y", "Z", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
                    int nameLength = 5;
                    string randomName = "";
                    for (int j = 0; j < nameLength; j++)
                    {
                        int randomNumber = UnityEngine.Random.Range(0, randomNameElements.Length);
                        randomName += randomNameElements[randomNumber];
                    }

                    AddHighscore(randomName, randomScore, true, false);
                }
                saveIt = true;
            }

        }

        if (saveIt)
        UpdatePlayerPrefs();

    }

    public void ChangeSeason (TMP_Dropdown dropdown)
    {
        if (dropdown.options[dropdown.value].text != shownSeason)
        {
            shownSeason = dropdown.options[dropdown.value].text;
            RetrieveScores(ScoreBoardID(), entryCount, shownSeason);
            GenerateHighscoreUI();
        }

    }

    int IndexOfSeason (string season)
    {
        int index = seasons.IndexOf(season);

        return (seasons.Count - 1) - index;
    }

    bool TestScore(float score)
    {
        if (scores.Count < entryCount)
        {
            return true;
        }

        if ((sortingType == SortingType.LowIsWin && score < scores[scores.Count - 1].score) || (sortingType == SortingType.HighIsWin && score > scores[scores.Count - 1].score))
        {
            return true;
        }

        return false;
    }

    public void CancelNewHighscore ()
    {
        if (ReadyForName())
        {
            scores.Remove(newHighscoreData);
            newHighscoreData = null;
            if (cancelNewScore != null)
            {
                cancelNewScore.Invoke();
                cancelNewScore = null;
            }
        }
        GenerateHighscoreUI();
    }
    void AddHighscore(string name, float score, bool waitWrite, bool generateUI = true)
    {
        if (scores.Count == 0)
        {
            scores.Insert(0, new ScoreData(name, score, activeSeason));
            if (!waitWrite)
                UpdatePlayerPrefs();

        }
        else
        {
            bool isAdded = false;
            for (int i = 0; i < scores.Count; i++)
            {
                if ((sortingType == SortingType.LowIsWin && score < scores[i].score) || (sortingType == SortingType.HighIsWin && score > scores[i].score))
                {
                    scores.Insert(i, new ScoreData(name, score, activeSeason));
                    Debug.Log("Inserting: " + name + ", Score: " + score.ToString(scoreFormatter) + ", at: " + i);
                    while (scores.Count > entryCount)
                    {
                        //PlayerPrefs.DeleteKey(ScoreBoardID() + (entryCount - 1).ToString() + "name");
                        //PlayerPrefs.DeleteKey(ScoreBoardID() + (entryCount - 1).ToString() + "score");

                        scores.RemoveAt(entryCount);
                    }
                    Debug.Log("Last? " + scores[scores.Count - 1].name);
                    //UpdatePlayerPrefs();
                    isAdded = true;
                    break;
                }
            }
            if (!isAdded)
            {
                scores.Add(new ScoreData(name, score, activeSeason));
                
            }
            if (!waitWrite)
                UpdatePlayerPrefs();
        }



        if (generateUI)
        {
            GenerateHighscoreUI();
        }
    }



    void WriteScores ()
    {
        string sharedApplicationPath = ArcadeGlobals.SharedApplicationPath;

        string[] scoresAsText = new string[scores.Count];

        //Debug.Log("Scores: " + scores.Count + ", texts: " + scoresAsText.Length);
        for (int i = 0; i < scores.Count; i++)
        {
            scoresAsText[i] = "name:" + scores[i].name + ";score:" + scores[i].score;
            //Debug.Log("Entry " + i + ": " + scoresAsText[i]);
        }

        ArcadeGlobals.WriteLinesToFile(scoresAsText, sharedApplicationPath + "\\scores_" + scoreBoardID + activeSeason + ".txt");

    }

    void ClearHighscoreUI()
    {

        for (int i = 0; i < highscoreEntries.Count; i++)
        {
            Destroy(highscoreEntries[i].gameObject);
        }

        highscoreEntries.Clear();
    }

    void UpdatePlayerPrefs()
    {
        //writeScoresThread = new Thread(WriteScores);
        //awaitWriting = true;
        //writeScoresThread.Start();
        WriteScores();
        return;
        for (int i = 0; i < scores.Count; i++)
        {
            PlayerPrefs.SetString(ScoreBoardID() + i.ToString() + "name", scores[i].name);
            PlayerPrefs.SetFloat(ScoreBoardID() + i.ToString() + "score", scores[i].score);
        }
    }

    void ResortAndSave()
    {
        if (sortingType == SortingType.HighIsWin)
        {
            scores.Sort((x, y) => y.score.CompareTo(x.score));
        }
        else
        {
            scores.Sort((x, y) => x.score.CompareTo(y.score));
        }

        UpdatePlayerPrefs();
    }

   

    /*

    List<TMP_Dropdown.OptionData> CreateSeasonOptions()
    {
        List<TMP_Dropdown.OptionData> result = new List<TMP_Dropdown.OptionData>();


        for (int i = seasons.Count-1; i >= 0; i--)
        {
            result.Add(new TMP_Dropdown.OptionData(seasons[i]));
        }

        return result;
    }
    */

}


