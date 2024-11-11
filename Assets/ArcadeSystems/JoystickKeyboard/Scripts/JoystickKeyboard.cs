using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.Events;
public enum KeyboardAction
{
    Default = 0,
    MoveDown = 1,
    MoveUp = 2,
    MoveLeft = 3,
    MoveRight = 4,
    PushSelected = 5,
}

[System.Serializable]
public class LetterGridRow
{
    public List<Letter> columns = new List<Letter>();
}

[System.Serializable]
public class Letter
{
    public bool selectable = true;
    public string letter;
    public bool useLabel = false;
    public string label = "";
    public TextMeshProUGUI textField;
    public Image imageField;
    public Sprite overrideSprite;
    public float scaleFactor = 1;
    public bool overrideDefault = false;
    public Color defaultColor;
    public bool overrideSelected = false;
    public Color selectedColor;
    public Action pushAction;
    public bool validate = false;
    public ArcadeInputType pushByInput = ArcadeInputType.Undefined;
    public UnityEvent onPush;
}
[DefaultExecutionOrder(50000)]
public class JoystickKeyboard : MonoBehaviour
{
    
    public static List<JoystickKeyboard> keyboards = new List<JoystickKeyboard>();
    public int id = 0;
    public int registerInputsFromPlayer = 1;
    public bool replaceWithIdeniticalID;
    public bool colorButtonsAsInput = false;
    public bool hideSelection = false;
    public bool hideCharacters = false;
    public bool automaticAcceptWhenValid = false;
    private bool isModal = true;
    //public bool isSecondary;
    public bool registerInputsInternally = false;
    
    public string validation = "";
    public string validationFailedString = "";
    public TMP_FontAsset font;
    public float globalFontScale = 1;
    public bool overrideAllFonts = false;

    public RectTransform joystickKeyboardPanel;
    public List<LetterGridRow> letterGrid = new List<LetterGridRow>();
    public RectTransform letterGridContainer;
    public RectTransform letterTemplate;
    public Color defaultLetterBackground = Color.white;
    public Color selectedLetterBackground = Color.white;
    public int minTextLength = 0;
    public int maxTextLength = 11;
    public float cursorBlinkTime = 0.5f;
    public float baseFontSize = 80;
    public float selectedFontSize = 110;
    public TextMeshProUGUI keyboardCurrentText;
    public TextMeshProUGUI helpText;
    public TextMeshProUGUI title;
    //public TextMeshProUGUI inputField;
    int selectedLetterRow = -1;
    int selectedLetterColumn = -1;
    int cursorFactor = 1;
    float lastCursorBlink = 0;
    bool keyboardIsActive;
    int[] values;


    string currentText = "_";
    string lastText = "";
    bool letterGridCreated = false;
    bool delayFunctionality = false;
    string alphaTag = "<alpha=#00>";
    string defaultHelptext = "";
    string defaultTitle = "";
    Color defaultColor = Color.white;
    Action acceptAction;
    public Action cancelAction;
    /*
    public static bool BlockOtherInputs()
    {
        if (Secondary == null)
        {
            if ((instance == null || !instance.IsActive()))
            {
                return false;
            }
            return true;
        }
        else
        {
            


        }
    }
    */


    public static JoystickKeyboard GetByID(int id)
    {
        return keyboards.Find(c => c.id == id);
    }

    public static JoystickKeyboard Main
    {
        get
        {
            return GetByID(0);
        }
    }

    public static JoystickKeyboard Secondary
    {
        get
        {
            return GetByID(1);
        }
    }

    public string Title
    {
        get
        {
            if (title == null)
            {
                return "";
            }
            return title.text;
        }
        set
        {
            if (title!= null)
            title.text = value;
        }
    }

    public string HelpText
    {
        get
        {
            return helpText.text;
        }
        set
        {
            helpText.text = value;
        }
    }

