using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
/// <summary>
/// Class that create the Player Specific Button
/// </summary>
public static class PlayerSpecificButtonCreator
{


    /// <summary>
    /// Create a new Voice on Menu to Create Animated Button
    /// </summary>
    [MenuItem("GameObject/UI/PlayerSpecific/Button - TextMeshPro")]
    static void CreatePlayerSpecificButton()
    {
        // Create new Object then assign it's name
        var o = new GameObject("Button (PlayerSpecific)");
        if (Selection.activeGameObject == null)
        {
            // Get the Canvas Reference
            Canvas canvas = GameObject.FindObjectOfType<Canvas>();

            // if Canvas Exist
            if (canvas != null)
            {
                // Set AButton Object as child of it
                o.transform.SetParent(canvas.transform);
                o.transform.localPosition = Vector3.zero;
            }
        }
        // else set the button as parent of it
        else
        {
            o.transform.SetParent(Selection.activeGameObject.transform);
        }

        // Add Button and Image Script to the object
        o.AddComponent<Image>();
        o.AddComponent<PlayerSpecificButton>();
        Image buttonImage = o.GetComponent<Image>();
        buttonImage.sprite = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        buttonImage.type = Image.Type.Sliced;

        RectTransform buttonRect = o.GetComponent<RectTransform>();
        buttonRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 160);
        buttonRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 30);

        // Get button component attached to AButton Object
        PlayerSpecificButton button = o.GetComponent<PlayerSpecificButton>();
        Navigation buttonNavigation = new Navigation();
        buttonNavigation.mode = Navigation.Mode.Explicit;
        button.navigation = buttonNavigation;

        // Create a new Text Object
        var child = new GameObject("Text (TMP)");

        // Add Text script to the object then set as parent of AButton Object
        child.AddComponent<TextMeshProUGUI>();
        child.transform.SetParent(o.transform);
        RectTransform textRect = child.GetComponent<RectTransform>();

        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.pivot = Vector2.one * .5f;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        // set it's position to zero
        child.transform.localPosition = Vector3.zero;

        // Get Text Component Attached to Child
        TextMeshProUGUI text = child.GetComponent<TextMeshProUGUI>();


        // if text exist
        if (text != null)
        {
            text.fontSize = 24;
            // Switch It's Color to Black
            text.color = new Color(0.196f, 0.196f, 0.196f, 1);

            //set the text alignment to the middlecenter
            text.alignment = TextAlignmentOptions.Center;

            // Standard Text
            text.text = "Button";
        }


        // if no object is active on the editor go to create one


        // register root object for undo
        Undo.RegisterCreatedObjectUndo(o, "Create Button (Player Specific)");

        // Set the activeobject in the editor the AButton Object
        Selection.activeGameObject = o;

    }
}
#endif


