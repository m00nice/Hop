using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HighscoreEntry : MonoBehaviour
{
    public Image frame;
    public Image help;
    public TMP_Text placementUI;
    public TMP_Text entryNameUI;
    public TMP_Text scoreUI;
    public AnimationCurve blinkCurve;
    public Sprite blinkSprite;
    public Sprite allTimeBestBGSprite;
    public GameObject allTimeBest;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void AddData(int placement, string entryName, float score, ScoreBoard scoreBoard)
    {
        frame.gameObject.SetActive(true);
        placementUI.text = placement.ToString();
        entryNameUI.text = entryName;
        scoreUI.text = score.ToString(scoreBoard.scoreFormatter);
        if (placement == 1)
        {
            allTimeBest.SetActive(true);
            frame.sprite = allTimeBestBGSprite;
        }
        else
        {
            allTimeBest.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator Blink (float blinkTime, float startAlpha, float endAlpha)
    {
        float current = Time.time;
        frame.sprite = blinkSprite;
        while (current+ blinkTime > Time.time)
        {
            float currentFactor = blinkCurve.Evaluate((Time.time - current) / blinkTime);

            float currentAlpha = Mathf.Lerp(startAlpha, endAlpha, currentFactor);
            Color currentColor = frame.color;
            currentColor.a = currentAlpha;
            frame.color = currentColor;
            yield return null;
        }

        Color currentColorFinal = frame.color;
        currentColorFinal.a = blinkCurve.Evaluate(1);
        frame.color = currentColorFinal;
        StartCoroutine(Blink(blinkTime, startAlpha, endAlpha));
    }
}