    public Color BackgroundColor
    {
        get
        {
            return joystickKeyboardPanel.GetComponent<Image>().color;
        }
        set
        {
            joystickKeyboardPanel.GetComponent<Image>().color = value;
        }
    }

    public void Activate(Action defaultConfirmAction)
    {
        //Syntax for an action: () => { Debug.Log("Something"); }
        delayFunctionality = true;
        if (isModal)
        {
           // 
            ArcadeInput.SetModal(this);
        }
        if (!letterGridCreated)
        {
            if (font != null)
            {
                SetFont(font);
            }
            else
            {
                if (!ValidFonts())
                {
                    Debug.Log("Fonts not set up correctly!");
                    return;
                }
            }

            joystickKeyboardPanel.gameObject.SetActive(true);
            SetAction("OK", () => { EndModalNow(); });
            SetAction("OK", () => { EndKeyboard(); }, false);
            SetAction("OK", defaultConfirmAction, false);
            acceptAction = defaultConfirmAction;
            CreateLetterGrid();
            letterGridCreated = true;
        }
        else
        {
            joystickKeyboardPanel.gameObject.SetActive(true);
            SetAction("OK", () => { EndModalNow(); });
            SetAction("OK", () => { EndKeyboard(); }, false);
            SetAction("OK", defaultConfirmAction, false);
            acceptAction = defaultConfirmAction;
            ClearColors();
            selectedLetterRow = 1;
            selectedLetterColumn = 0;
        }

        
        //defaultTitle = tit
        keyboardIsActive = true;
        lastCursorBlink = Time.time;
        cursorFactor = 1;
        currentText = "_";
        Debug.Log("Opening Again: " + validation);
        UpdateKeyboardGraphics();
    }