public class PlayerSpecificButton : Button
{
    public EventSystem eventSystem;
    public Action onSelectAction;
    //JoystickKeyboard.instance.Activate(() => { KeyboardFinished(); });
    //public static PlayerSpecificButton currentNavigation
    PlayerSpecificEventSystem FindEventSystem (Transform current)
    {
        if (GetComponent<PlayerDefine>() != null)
        {
            int player = GetComponent<PlayerDefine>().playerID;
            PlayerSpecificEventSystem[] systems = FindObjectsOfType<PlayerSpecificEventSystem>();
            for (int i = 0; i < systems.Length; i++)
            {
                if (systems[i].player == player)
                {
                    return systems[i];
                }
            }
        }
        

        PlayerSpecificEventSystem currentEventSystem = current.gameObject.GetComponent<PlayerSpecificEventSystem>();
        if (currentEventSystem != null)
        {
            return currentEventSystem;
        }
        if (current.parent != null)
        {
            return FindEventSystem(current.parent);
        }
        return null;
    }

    
    protected override void Awake()
    {
        base.Awake();
        eventSystem = FindEventSystem(transform);
        
        if (eventSystem == null)
        {
            Debug.LogError("This player spcific UI element don't have an event system!\r\nMake sure it is a child of an object with an event system!\r\nTrying to obtain a default event system from scene...");
        }
        else
        {

        }
    }
    public override void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;
        // Selection tracking
        if (IsInteractable() && navigation.mode != Navigation.Mode.None)
            eventSystem.SetSelectedGameObject(gameObject, eventData);
        base.OnPointerDown(eventData);
    }
    public override void Select()
    {
        if (eventSystem.alreadySelecting)
            return;
        eventSystem.SetSelectedGameObject(gameObject);
    }

    public override Selectable FindSelectableOnLeft()
    {
        Selectable result;
        if (this.navigation.mode == Navigation.Mode.Explicit)
        {
            result = this.navigation.selectOnLeft;
        }
        else if ((this.navigation.mode & Navigation.Mode.Horizontal) != Navigation.Mode.None)
        {
            result = this.FindSelectable(base.transform.rotation * Vector3.left);
            if (result != null && (result.GetType() != typeof(PlayerSpecificButton) || (result as PlayerSpecificButton).eventSystem != eventSystem))
            {
                if (result.GetType() == typeof(PlayerSpecificButton))
                {
                    return (result as PlayerSpecificButton).SendRequestLeft(this);
                }
                else
                {
                    result = null;
                }
            }
        }
        else
        {
            result = null;
        }
        return result;
    }

    private Selectable SendRequestLeft(PlayerSpecificButton requester)
    {
        Selectable result;
        if (this.navigation.mode == Navigation.Mode.Explicit)
        {
            result = this.navigation.selectOnLeft;
        }
        else if ((this.navigation.mode & Navigation.Mode.Horizontal) != Navigation.Mode.None)
        {
            result = this.FindSelectable(base.transform.rotation * Vector3.left);
            if (result != null && (result.GetType() != typeof(PlayerSpecificButton) || (result as PlayerSpecificButton).eventSystem != requester.eventSystem))
            {

                if (result.GetType() == typeof(PlayerSpecificButton))
                {
                    return (result as PlayerSpecificButton).SendRequestLeft(requester);
                }
                else
                {
                    result = null;
                }
            }
        }
        else
        {
            result = null;
        }
        return result;
    }

    public override Selectable FindSelectableOnRight()
    {
        Selectable result;
        //Debug.Log("Searching Right", this.gameObject);
        if (this.navigation.mode == Navigation.Mode.Explicit)
        {
            result = this.navigation.selectOnRight;
        }
        else if ((this.navigation.mode & Navigation.Mode.Horizontal) != Navigation.Mode.None)
        {
            result = this.FindSelectable(base.transform.rotation * Vector3.right);
            if (result != null && (result.GetType() != typeof(PlayerSpecificButton) || (result as PlayerSpecificButton).eventSystem != eventSystem))
            {
                //Debug.Log("Found one But not right!", result.gameObject);
                if (result.GetType() == typeof(PlayerSpecificButton))
                {
                    return (result as PlayerSpecificButton).SendRequestRight(this);
                }
                else
                {
                    result = null;
                }
            }
        }
        else
        {
            result = null;
        }
        return result;
    }

    private Selectable SendRequestRight(PlayerSpecificButton requester)
    {
        Selectable result;
        //Debug.Log("Searching Right", this.gameObject);
        if (this.navigation.mode == Navigation.Mode.Explicit)
        {
            result = this.navigation.selectOnRight;
        }
        else if ((this.navigation.mode & Navigation.Mode.Horizontal) != Navigation.Mode.None)
        {
            result = this.FindSelectable(base.transform.rotation * Vector3.right);
            if (result != null && (result.GetType() != typeof(PlayerSpecificButton) || (result as PlayerSpecificButton).eventSystem != requester.eventSystem))
            {
                //Debug.Log("Found one But not right!", result.gameObject);
                if (result.GetType() == typeof(PlayerSpecificButton))
                {
                    return (result as PlayerSpecificButton).SendRequestRight(requester);
                }
                else
                {
                    result = null;
                }
            }
        }
        else
        {
            result = null;
        }
        return result;
    }

    public override Selectable FindSelectableOnUp()
    {
        Selectable result;
        if (this.navigation.mode == Navigation.Mode.Explicit)
        {
            result = this.navigation.selectOnUp;
        }
        else if ((this.navigation.mode & Navigation.Mode.Vertical) != Navigation.Mode.None)
        {
            result = this.FindSelectable(base.transform.rotation * Vector3.up);
            if (result != null && (result.GetType() != typeof(PlayerSpecificButton) || (result as PlayerSpecificButton).eventSystem != eventSystem))
            {
                if (result.GetType() == typeof(PlayerSpecificButton))
                {
                    return (result as PlayerSpecificButton).SendRequestUp(this);
                }
                else
                {
                    result = null;
                }
            }
        }
        else
        {
            result = null;
        }
        return result;
    }

    private Selectable SendRequestUp(PlayerSpecificButton requester)
    {
        Selectable result;
        if (this.navigation.mode == Navigation.Mode.Explicit)
        {
            result = this.navigation.selectOnUp;
        }
        else if ((this.navigation.mode & Navigation.Mode.Vertical) != Navigation.Mode.None)
        {
            result = this.FindSelectable(base.transform.rotation * Vector3.up);
            if (result != null && (result.GetType() != typeof(PlayerSpecificButton) || (result as PlayerSpecificButton).eventSystem != requester.eventSystem))
            {
                if (result.GetType() == typeof(PlayerSpecificButton))
                {
                    return (result as PlayerSpecificButton).SendRequestUp(requester);
                }
                else
                {
                    result = null;
                }
            }
        }
        else
        {
            result = null;
        }
        return result;
    }

    public override Selectable FindSelectableOnDown()
    {
        Selectable result;
        if (this.navigation.mode == Navigation.Mode.Explicit)
        {
            result = this.navigation.selectOnDown;
        }
        else if ((this.navigation.mode & Navigation.Mode.Vertical) != Navigation.Mode.None)
        {
            result = this.FindSelectable(base.transform.rotation * Vector3.down);
            if (result != null && (result.GetType() != typeof(PlayerSpecificButton) || (result as PlayerSpecificButton).eventSystem != eventSystem))
            {
                if (result.GetType() == typeof(PlayerSpecificButton))
                {
                    return (result as PlayerSpecificButton).SendRequestDown(this);
                }
                else
                {
                    result = null;
                }
            }
        }
        else
        {
            result = null;
        }
        return result;
    }

    private Selectable SendRequestDown(PlayerSpecificButton requester)
    {
        Selectable result;
        if (this.navigation.mode == Navigation.Mode.Explicit)
        {
            result = this.navigation.selectOnDown;
        }
        else if ((this.navigation.mode & Navigation.Mode.Vertical) != Navigation.Mode.None)
        {
            result = this.FindSelectable(base.transform.rotation * Vector3.down);
            if (result != null && (result.GetType() != typeof(PlayerSpecificButton) || (result as PlayerSpecificButton).eventSystem != requester.eventSystem))
            {
                if (result.GetType() == typeof(PlayerSpecificButton))
                {
                    return (result as PlayerSpecificButton).SendRequestDown(requester);
                }
                else
                {
                    result = null;
                }
            }
        }
        else
        {
            result = null;
        }
        return result;
    }

    public override void OnSelect(BaseEventData eventData)
    {
        base.OnSelect(eventData);
        onSelectAction?.Invoke();
    }
}