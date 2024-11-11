using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GroupBattleUI : MonoBehaviour
{
    public Image background;
    public TMP_Text battleTextUI;
    public AnimationCurve blinkCurve;
    public Color blinkTarget;
    public float blinkDuration;
    Coroutine blink = null;
    
    bool stopBlink;
    public void SetData (TournamentBattle battle, TournamentBattle nextBattle, TournamentTeam owner, TournamentTeam other)
    {
        battleTextUI.text = "vs " + other.name;
        if (battle.Winner != null)
        {
            
            if (battle.status == BattleStatus.Won)
            {
                if (battle.Winner == owner)
                {
                    background.color = ActiveTournament.instance.battleWinnerColor;
                    if (nextBattle == battle)
                    {
                        stopBlink = true;
                        blinkDuration = .15f;
                        blink = StartCoroutine(Blink());
                    }
                }
                else
                {
                    background.color = ActiveTournament.instance.battleLoserColor;
                    if (nextBattle == battle)
                    {
                        //stopBlink = true;
                        //blinkDuration = .25f;
                        //blink = StartCoroutine(Blink());
                    }
                }
            }
            
        }
        else
        {
            if (battle.status == BattleStatus.Pending)
            {
                if (battle == nextBattle)
                {
                    background.color = ActiveTournament.instance.battleNextColor;
                    blink = StartCoroutine(Blink());
                }
                else
                {
                    background.color = ActiveTournament.instance.battlePendingColor;
                }

            }
            else if (battle.status == BattleStatus.Draw)
            {
                background.color = ActiveTournament.instance.battleDrawColor;
                if (nextBattle == battle)
                {
                    stopBlink = true;
                    blinkDuration = .15f;
                    blink = StartCoroutine(Blink());
                }
            }
            else if (battle.status == BattleStatus.Cancelled)
            {
                background.color = ActiveTournament.instance.battleCancelledColor;
            }
        }
    }

    IEnumerator Blink ()
    {
        Color start = background.color;
        float baseStart = Time.time;
        float startTime = Time.time;
        float maxTime = 1;
        while (!stopBlink || (stopBlink && baseStart + maxTime > Time.time))
        {
            float currentDuration = Time.time - startTime;
            if (currentDuration > blinkDuration)
            {
                currentDuration -= blinkDuration;
                startTime = Time.time+ currentDuration;
            }
            float factor = blinkCurve.Evaluate(currentDuration / blinkDuration);

            background.color = Color.Lerp(start, blinkTarget, factor);

            yield return null;
        }
        background.color = start;
    }

    private void OnDestroy()
    {
        if (blink != null)
        {
            StopCoroutine(blink);
        }
    }
}
