using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
//[System.Serializable]
public class KnockoutPlayer
{
    public string id;

    [NonSerialized]
    private TournamentTeam player;
    [NonSerialized]
    private TournamentBattle battle;

    public int battleIndex = -1;
    public int levelIndex;
    [NonSerialized]
    private KnockoutPlayerUI ui;
    [NonSerialized]
    private List<KnockoutPlayer> leadIns = new List<KnockoutPlayer>();
    [NonSerialized]
    private KnockoutPlayer leadOut = null;

    public KnockoutPlayerUI UI
    {
        get
        {
            return ui;
        }
        set
        {
            ui = value;
        }
    }

    public TournamentTeam Player
    {
        get
        {
            return player;
        }
        set
        {
            player = value;
        }
    }

    public TournamentBattle Battle
    {
        get
        {
            return battle;
        }
        set
        {
            battle = value;
        }
    }

    public List<KnockoutPlayer> Ins
    {
        get
        {
            return leadIns;
        }
        set
        {
            leadIns = value;
        }
    }

    public KnockoutPlayer Out
    {
        get
        {
            return leadOut;
        }
        set
        {
            leadOut = value;
        }
    }
}

public class KnockoutStructure : MonoBehaviour
{
    public static KnockoutStructure instance;
    public int players;

    public RectTransform knockoutArea;
    public RectTransform sectionTemplate;
    public RectTransform playerTemplate;

    public List<KnockoutPlayer> knockoutDatas = new List<KnockoutPlayer>();
    
    public bool buildNow;

    public KnockoutPlayer p1Selected;
    public KnockoutPlayer p2Selected;
    public Color[] shiftingColorsPlayers;
    public Color[] shiftingColorsSections;
    int numDatas = 0;
    int lastPlayers;
    public int lastShift;
    public int shiftCount;
    List<Transform> sections = new List<Transform>();
    bool createPlayerButtons;

