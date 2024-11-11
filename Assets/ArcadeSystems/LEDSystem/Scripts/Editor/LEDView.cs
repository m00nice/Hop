using UnityEngine;
using UnityEditor;

public class LEDView : EditorWindow
{
    bool drawTexture = false;
    Texture2D mask = null;
    // Add menu named "My Window" to the Window menu
    [MenuItem("LED Panel/Simulated panel output")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        LEDView window = (LEDView)EditorWindow.GetWindow(typeof(LEDView));

        window.Show();
    }

    void OnGUI()
    {
        if (mask == null)
        {
            mask = Instantiate(Resources.Load("LEDNoLight", typeof(Texture2D))) as Texture2D;
        }
        Rect currentViewSize = new Rect(0, 0, position.width-20, position.height - 60);
        Rect fullViewSize = new Rect(0, 0, position.width, position.height);


        if (currentViewSize.height < currentViewSize.width / 3)
        {
            currentViewSize.width = currentViewSize.height * 3;
        }
        else
        {
            currentViewSize.height = currentViewSize.width / 3;
        }

        currentViewSize.x = (float)position.width * .5f - currentViewSize.width * .5f;
        currentViewSize.y = 35;


        Color preColor = GUI.color;
        Color preBgColor = GUI.backgroundColor;
        int preFontSize = GUI.skin.label.fontSize;
        GUI.color = Color.black;
        GUI.DrawTexture(fullViewSize, EditorGUIUtility.whiteTexture);

        GUI.color = Color.white;

        float margin = 3f;
        Rect bgBoxRect = currentViewSize;
        bgBoxRect.x -= margin;
        bgBoxRect.y -= margin;
        bgBoxRect.xMax += margin*2;
        bgBoxRect.yMax += margin*2;
        
        GUI.color = new Color(.2f, .2f, .2f, 1);
        GUI.DrawTexture(bgBoxRect, EditorGUIUtility.whiteTexture);
        
        bgBoxRect.x += 1;
        bgBoxRect.y += 1;
        bgBoxRect.xMax -= 2;
        bgBoxRect.yMax -= 2;
        GUI.color = new Color(.5f, .5f, .5f, 1);
        GUI.DrawTexture(bgBoxRect, EditorGUIUtility.whiteTexture);

        GUI.color = Color.white;
        if (LEDCam.ledTexture != null && drawTexture)
        {
            if (LEDCam.ledVisualizer == null)
            {
                LEDCam ledCam = FindObjectOfType<LEDCam>();
                if (ledCam != null)
                {
                    Debug.Log("Awake");
                    ledCam.AwakeProofed();
                }
                return;
            }

            LEDCam.ledVisualizer.SetFloat("_Overglow", 1.3f);
            //GUI.color = Color.white * 2;
            Color newBg = Color.black;
            newBg.a = 0;
            GUI.backgroundColor = newBg;
            
            EditorGUI.DrawPreviewTexture(currentViewSize, LEDCam.ledTexture, LEDCam.ledVisualizer, ScaleMode.ScaleToFit);
            //GUI.DrawTexture(currentViewSize, LEDCam.ledTexture, ScaleMode.ScaleToFit);
            //GUI.Box(currentViewSize, "Active LED Signal");
            //GUI.Box(currentViewSize, "Active LED Signal");
            GUI.color = Color.white;
        }
        else
        {
            if (LEDCam.ledTexture != null)
            {
                drawTexture = true;
            }
            else
            {
                drawTexture = false;
            }
            
            
            Color newFg = Color.white;
            Color newBg = Color.black;
            newBg.a = 1;
            newFg.a = 1f;

            GUI.backgroundColor = newBg;
            GUI.color = newFg;
            GUI.Box(currentViewSize, "");
            GUI.DrawTexture(currentViewSize, mask, ScaleMode.ScaleToFit);

            newBg.a = 0;
            GUI.backgroundColor = newBg;
            newFg.a = 1.0f;
            GUI.color = newFg;
            Rect boxSize = currentViewSize;
            boxSize.y -= 38;
            GUI.Box(boxSize, "No LED Signal!\r\nHUB signal will show");
            GUI.color = Color.white;
        }
        GUI.skin.label.fontSize *= 2;
        GUI.Label(new Rect(currentViewSize.x, currentViewSize.y-35, 400,35), "LED Panel");
        GUI.skin.label.fontSize = preFontSize;
        GUI.color = preColor;
        GUI.backgroundColor = preBgColor;
    }

    private void Update()
    {
        Repaint();
    }
}