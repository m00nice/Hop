using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameBaseSystem
{
    public enum GameStateChange
    {
        None = 0,
        OnLoad = 1,
        OnActivation = 2,
        OnLoadAndActivation = 3,
    }

    public class SceneController : MonoBehaviour
    {
        public GameStateChange gameStateChangeType = GameStateChange.OnLoad;
        public GameState gameState;
        public List<Camera> sceneCameras;
        public bool protectFromUnload;
        public bool turnOffCamerasWhenInactive;
        public UnityEvent onSceneLoad;
        public UnityEvent onSceneUnload;

        private Selectable currentSelectable;
        private Scene scene;
        private bool hasSubscribed = false;
        public Scene Scene
        {
            get
            {
                return scene;
            }
        }

        private void LateUpdate()
        {
            if (!hasSubscribed)
            {
                GameController.instance.SubscribeScene(this);
                hasSubscribed = true;
            }
            if (SceneManager.GetActiveScene().name == gameObject.scene.name)
            {
                if (EventSystem.current.currentSelectedGameObject != null && EventSystem.current.currentSelectedGameObject.scene.name == gameObject.scene.name)
                {
                    currentSelectable = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>();

                }
                else if (EventSystem.current.currentSelectedGameObject != null && EventSystem.current.currentSelectedGameObject.scene.name != gameObject.scene.name && currentSelectable != null)
                {
                    //Debug.Log("Setting from script: ", currentSelectable.gameObject);
                    EventSystem.current.SetSelectedGameObject(currentSelectable.gameObject);
                }
            }
        }

        // Start is called before the first frame update
        protected virtual void Start()
        {
            scene = gameObject.scene;
        }

        protected virtual void OnDestroy()
        {
            GameController.instance.UnsubscribeScene(this);
        }

        public void Quit()
        {
            GameController.instance.Quit();
        }
    }
}

