using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.Events;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GameBaseSystem
{
    public enum GameState
    {
        Start = 0,
        Play = 1,
        End = 2,
        Pause = 3,
        NextLevel = 4,
        Settings = 5,
        Menu = 6,
        PlayMenu = 7,
        Misc0 = 8,
        Misc1 = 9,
        Misc2 = 10,

        Undefined = 11,
    }

    public class GameController : MonoBehaviour
    {
        public static GameController instance;

        public SceneActionComponent gameStartAction;
        public List<SceneActionComponent> levelLoaders;
        public UnityEvent onGameStateStart;
        public UnityEvent onGameStatePlay;
        public UnityEvent onGameStatePause;
        public UnityEvent onGameStateEnd;
        public UnityEvent onGameStateNextLevel;
        public UnityEvent onGameStateSettings;

        List<UIController> uiControllers = new List<UIController>();
        List<PlayerController> players = new List<PlayerController>();
        List<SceneController> loadedScenes = new List<SceneController>();

        private GameState currentGameState = GameState.Undefined;

        public GameState Current
        {
            get
            {
                return currentGameState;
            }
            set
            {
                ChangeGameState(value);
            }
        }

        public List<PlayerController> Players
        {
            get
            {
                return players;
            }
        }

        public List<SceneController> LoadedScenes
        {
            get
            {
                return loadedScenes;
            }
        }

        public void Awake()
        {
            if (instance != null)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            Object.DontDestroyOnLoad(gameObject);
        }

        public void SubscribePlayer (PlayerController player)
        {
            players.Add(player);
        }

        public void UnsubscribePlayer(PlayerController player)
        {
            players.Remove(player);
        }

        public void SubscribeScene(SceneController sceneController)
        {
            loadedScenes.Add(sceneController);
            if (sceneController.gameStateChangeType == GameStateChange.OnLoad || sceneController.gameStateChangeType == GameStateChange.OnLoadAndActivation)
            {
                Current = sceneController.gameState;
            }
        }

        public void UnsubscribeScene(SceneController sceneController)
        {
            loadedScenes.Remove(sceneController);
        }

        public void SceneBecameActivated (SceneController sceneController)
        {
            if (sceneController.gameStateChangeType == GameStateChange.OnActivation || sceneController.gameStateChangeType == GameStateChange.OnLoadAndActivation)
            {
                Current = sceneController.gameState;
            }
        }

        public void InactivateInactiveSceneCameras ()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            for (int i = 0; i < loadedScenes.Count; i++)
            {
                if (loadedScenes[i].turnOffCamerasWhenInactive && activeScene.name != loadedScenes[i].Scene.name)
                {
                    for (int j = 0; j < loadedScenes[i].sceneCameras.Count; j++)
                    {
                        loadedScenes[i].sceneCameras[j].gameObject.SetActive(false);
                    }
                }
            }
        }

        public void ActivateActiveSceneCameras()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            for (int i = 0; i < loadedScenes.Count; i++)
            {
                if (activeScene.name == loadedScenes[i].Scene.name)
                {
                    for (int j = 0; j < loadedScenes[i].sceneCameras.Count; j++)
                    {
                        loadedScenes[i].sceneCameras[j].gameObject.SetActive(true);
                    }
                }
            }
        }

        public void SubscribeUIController(UIController uiController)
        {
            uiControllers.Add(uiController);
        }

        public void UnsubscribeUIController(UIController uiController)
        {
            uiControllers.Remove(uiController);
        }

        private void UpdateUI()
        {
            for (int i = 0; i < uiControllers.Count; i++)
            {
                uiControllers[i].UpdateUI(currentGameState);
            }
        }
        public static void EndGame()
        {
            instance.ChangeGameState(GameState.End);
        }

        public static void ResetGame()
        {
            instance.ChangeGameState(GameState.Start);
            
        }

        public void Start()
        {
            if (gameStartAction != null)
            {
                gameStartAction.Activate();
            }

        }
        public void ChangeGameState(GameState gameState)
        {
            if (currentGameState != gameState)
            {
                currentGameState = gameState;
                if (gameState == GameState.Start)
                {
                    onGameStateStart?.Invoke();
                }
                else if (gameState == GameState.Play)
                {
                    onGameStatePlay?.Invoke();
                }
                else if (gameState == GameState.Pause)
                {
                    onGameStatePause?.Invoke();
                }
                else if (gameState == GameState.End)
                {
                    onGameStateEnd?.Invoke();
                }
                else if (gameState == GameState.NextLevel)
                {
                    onGameStateNextLevel?.Invoke();
                }
                else if (gameState == GameState.Settings)
                {
                    onGameStateNextLevel?.Invoke();
                }
                UpdateUI();
            }
        }

        public void Quit()
        {
            if (!Application.isEditor)
            {
                Application.Quit();
            }
            else
            {
#if UNITY_EDITOR
                EditorApplication.isPlaying = false;
#endif
            }
        }
    }
}

 