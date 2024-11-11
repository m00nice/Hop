using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.Events;


namespace GameBaseSystem
{
    public enum SceneActionType
    {
        Load = 0,
        Unload = 1,
        Reload = 2,
    }

    [System.Serializable]
    public struct SceneActionEventParameter
    {
        public SceneAction sceneAction;
        public SceneActionType sceneActionType;
        public float delay;



        public SceneActionEventParameter(SceneAction sceneAction, SceneActionType sceneActionType, float delay)
        {
            this.sceneAction = sceneAction;
            this.sceneActionType = sceneActionType;
            this.delay = delay;
        }

        public static SceneActionEventParameter New(SceneAction sceneAction)
        {
            return new SceneActionEventParameter(sceneAction, SceneActionType.Load, 0);
        }

        public static SceneActionEventParameter New(SceneAction sceneAction, float delay)
        {
            return new SceneActionEventParameter(sceneAction, SceneActionType.Load, delay);
        }

        public static SceneActionEventParameter New(SceneAction sceneAction, SceneActionType sceneActionType, float delay)
        {
            return new SceneActionEventParameter(sceneAction, sceneActionType, delay);
        }

        public Scene scene
        {
            get
            {
                return sceneAction.sceneField.Scene;
            }
        }
    }

    [System.Serializable]
    public class SceneActionEvent : UnityEvent<SceneActionType>
    {


    }


    [System.Serializable]
    public class SceneAction
    {
        public SceneField sceneField;
        public SceneActionType sceneActionType;
        public float delay;
        public float activationTime;
        public float progressTime;
        public bool activate;
        public bool activatePrevious;
        public UnityEvent onActivate;
        public UnityEvent onProgress;
        public UnityEvent onFinished;
    }


    public class SceneActionManager : MonoBehaviour
    {
        public static SceneActionManager instance;
        private Scene currentActive;
        private Scene previousActive;
        private SceneActionComponent pendingSceneReload = null;

        private List<SceneActionComponent> pendingSceneLoads = new List<SceneActionComponent>();
        private List<SceneActionComponent> pendingSceneUnloads = new List<SceneActionComponent>();
        // Start is called before the first frame update
        void Awake()
        {
            instance = this;
            previousActive = gameObject.scene;
            currentActive = gameObject.scene;
            SceneManager.sceneLoaded += OnSceneLoaded;

        }
     
        public void LoadScene(SceneActionComponent sceneActionComponent)
        {
            //Debug.Log("Loading: " + sceneActionComponent.sceneAction.sceneField.SceneName);
            StartCoroutine(LoadSceneAsync(sceneActionComponent));
        }

        public void UnloadScene(SceneActionComponent sceneActionComponent, bool unloadCurrent = false)
        {
            Scene scene = sceneActionComponent.sceneAction.sceneField.Scene;
            if (unloadCurrent)
            {
                scene = sceneActionComponent.Current;
            }

            if (GameController.instance.LoadedScenes.Find(c => (c.Scene == scene || c.Scene.name == scene.name) && c.protectFromUnload) != null)
            {
                return;
            }

            StartCoroutine(UnloadSceneAsync(sceneActionComponent, unloadCurrent));
        }

        public void ReloadScene(SceneActionComponent sceneActionComponent)
        {
            //Debug.Log("Loading: " + sceneActionComponent.sceneAction.sceneField.SceneName);
            StartCoroutine(UnloadSceneAsync(sceneActionComponent));
        }
        public void ActivateScene(SceneActionComponent sceneActionComponent, bool previous = false)
        {
            //Debug.Log("Activate Scene: " + sceneActionComponent.sceneAction.sceneField.ScenePath);
            //Debug.Log("Activate Scene (Name): " + sceneActionComponent.sceneAction.sceneField.SceneName);
            Scene next = gameObject.scene;
            //Debug.Log("Is valid: " + sceneActionComponent.sceneAction.sceneField.Scene.IsValid());
            if (previous)
            {
                next = SceneManager.GetSceneByName(previousActive.name);
            } 
            else
            {
                next = SceneManager.GetSceneByName(sceneActionComponent.sceneAction.sceneField.SceneName);
                previousActive = currentActive;
            }
                
            currentActive = next;
            
            SceneManager.SetActiveScene(currentActive);

            

            GameController.instance.InactivateInactiveSceneCameras();
            GameController.instance.ActivateActiveSceneCameras();

            Scene currentlyActive = SceneManager.GetActiveScene();
            GameObject[] roots = currentlyActive.GetRootGameObjects();

            SceneController sc = null;

            for (int i = 0; i < roots.Length; i++)
            {
                sc = roots[i].GetComponentInChildren<SceneController>();
                if (sc != null)
                {
                    GameController.instance.SceneBecameActivated(sc);
                    break;
                }
            }
        }

