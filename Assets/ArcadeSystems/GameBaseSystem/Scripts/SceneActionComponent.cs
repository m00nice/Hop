using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace GameBaseSystem
{
    public class SceneActionComponent : MonoBehaviour
    {
        public SceneAction sceneAction;
        
        private Scene current;
        private Scene loaded;
        public Scene Current 
        {
            get
            {
                return current;
            }
        }

        public Scene Loaded
        {
            get
            {
                return loaded;
            }
            set
            {
                loaded = value;
            }
        }

        void Start()
        {
            current = gameObject.scene;
        }
        public void Activate()
        {
            StartCoroutine(ActivateSceneAction());
        }

        public void ActivatePrevious()
        {
            StartCoroutine(ActivateSceneAction());
        }

        public void UnloadCurrent()
        {

            StartCoroutine(ActivateSceneAction(true));
        }

        public void ReloadScene ()
        {
            /*
            SceneManager.sceneUnloaded += OnSceneUnloaded;

            void OnSceneUnloaded(Scene scene)
            {
                if (pendingSceneReload == scene.name)
                    SceneManager.LoadScene(pendingSceneReload, LoadSceneMode.Additive);
            }
            */
        }

        IEnumerator ActivateSceneAction(bool unloadCurrent = false)
        {
            float activationTime = Time.time;

            while (activationTime + sceneAction.delay > Time.time)
            {
                yield return null;
            }

            sceneAction.activationTime = Time.time;
            sceneAction.progressTime = Time.time;

            if (sceneAction.sceneActionType == SceneActionType.Load && !unloadCurrent)
            {
                SceneActionManager.instance.LoadScene(this);
            }
            else if (sceneAction.sceneActionType == SceneActionType.Unload || unloadCurrent)
            {
                SceneActionManager.instance.UnloadScene(this, unloadCurrent);
            }
            else if (sceneAction.sceneActionType == SceneActionType.Reload)
            {
                SceneActionManager.instance.UnloadScene(this);
            }
        }

    }
}