    int battleIndex;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        
    }

    private void Update()
    {
        if (ActiveTournament.instance != null && ActiveTournament.instance.isActive)
        {
            if (ActiveTournament.instance.lockPlayers)
            {

                return;

            }


            if (p1Selected != null && p2Selected != null && p1Selected.Battle == p2Selected.Battle && p1Selected.Battle.status == BattleStatus.Pending)
            {
                if (ActiveTournament.instance.activeBattle != p1Selected.Battle)
                {
                    ActiveTournament.instance.koNextBattleUI1.text = p1Selected.Player.name;
                    ActiveTournament.instance.koNextBattleUI2.text = p2Selected.Player.name;

                    ActiveTournament.instance.koNextBattleUI1.color = ActiveTournament.instance.player1Color * 1.25f;
                    ActiveTournament.instance.koNextBattleUI2.color = ActiveTournament.instance.player2Color * 1.25f;
                    ActiveTournament.instance.koVsUI.text = "vs";
                }

                ActiveTournament.instance.activeBattle = p1Selected.Battle;

                if (ArcadeInput.InputInitiated(1, ArcadeInputType.ButtonStart, AxisType.Raw, ActiveTournament.instance) || (ArcadeInput.InputInitiated(1, ArcadeInputType.ButtonA, AxisType.Raw, ActiveTournament.instance)))
                {
                    if (!ActiveTournament.instance.active1Confirmed && ActiveTournament.instance.activeBattle != null)
                    {
                        Debug.Log("Confirm 1");
                        ActiveTournament.instance.active1Confirmed = true;
                        //StartCoroutine(BlinkName());
                        ActiveTournament.instance.koNextBattleUI1.text += "<size=55%><color=#FFFFFF>\r\nCONFIRMED";

                        if (ActiveTournament.instance.active1Confirmed && ActiveTournament.instance.active2Confirmed && ActiveTournament.instance.activeBattle != null)
                        {
                            Debug.Log("Inititate Battle");
                            ActiveTournament.instance.InitiateBattle(ActiveTournament.instance.activeBattle, ActiveTournament.instance.activePlayer1, ActiveTournament.instance.activePlayer2, -1, -1);
                        }
                    }
                }
                if (ArcadeInput.InputInitiated(2, ArcadeInputType.ButtonStart, AxisType.Raw, ActiveTournament.instance) || (ArcadeInput.InputInitiated(2, ArcadeInputType.ButtonA, AxisType.Raw, ActiveTournament.instance)))
                {
                    if (!ActiveTournament.instance.active2Confirmed && ActiveTournament.instance.activeBattle != null)
                    {
                        Debug.Log("Confirm 2");
                        ActiveTournament.instance.active2Confirmed = true;
                        //StartCoroutine(BlinkName());
                        ActiveTournament.instance.koNextBattleUI2.text += "<size=55%><color=#FFFFFF>\r\nCONFIRMED";

                        if (ActiveTournament.instance.active1Confirmed && ActiveTournament.instance.active2Confirmed && ActiveTournament.instance.activeBattle != null)
                        {
                            Debug.Log("Inititate Battle");
                            ActiveTournament.instance.InitiateBattle(ActiveTournament.instance.activeBattle, ActiveTournament.instance.activePlayer1, ActiveTournament.instance.activePlayer2, -1, -1);
                        }
                    }
                }

            }
            else
            {
                ActiveTournament.instance.koNextBattleUI1.text = "";
                //ActiveTournament.instance.koNextBattleUI1.color = Color.white;
                ActiveTournament.instance.koNextBattleUI2.text = "";
                ActiveTournament.instance.active1Confirmed = false;
                ActiveTournament.instance.active2Confirmed = false;
                ActiveTournament.instance.koVsUI.text = "<size=70%>SELECTED PLAYERS HAVE NO FIGHTS";
                ActiveTournament.instance.activeBattle = null;
            }
        }
    }

    public void Clear()
    {
        for (int i = 0; i < sections.Count; i++)
        {
            sections[i].SetParent(null);
            Destroy(sections[i].gameObject);
        }

        sections.Clear();

        knockoutDatas.Clear();
    }

    public void BuildKnockoutStructure (int playerSize)
    {
        numDatas = 0;
        playerSize = ToNextPOT(playerSize);
        players = playerSize;
        createPlayerButtons = (ActiveTournament.instance != null && ActiveTournament.instance.isActive);
        for (int i = 0; i < sections.Count; i++)
        {
            sections[i].SetParent(null);
            Destroy(sections[i].gameObject);
        }

        sections.Clear();

        knockoutDatas.Clear();
        lastShift = 0;
        shiftCount = 0;
        if (ActiveTournament.instance != null && ActiveTournament.instance.isActive)
        {
            battleIndex = ActiveTournament.instance.battles.Count - 1;
        }

        //Debug.Log("Yeah?");
        KnockoutPlayer a = new KnockoutPlayer();
        KnockoutPlayer b = new KnockoutPlayer();
        a.levelIndex = 0;
        b.levelIndex = 1;

        if (ActiveTournament.instance != null && ActiveTournament.instance.isActive)
        {
            a.Battle = ActiveTournament.instance.battles[battleIndex];
            b.Battle = ActiveTournament.instance.battles[battleIndex];
            a.battleIndex = 0;
            b.battleIndex = 1;

            if (ActiveTournament.instance.battles[battleIndex].TeamA != null)
            {
                a.Player = ActiveTournament.instance.battles[battleIndex].TeamA;
            }
            if (ActiveTournament.instance.battles[battleIndex].TeamB != null)
            {
                b.Player = ActiveTournament.instance.battles[battleIndex].TeamB;
            }
        }

        int halfSections = Mathf.RoundToInt((float)Math.Log(playerSize, 2));
        
        knockoutDatas.Add(a);
        knockoutDatas.Add(b);
        numDatas += 2;
        BuildData(a, halfSections-1, 0);
        BuildData(b, halfSections-1, 1);
        //Debug.Log("Datas: " + numDatas);
        int centerDivision = 30;
        Rect areaSize = knockoutArea.rect;

        
        float sectionWidth = (areaSize.width - centerDivision) / (halfSections * 2);

        //This is to indent them halfways
        //float sectionWidth = areaSize.width / (halfSections + 1);

        float playersHeight = sectionWidth * .25f;
        //float playersHeight = areaSize.height / ((playerSize/2)*1.25f);


        int sectionPlayers = playerSize;
        float factor = .5f;
        
        float playerPosOffset = -.25f;
        int expX = 0;
        int expXDirection = 1;

        float sectionBaseHeight = areaSize.height - playersHeight;
        float playerPosDistanceBase = sectionBaseHeight / ((playerSize / 2)-1);
        int expSub = 0;

        float sectionOffset = sectionWidth / 2;
        Vector2 currentSectionPosition = new Vector2(-areaSize.width * .5f + sectionWidth*.5f, 0);

        for (int i = 0; i < halfSections*2; i++)
        {
            RectTransform newSection = Instantiate(sectionTemplate.gameObject, knockoutArea).transform as RectTransform;
            sections.Add(newSection);
            if (shiftCount > 0)
            {
                lastShift = Mathf.Abs(lastShift - 1);
                shiftCount = 0;
            }

            newSection.GetComponent<Image>().color = shiftingColorsSections[lastShift];
            shiftCount++;
            newSection.localPosition = currentSectionPosition;

            currentSectionPosition.x += sectionWidth;
            //This is to indent them halfways, but only works for few before being a stupid approach
            
            if (i == halfSections-1)
            {
                lastShift = Mathf.Abs(lastShift - 1);
                currentSectionPosition.x += centerDivision;
            }
            /*
            else
            {
                currentSectionPosition.x += sectionWidth;
            }
            */

            newSection.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, sectionWidth);
            newSection.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, areaSize.height);
            sectionPlayers = Mathf.RoundToInt((float)sectionPlayers* factor);
            //Debug.Log("ExpX: " + expX + ", dir: " + expXDirection);

            float playerPosDistance = playerPosDistanceBase * Mathf.Pow(2f, expX- expSub);

            Rect sectionArea = newSection.rect;

            float newExpSize = 0.25f * Mathf.Pow(2f, expX);
            
            playerPosOffset += newExpSize * expXDirection;
            //Debug.Log("ExpX: " + expX + ", dir: " + expXDirection + ", dist.: " + playerPosDistance + ", offset: " + playerPosOffset);
            //Debug.Log("Exp size: " + newExpSize + ", offset: " + playerPosOffset);
            Vector2 playerPosition = new Vector2(0, -sectionArea.height*.5f + playerPosOffset * playerPosDistanceBase + playersHeight * .5f);

            expX += expXDirection;

            for (int j = 0; j < sectionPlayers; j++)
            {
                RectTransform newPlayer = Instantiate(playerTemplate.gameObject, newSection).transform as RectTransform;
                newPlayer.gameObject.SetActive(true);
                newPlayer.localPosition = playerPosition;

                newPlayer.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, sectionArea.width);
                newPlayer.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Min(playersHeight, sectionArea.height/(playerSize/2)));

                playerPosition.y += playerPosDistance;

                if (expXDirection < 1)
                {
                    KnockoutPlayerUI knockoutPlayerUI = newPlayer.GetComponent<KnockoutPlayerUI>();
                    knockoutPlayerUI.invertInOuts = true;
                }
            }

            //Debug.Log("Section Pl.: " + sectionPlayers);
            if (sectionPlayers == 1 && factor != 1)
            {
                factor = 1;
                expX -= expXDirection;
                expXDirection = 0;
            }
            else if (factor == 1)
            {
                factor = 2;
                //expX -= 1;
                expSub = 1;
                expXDirection = -1;
            }

        }
        shiftCount = 0;
        lastShift = 0;
        ConnectDatas(knockoutDatas[0], 0, 2);
        ConnectDatas(knockoutDatas[1], 0, 2);
    }

    void ConnectDatas (KnockoutPlayer current, int level, int levelSize)
    {
        int sectionIndex = -1;
        int levelIndex = current.levelIndex;

        if (current.levelIndex >= levelSize/2)
        {
            levelIndex -= levelSize/2;
            sectionIndex = knockoutArea.childCount / 2 + level;
        }
        else
        {
            sectionIndex = knockoutArea.childCount / 2 - (1 + level);
        }

        if (levelIndex >= knockoutArea.GetChild(sectionIndex).childCount)
        {
            Debug.Log("Is Broken");
        }
        else
        {
            //Debug.Log("Sect: " + sectionIndex + ", lvl.: " + levelIndex);

            KnockoutPlayerUI knockoutPlayerUI = knockoutArea.GetChild(sectionIndex).GetChild(levelIndex).GetComponent<KnockoutPlayerUI>();
            string labelText = "";
            if (current.Ins.Count == 0)
            {
                labelText = "PLAYER";
            }
            knockoutPlayerUI.Data = current;
            current.UI = knockoutPlayerUI;

            if (current.Out != null)
            {
                current.UI.CreateUI(current.Out.UI, labelText, createPlayerButtons);
            }
            else
            {
                current.UI.CreateUI(null, labelText, createPlayerButtons);
            }
        }

        if (current.Ins.Count > 0)
        {
            level++;
            levelSize *= 2;
            ConnectDatas(current.Ins[0], level, levelSize);
            ConnectDatas(current.Ins[1], level, levelSize);
        }

    }

    public List<KnockoutPlayer> GetKOByBattle (TournamentBattle battle)
    {
        List<KnockoutPlayer> result = new List<KnockoutPlayer>();
        SearchTreeForBattle(knockoutDatas[0], battle, result);
        SearchTreeForBattle(knockoutDatas[1], battle, result);
        return result;
    }

    void SearchTreeForBattle(KnockoutPlayer current, TournamentBattle battle, List<KnockoutPlayer> result)
    {
        if (current.Battle == battle)
        {
            result.Add(current);
        }
        if (current.Ins != null)
        {
            for (int i = 0; i < current.Ins.Count; i++)
            {
                SearchTreeForBattle(current.Ins[i], battle, result);
            }
        }
    }

    void BuildData (KnockoutPlayer current, int level, int side)
    {
        current.id = level.ToString() + current.levelIndex.ToString();
        KnockoutPlayer a = new KnockoutPlayer();
        KnockoutPlayer b = new KnockoutPlayer();

        if (current.Battle != null)
        {
            TournamentBattle battle = ActiveTournament.instance.battles.Find(c => c.battleID == current.Battle.leadupBattles[side]);
            a.Battle = battle;
            b.Battle = battle;

            a.battleIndex = 0;
            b.battleIndex = 1;

            if (battle.TeamA != null)
            {
                a.Player = battle.TeamA;
            }
            if (battle.TeamB != null)
            {
                b.Player = battle.TeamB;
            }
        }

        numDatas += 2;
        a.Out = current;
        b.Out = current;

        a.levelIndex = 2 * current.levelIndex;
        b.levelIndex = 2 * current.levelIndex + 1;

        current.Ins.Add(a);
        current.Ins.Add(b);

        level--;
        if (level > 0)
        {
            BuildData(a, level, 0);
            BuildData(b, level, 1);
        }
    }

    bool IsPowerOfTwo(int x)
    {
        return x > 0 && (x & (x - 1)) == 0;
    }

    int ToNextPOT(int x)
    {
        if (x < 0) { return 0; }
        --x;
        x |= x >> 1;
        x |= x >> 2;
        x |= x >> 4;
        x |= x >> 8;
        x |= x >> 16;
        return x + 1;
    }

    int ToPreviousPOT(int x)
    {
        int next = ToNextPOT(x);
        return next >> 1;
        //return next - x < x - prev ? next : prev;
    }
}
