using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartTournamentSetupExample : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        ArcadeInput.SetModal(this);
    }

    // Update is called once per frame
    void Update()
    {
        if ((ActiveTournament.instance == null || !ActiveTournament.instance.isActive) && !TournamentSetup.instance.IsActive && ArcadeInput.InputInitiated(0, ArcadeInputType.ButtonA, AxisType.Raw, this))
        {
            List<GameData> testData = new List<GameData>();
            testData.Add(new GameData("Game1", "Comp.GameName1"));
            testData.Add(new GameData("Game2", "Comp.GameName2"));
            testData.Add(new GameData("Game3", "Comp.GameName3"));
            testData.Add(new GameData("Game4", "Comp.GameName4"));
            TournamentSetup.instance.Activate(testData, () => { TournamentSetupClosing(); });
        }

        if ((ActiveTournament.instance == null || !ActiveTournament.instance.isActive) && ArcadeInput.InputInitiated(0, ArcadeInputType.ButtonB, AxisType.Raw, this))
        {
            if (TournamentSetup.lastCreatedIdentifier != "")
            {
                ActiveTournament.TryActivate(() => { Callback(); }, () => { BattleInitiate(); }, () => { TournamentClosing(); }, TournamentSetup.lastCreatedIdentifier, TournamentSetup.lastCreatedName, TournamentStatus.ReadyForSubscription, false);
            }
            
        }
    }

    public void TournamentSetupClosing()
    {
        Debug.Log("Closing Setup");
    }

    public void Callback ()
    {
        Debug.Log("Saved! Callback");
    }

    public void BattleInitiate()
    {
        Debug.Log("Battle Init callback");
    }

    public void TournamentClosing()
    {
        Debug.Log("Tournament Was closed!");
    }
}
