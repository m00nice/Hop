using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public enum UIAction
{
    MakeVisible = 0,
    TurnOnInteractable = 1,
}

namespace GameBaseSystem
{
    public class UIController : MonoBehaviour
    {
        //Which gamestates should this be active on...
        public List<GameState> gameStates;
        public List<UIAction> uiActions;
        // Start is called before the first frame update
        void Start()
        {
            GameController.instance.SubscribeUIController(this);
            if (uiActions.Count == 0)
            {
                uiActions.Add(UIAction.MakeVisible);
            }
        }

        public void UpdateUI(GameState currentGameState)
        {
            if (gameStates.Contains(currentGameState))
            {
                // Turn on UI
                for (int i = 0; i < uiActions.Count; i++)
                {
                    if (uiActions[i] == UIAction.MakeVisible)
                    {
                        gameObject.SetActive(true);
                    }
                    else if (uiActions[i] == UIAction.TurnOnInteractable)
                    {
                        Selectable[] selectables = GetComponentsInChildren<Selectable>(true);
                        for (int j = 0; j < selectables.Length; j++)
                        {
                            selectables[j].interactable = true;
                        }
                    }
                }
            }
            else
            {
                // Turn off UI
                for (int i = 0; i < uiActions.Count; i++)
                {
                    if (uiActions[i] == UIAction.MakeVisible)
                    {
                        gameObject.SetActive(false);
                    }
                    else if (uiActions[i] == UIAction.TurnOnInteractable)
                    {
                        Selectable[] selectables = GetComponentsInChildren<Selectable>(true);
                        for (int j = 0; j < selectables.Length; j++)
                        {
                            selectables[j].interactable = false;
                        }
                    }
                }
            }
        }

        public void OnDestroy()
        {
            GameController.instance.UnsubscribeUIController(this);
        }

    }
}