        IEnumerator LoadSceneAsync(SceneActionComponent sceneActionComponent)
        {
            pendingSceneLoads.Add(sceneActionComponent);
            
            sceneActionComponent.sceneAction.progressTime = Time.time;
            AsyncOperation sceneLoad = SceneManager.LoadSceneAsync(sceneActionComponent.sceneAction.sceneField.ScenePath, LoadSceneMode.Additive);
            sceneActionComponent.sceneAction.onActivate?.Invoke();

            while (!sceneLoad.isDone)
            {
                sceneActionComponent.sceneAction.progressTime = Time.time;
                sceneActionComponent.sceneAction.onProgress?.Invoke();
                yield return null;
            }
        }

        IEnumerator UnloadSceneAsync(SceneActionComponent sceneActionComponent, bool unloadCurrent = false)
        {
            sceneActionComponent.sceneAction.progressTime = Time.time;
            SceneController sc = GameController.instance.LoadedScenes.Find(c => c.Scene.name == sceneActionComponent.sceneAction.sceneField.SceneName);
            Scene scene = gameObject.scene;
            if (sc == null && !unloadCurrent)
            {
                yield break;
            }
            else if (sc != null && !unloadCurrent)
            {
                scene = sc.Scene;
            }
            
            if (unloadCurrent)
            {
                scene = sceneActionComponent.Current;
            }

            bool activatePrevious = sceneActionComponent.sceneAction.activatePrevious;
            string actionSceneName = sceneActionComponent.gameObject.scene.name;
            string sceneUnloadName = sceneActionComponent.sceneAction.sceneField.SceneName;

            if (sceneActionComponent.sceneAction.sceneActionType == SceneActionType.Reload)
            {
                DontDestroyOnLoad(sceneActionComponent.gameObject);
                pendingSceneReload = sceneActionComponent;
                SceneManager.sceneUnloaded += OnSceneUnloaded;
            }

            AsyncOperation sceneUnload = SceneManager.UnloadSceneAsync(scene.path, UnloadSceneOptions.None);
            if (!unloadCurrent)
                sceneActionComponent.sceneAction.onActivate?.Invoke();

            while (!sceneUnload.isDone)
            {
                if (actionSceneName != sceneUnloadName)
                {
                    sceneActionComponent.sceneAction.progressTime = Time.time;
                    if (!unloadCurrent)
                        sceneActionComponent.sceneAction.onProgress?.Invoke();
                }
                yield return null;
            }

            if (actionSceneName != sceneUnloadName)
            {
                sceneActionComponent.sceneAction.progressTime = Time.time;
                if (!unloadCurrent)
                    sceneActionComponent.sceneAction.onFinished?.Invoke();

                
            }

            if (sceneActionComponent.sceneAction.activatePrevious)
            {
                ActivateScene(sceneActionComponent, true);
            }



        }

        void OnSceneUnloaded(Scene scene)
        {
            if (pendingSceneReload.sceneAction.sceneField.SceneName == scene.name)
            {
                SceneManager.sceneUnloaded -= OnSceneUnloaded;
                LoadScene(pendingSceneReload);
                //SceneManager.LoadScene(pendingSceneReload, LoadSceneMode.Additive);
                
            }
                
        }

        void OnSceneLoaded (Scene scene, LoadSceneMode loadSceneMode)
        {
            SceneActionComponent sceneActionComponent = pendingSceneLoads.Find(c => c.sceneAction.sceneField.SceneName == scene.name);
            if (sceneActionComponent != null)
            {
                pendingSceneLoads.Remove(sceneActionComponent);
                sceneActionComponent.sceneAction.progressTime = Time.time;
                sceneActionComponent.sceneAction.onFinished?.Invoke();
                if (sceneActionComponent.sceneAction.activate)
                {
                    ActivateScene(sceneActionComponent);
                }
                if (pendingSceneReload != null)
                {
                    Destroy(pendingSceneReload.gameObject);
                    pendingSceneReload = null;
                }
            }
            
        }
    }
}