    public void EndModalNow()
    {
        ArcadeInput.EndModalNow(this);
    }
    public void EndKeyboard()
    {
        if (joystickKeyboardPanel.gameObject.activeSelf)
        {
            lastText = GetText();
            helpText.text = defaultHelptext;
            defaultHelptext = helpText.text;
            joystickKeyboardPanel.GetComponent<Image>().color = defaultColor;
            if (title != null)
                title.text = defaultTitle;
            validation = "";
            ArcadeInput.EndModal(this);
            currentText = "_";
            keyboardIsActive = false;
            joystickKeyboardPanel.gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (IsActive())
        {
            EndKeyboard();
        }

        keyboards.Remove(this);
    }
    public bool IsActive()
    {
        return keyboardIsActive && !delayFunctionality;
    }

    public void SetAction(string letter, Action action, bool clearExisting = true)
    {
        for (int y = 0; y < letterGrid.Count; y++)
        {
            for (int x = 0; x < letterGrid[y].columns.Count; x++)
            {
                if (letterGrid[y].columns[x].letter == letter)
                {
                    if (clearExisting)
                    {
                        letterGrid[y].columns[x].pushAction = null;
                    }
                    letterGrid[y].columns[x].pushAction += action;
                }

            }
        }
    }

    Letter GetByInput (ArcadeInputType arcadeInputType)
    {
        for (int y = 0; y < letterGrid.Count; y++)
        {
            for (int x = 0; x < letterGrid[y].columns.Count; x++)
            {
                if (letterGrid[y].columns[x].pushByInput == arcadeInputType)
                {
                    return letterGrid[y].columns[x];
                }
            }
        }
        return null;
    }

    void PushByLetter (Letter letter)
    {
        string cursor = "_";
        int cursorAlphaDel = 0;
        if (cursorFactor == 0)
        {
            cursor = alphaTag + "_";
            cursorAlphaDel = 11;
        }
        if (currentText.Replace("_", "").Replace(alphaTag, "").Length < maxTextLength)
        {
            currentText = currentText.Substring(0, currentText.Length - 1 - cursorAlphaDel) + letter.letter + cursor;
        }
    }

    public Letter MoveDown()
    {
        if (!IsActive() || delayFunctionality)
        {
            return null;
        }
        UpdateSelectedLetter(KeyboardAction.MoveDown);
        UpdateKeyboardGraphics();
        return GetSelected();
    }

    public Letter MoveUp()
    {
        if (!IsActive() || delayFunctionality)
        {
            return null;
        }
        UpdateSelectedLetter(KeyboardAction.MoveUp);
        UpdateKeyboardGraphics();
        return GetSelected();
    }

    public Letter MoveLeft()
    {
        if (!IsActive() || delayFunctionality)
        {
            return null;
        }
        UpdateSelectedLetter(KeyboardAction.MoveLeft);
        UpdateKeyboardGraphics();
        return GetSelected();
    }

    public Letter MoveRight()
    {
        if (!IsActive() || delayFunctionality)
        {
            return null;
        }
        UpdateSelectedLetter(KeyboardAction.MoveRight);
        UpdateKeyboardGraphics();
        return GetSelected();
    }

    public Letter PushSelected()
    {
        if (!IsActive() || delayFunctionality)
        {
            return null;
        }
        UpdateSelectedLetter(KeyboardAction.PushSelected);

        if (GetSelected().letter == "OK")
        {
            if ((GetText().Length >= minTextLength && (validation == "" || GetText() == validation)) || !GetSelected().validate)
            {
                GetSelected().pushAction?.Invoke();
            }
            else
            {
                if (GetText().Length < minTextLength)
                {
                    helpText.text = "TEXT HAS TO BE MINIMUM " + minTextLength + " CHARACTERS";
                }
                else if (validation != GetText())
                {
                    helpText.text = validationFailedString;
                }
            }
        } 
        else
        {
            GetSelected().pushAction?.Invoke();
        }
        

        UpdateKeyboardGraphics();
        return GetSelected();
    }

    public string GetCurrentText()
    {
        
        return lastText.Replace("_", "").Replace(alphaTag, "");
    }

    string GetText()
    {
        if (!IsActive())
        {
            return "";
        }

        UpdateKeyboardGraphics();
        //Debug.Log("Length: " + currentText.Replace("_", "").Replace(alphaTag, "").Length);
        return currentText.Replace("_", "").Replace(alphaTag, "");
    }

    public string GetCurrentTextWithCursor()
    {
        if (!IsActive())
        {
            return "";
        }
        UpdateKeyboardGraphics();
        return currentText;
    }

    void SetFont(TMP_FontAsset font)
    {
        if (font != null)
        {
            TextMeshProUGUI[] allText = GetComponentsInChildren<TextMeshProUGUI>(true);
            Debug.Log("Setting font! " + allText.Length);
            for (int i = 0; i < allText.Length; i++)
            {
                if (allText[i].font == null || overrideAllFonts)
                {
                    allText[i].font = font;
                    allText[i].fontSize *= globalFontScale;
                }
            }
        }


    }

    private void Awake()
    {
        if (keyboards.Find(c => c.id == id) == null)
        {
            keyboards.Add(this);
        }
        else
        {
            if (replaceWithIdeniticalID)
            {
                Debug.Log("This keyboard shares id with another keyboard. Replacing!");
                keyboards.Remove(keyboards.Find(c => c.id == id));
                keyboards.Add(this);
            }
        }
        defaultColor = joystickKeyboardPanel.GetComponent<Image>().color;
        defaultHelptext = helpText.text;
        if (title != null)
        defaultTitle = title.text;


    }

    // Start is called before the first frame update
    void Start()
    {
        values = (int[])System.Enum.GetValues(typeof(KeyCode));
        joystickKeyboardPanel.gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        delayFunctionality = false;
    }

    private void Update()
    {
        if (delayFunctionality)
        {
            return;
        }
        if (keyboardIsActive)
        {
            
            if (registerInputsInternally)
            {
                
                RegisterInputs();
            }


            if (lastCursorBlink + cursorBlinkTime <= Time.time)
            {
                cursorFactor = Mathf.Abs(cursorFactor - 1);
                if (cursorFactor == 0)
                {
                    //Has gone from cursor to no cursor. Remove!
                    if (currentText.Length - 1 >= 0)
                        currentText = currentText.Substring(0, currentText.Length - 1) + alphaTag + "_";
                    //
                }
                else
                {
                    //Has gone from no cursor to cursor. Add!
                    if (currentText.Length - 12 >= 0)
                        currentText = currentText.Substring(0, currentText.Length - 12) + "_";
                }
                lastCursorBlink = Time.time;
            }
            if (hideCharacters)
            {
                string stars = "";
                string cleanText = currentText.Replace("_", "").Replace(alphaTag, "");
                for (int i = 0; i < cleanText.Length; i++)
                {
                    stars += "*";
                }

                keyboardCurrentText.text = stars;

                if (validation != "" && GetText().Length >= minTextLength && GetText().Length <= maxTextLength && GetText() != validation)
                {
                    string dots = "";
                    for (int i = 0; i < validation.Length - 2; i++)
                    {
                        dots += ".";
                    }
                    string hint = validation.Substring(0, 1) + dots + validation.Substring(validation.Length - 1, 1);
                    keyboardCurrentText.text += " <size=60%><color=#FF0000>INCORRECT!<size=70%> HINT: " + hint;
                }
            }
            else
            {
                keyboardCurrentText.text = currentText;
            }
            
        }
    }

    void RegisterInputs()
    {
        if (colorButtonsAsInput)
        {
            if (ArcadeInput.AnyColoredButtonInitiated(registerInputsFromPlayer, out ArcadeInputType result, this))
            {
                
                Letter letter = GetByInput(result);

                if (letter != null)
                {
                    //Debug.Log(letter.letter);
                    PushByLetter(letter);
                }

                if (GetText() == validation && automaticAcceptWhenValid)
                {
                    EndModalNow();
                    EndKeyboard();
                    acceptAction.Invoke();
                }

            }
            if (ArcadeInput.InputInitiated(registerInputsFromPlayer, ArcadeInputType.JoystickLeft, AxisType.Raw, this))
            {
                DeleteLastCharacter();
            }

            if (ArcadeInput.InputInitiated(registerInputsFromPlayer, ArcadeInputType.JoystickRight, AxisType.Raw, this))
            {

                if ((GetText().Length >= minTextLength && (validation == "" || GetText() == validation)))
                {
                    EndModalNow();
                    EndKeyboard();
                    acceptAction.Invoke();
                }
                else
                {
                    if (GetText().Length < minTextLength)
                    {
                        helpText.text = "TEXT HAS TO BE MINIMUM " + minTextLength + " CHARACTERS";
                        if (validation != "")
                        {
                            string dots = "";
                            for (int i = 0; i < validation.Length - 2; i++)
                            {
                                dots += ".";
                            }
                            string hint = validation.Substring(0, 1) + dots + validation.Substring(validation.Length - 1, 1);
                            helpText.text += "\r\nHINT: " + hint;
                        }
                    }
                    else if (validation != GetText())
                    {
                        helpText.text = validationFailedString;
                    }
                }
            }

            if (ArcadeInput.InputInitiated(registerInputsFromPlayer, ArcadeInputType.JoystickDown, AxisType.Raw, this))
            {
                EndModalNow();
                EndKeyboard();
                if (cancelAction != null)
                {
                    cancelAction.Invoke();
                    cancelAction = null;
                }
            }
        } 
        else
        {
            if (ArcadeInput.InputInitiated(registerInputsFromPlayer, ArcadeInputType.JoystickDown, AxisType.Raw, this))
            {
                MoveDown();
            }
            if (ArcadeInput.InputInitiated(registerInputsFromPlayer, ArcadeInputType.JoystickUp, AxisType.Raw, this))
            {
                MoveUp();
            }
            if (ArcadeInput.InputInitiated(registerInputsFromPlayer, ArcadeInputType.JoystickLeft, AxisType.Raw, this))
            {
                MoveLeft();
            }
            if (ArcadeInput.InputInitiated(registerInputsFromPlayer, ArcadeInputType.JoystickRight, AxisType.Raw, this))
            {
                MoveRight();
            }
            if (ArcadeInput.AnyButtonInitiated(registerInputsFromPlayer, this))
            {
                PushSelected();
            }
        }
        
    }
    Letter GetSelected()
    {
        return letterGrid[selectedLetterRow].columns[selectedLetterColumn];
    }

    void CreateLetterGrid()
    {
        float currentX = 0;
        float xOffset = 0;
        float currentY = 0;
        float yOffset = 0;

        RectTransform container = letterGridContainer;
        Rect containerRect = container.rect;
        xOffset = containerRect.width / letterGrid[0].columns.Count; //The width of the rect-field divided by number of columns
        yOffset = containerRect.height / letterGrid.Count;

        currentX = containerRect.x;
        currentY = containerRect.y + yOffset * 0.5f;

        for (int y = 0; y < letterGrid.Count; y++)
        {
            float summedFactors = CalculateSumFactors(letterGrid[y]);
            float factor = containerRect.width / summedFactors;

            for (int x = 0; x < letterGrid[y].columns.Count; x++)
            {
                Letter currentLetterData = letterGrid[y].columns[x];
                float currentWidth = factor * currentLetterData.scaleFactor;
                currentX += currentWidth * 0.5f;

                if (currentLetterData.selectable)
                {
                    RectTransform currentLetter = (RectTransform)Instantiate(letterTemplate.gameObject, container).transform;
                    TextMeshProUGUI currentText = currentLetter.GetComponentInChildren<TextMeshProUGUI>();
                    currentText.fontSize = baseFontSize * globalFontScale;
                    if (currentLetterData.useLabel)
                    {
                        currentText.text = currentLetterData.label;
                    }
                    else
                    {
                        currentText.text = currentLetterData.letter;
                    }

                    Image currentImage = currentLetter.GetComponent<Image>();
                    currentLetterData.textField = currentText;
                    currentLetterData.imageField = currentImage;

                    if (currentLetterData.overrideSprite != null)
                    {
                        currentLetterData.imageField.sprite = currentLetterData.overrideSprite;
                    }

                    Color defaultColor = defaultLetterBackground;
                    if (currentLetterData.overrideDefault)
                    {
                        defaultColor = currentLetterData.defaultColor;
                    }
                    currentImage.color = defaultColor;
                    currentLetter.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, currentWidth);
                    currentLetter.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, yOffset);
                    currentLetter.localPosition = new Vector3(currentX, containerRect.height - currentY - containerRect.height, 0);

                }

                currentX += currentWidth * 0.5f;
            }
            currentY += yOffset;
            currentX = containerRect.x;
        }
        selectedLetterRow = 1;
        selectedLetterColumn = 0;
    }

    void ClearColors()
    {

        for (int y = 0; y < letterGrid.Count; y++)
        {
            for (int x = 0; x < letterGrid[y].columns.Count; x++)
            {
                if (letterGrid[y].columns[x].selectable)
                {
                    Color defaultColor = defaultLetterBackground;
                    if (letterGrid[y].columns[x].overrideDefault)
                    {
                        defaultColor = letterGrid[y].columns[x].defaultColor;
                    }

                    letterGrid[y].columns[x].imageField.color = defaultColor;

                    letterGrid[y].columns[x].textField.fontSize = baseFontSize * globalFontScale;
                }


            }
        }
    }

    void UpdateKeyboardGraphics()
    {
        if (!hideSelection)
        {
            Color selectedColor = selectedLetterBackground;
            if (letterGrid[selectedLetterRow].columns[selectedLetterColumn].overrideSelected)
            {
                selectedColor = letterGrid[selectedLetterRow].columns[selectedLetterColumn].selectedColor;
            }
            letterGrid[selectedLetterRow].columns[selectedLetterColumn].imageField.color = selectedColor;
            letterGrid[selectedLetterRow].columns[selectedLetterColumn].textField.fontSize = selectedFontSize * globalFontScale;
        }
        

        //UpdateSelectedLetter(null);

        if (false && Input.anyKeyDown)
        {
            int row = -1;
            int column = -1;
            //Debug.Log("Key Down");
            for (int i = 0; i < values.Length; i++)
            {
                //Debug.Log("Testing: " + (KeyCode)values[i]);
                if (Input.GetKeyDown((KeyCode)values[i]))
                {
                    GetSelectedRowColumn((KeyCode)values[i], ref row, ref column);

                    if (row > -1 && column > -1)
                    {
                        Color defaultColor = defaultLetterBackground;
                        if (letterGrid[selectedLetterRow].columns[selectedLetterColumn].overrideDefault)
                        {
                            defaultColor = letterGrid[selectedLetterRow].columns[selectedLetterColumn].defaultColor;
                        }

                        letterGrid[selectedLetterRow].columns[selectedLetterColumn].imageField.color = defaultColor;
                        letterGrid[selectedLetterRow].columns[selectedLetterColumn].textField.fontSize = baseFontSize;
                        //Debug.Log("Trying: " + row + ", " + column);
                        selectedLetterRow = row;
                        selectedLetterColumn = column;
                    }
                }
            }



        }


    }

    void GetSelectedRowColumn(KeyCode keyCode, ref int row, ref int column)
    {
        for (int i = 0; i < letterGrid.Count; i++)
        {
            /*
            Letter letter = letterGrid[i].columns.Find(c => c.keyCode == keyCode);
            if (letter != null)
            {
                row = i;
                column = letterGrid[i].columns.IndexOf(letter);
                return;
            }
            */
        }
        row = -1;
        column = -1;
    }

    public void DeleteLastCharacter()
    {
        string cursor = "_";
        int cursorAlphaDel = 0;
        if (cursorFactor == 0)
        {
            cursor = alphaTag + "_";
            cursorAlphaDel = 11;
        }
        if (currentText.Length > 1 + cursorAlphaDel)
        {
            currentText = currentText.Substring(0, currentText.Length - 1 - 1 - cursorAlphaDel) + cursor;
        }
        //Debug.Log("Last Char del");
        //Letter currentLetter = letterGrid[selectedLetterRow].columns[selectedLetterColumn];
    }
    void UpdateSelectedLetter(KeyboardAction keyboardAction)
    {
        string cursor = "_";
        int cursorAlphaDel = 0;
        if (cursorFactor == 0)
        {
            cursor = alphaTag + "_";
            cursorAlphaDel = 11;
        }

        if (keyboardAction == KeyboardAction.PushSelected)
        {
            string current = letterGrid[selectedLetterRow].columns[selectedLetterColumn].letter;
            Letter currentLetter = letterGrid[selectedLetterRow].columns[selectedLetterColumn];
            if (current == "SPACE")
            {

                if (currentText.Replace("_", "").Replace(alphaTag, "").Length < maxTextLength)
                {
                    currentText = currentText.Substring(0, currentText.Length - 1 - cursorAlphaDel) + " " + cursor;
                }
                currentLetter.onPush.Invoke();
                return;
            }

            if (current == "DELETE")
            {
                if (currentText.Length > 1 + cursorAlphaDel)
                {
                    currentText = currentText.Substring(0, currentText.Length - 1 - 1 - cursorAlphaDel) + cursor;
                }
                //currentLetter.onPush.Invoke();
                return;
            }

            if (current == "OK")
            {
                string newName = currentText.Replace("_", "").Replace(alphaTag, "");

                if (newName != "")
                {
                    //highscores.Remove(newHighscoreData);
                    //AddHighscore(newName, newHighscoreData.score);
                    //newHighscoreData = null;
                    //highscoreHeader.text = "<size=80%><line-height=60%>SCORE BOARD\r\n<size=45%><color=#F5B909>PRESS SPACE FOR START SCREEN";
                }
                Debug.Log("Min length: " + minTextLength);
                if ((GetText().Length >= minTextLength && (validation == "" || GetText() == validation)) || !currentLetter.validate)
                {
                    Debug.Log("Closed 1");
                    currentLetter.onPush.Invoke();
                }
                else
                {
                    if (GetText().Length < minTextLength)
                    {
                        helpText.text = "TEXT HAS TO BE MINIMUM " + minTextLength + " CHARACTERS";
                    }
                    else if (validation != GetText())
                    {
                        helpText.text = validationFailedString;
                    }
                }
                
                return;
            }

            if (currentText.Replace("_", "").Replace(alphaTag, "").Length < maxTextLength)
            {
                currentText = currentText.Substring(0, currentText.Length - 1 - cursorAlphaDel) + current + cursor;
            }

            if ((GetText().Length >= minTextLength && (validation == "" || GetText() == validation)) || !currentLetter.validate)
            {
                //Debug.Log("Closed 2");
                currentLetter.onPush.Invoke();
            }
            else
            {
                if (GetText().Length < minTextLength)
                {
                    helpText.text = "TEXT HAS TO BE MINIMUM " + minTextLength + " CHARACTERS";
                }
                else if (validation != GetText())
                {
                    helpText.text = validationFailedString;
                }

            }
        }

        if (keyboardAction == KeyboardAction.MoveDown)
        {
            Color defaultColor = defaultLetterBackground;
            if (letterGrid[selectedLetterRow].columns[selectedLetterColumn].overrideDefault)
            {
                defaultColor = letterGrid[selectedLetterRow].columns[selectedLetterColumn].defaultColor;
            }

            letterGrid[selectedLetterRow].columns[selectedLetterColumn].imageField.color = defaultColor;
            letterGrid[selectedLetterRow].columns[selectedLetterColumn].textField.fontSize = baseFontSize * globalFontScale;


            RectTransform currentTransform = (RectTransform)letterGrid[selectedLetterRow].columns[selectedLetterColumn].imageField.transform;

            selectedLetterRow++;
            if (selectedLetterRow >= letterGrid.Count)
            {
                selectedLetterRow = 0;
            }

            float shortest = 10000;
            int shortestIndex = -1;

            for (int i = 0; i < letterGrid[selectedLetterRow].columns.Count; i++)
            {
                if (letterGrid[selectedLetterRow].columns[i].selectable)
                {
                    float distance = Mathf.Abs(letterGrid[selectedLetterRow].columns[i].imageField.transform.localPosition.x - currentTransform.localPosition.x);

                    if (distance < (letterGrid[selectedLetterRow].columns[i].imageField.transform as RectTransform).rect.width * .5f)
                    {
                        distance = 0;
                    }

                    if (distance < shortest)
                    {
                        shortest = distance;
                        shortestIndex = i;
                    }
                }

            }
            selectedLetterColumn = shortestIndex;
        }


        if (keyboardAction == KeyboardAction.MoveUp)
        {
            Color defaultColor = defaultLetterBackground;
            if (letterGrid[selectedLetterRow].columns[selectedLetterColumn].overrideDefault)
            {
                defaultColor = letterGrid[selectedLetterRow].columns[selectedLetterColumn].defaultColor;
            }
            letterGrid[selectedLetterRow].columns[selectedLetterColumn].imageField.color = defaultColor;
            letterGrid[selectedLetterRow].columns[selectedLetterColumn].textField.fontSize = baseFontSize * globalFontScale;

            RectTransform currentTransform = (RectTransform)letterGrid[selectedLetterRow].columns[selectedLetterColumn].imageField.transform;

            selectedLetterRow--;
            if (selectedLetterRow < 0)
            {
                selectedLetterRow = letterGrid.Count - 1;
            }

            float shortest = 10000;
            int shortestIndex = -1;

            for (int i = 0; i < letterGrid[selectedLetterRow].columns.Count; i++)
            {
                if (letterGrid[selectedLetterRow].columns[i].selectable)
                {
                    float distance = Mathf.Abs(letterGrid[selectedLetterRow].columns[i].imageField.transform.localPosition.x - currentTransform.localPosition.x);

                    if (distance < (letterGrid[selectedLetterRow].columns[i].imageField.transform as RectTransform).rect.width * .5f)
                    {
                        distance = 0;
                    }

                    if (distance < shortest)
                    {
                        shortest = distance;
                        shortestIndex = i;
                    }
                }
            }
            selectedLetterColumn = shortestIndex;
        }

        if (keyboardAction == KeyboardAction.MoveLeft)
        {
            Color defaultColor = defaultLetterBackground;
            if (letterGrid[selectedLetterRow].columns[selectedLetterColumn].overrideDefault)
            {
                defaultColor = letterGrid[selectedLetterRow].columns[selectedLetterColumn].defaultColor;
            }
            letterGrid[selectedLetterRow].columns[selectedLetterColumn].imageField.color = defaultColor;
            letterGrid[selectedLetterRow].columns[selectedLetterColumn].textField.fontSize = baseFontSize * globalFontScale;

            RectTransform currentTransform = (RectTransform)letterGrid[selectedLetterRow].columns[selectedLetterColumn].imageField.transform;

            do
            {
                selectedLetterColumn--;


                if (selectedLetterColumn < 0)
                {
                    selectedLetterColumn = letterGrid[selectedLetterRow].columns.Count - 1;
                }
            } while (!letterGrid[selectedLetterRow].columns[selectedLetterColumn].selectable);
        }

        if (keyboardAction == KeyboardAction.MoveRight)
        {
            Color defaultColor = defaultLetterBackground;
            if (letterGrid[selectedLetterRow].columns[selectedLetterColumn].overrideDefault)
            {
                defaultColor = letterGrid[selectedLetterRow].columns[selectedLetterColumn].defaultColor;
            }
            letterGrid[selectedLetterRow].columns[selectedLetterColumn].imageField.color = defaultColor;
            letterGrid[selectedLetterRow].columns[selectedLetterColumn].textField.fontSize = baseFontSize * globalFontScale;

            RectTransform currentTransform = (RectTransform)letterGrid[selectedLetterRow].columns[selectedLetterColumn].imageField.transform;

            do
            {
                selectedLetterColumn++;
                if (selectedLetterColumn >= letterGrid[selectedLetterRow].columns.Count)
                {
                    selectedLetterColumn = 0;
                }
            } while (!letterGrid[selectedLetterRow].columns[selectedLetterColumn].selectable);
        }
    }


    float CalculateSumFactors(LetterGridRow letterGridRow)
    {
        float result = 0;
        for (int i = 0; i < letterGridRow.columns.Count; i++)
        {
            result += letterGridRow.columns[i].scaleFactor;
        }
        return result;
    }


    private void OpenKeyboard()
    {
        CreateLetterGrid();
        letterGridContainer.gameObject.SetActive(true);
        keyboardIsActive = true;
    }

    bool ValidFonts()
    {
        TextMeshProUGUI[] allText = GetComponentsInChildren<TextMeshProUGUI>(true);

        for (int i = 0; i < allText.Length; i++)
        {
            if (allText[i].font == null)
            {
                return false;
            }
        }
        return true;
    }
}
