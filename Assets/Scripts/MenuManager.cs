using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{

    [SerializeField] private GameObject selector;
    [SerializeField] private Button startButton;
    [SerializeField] private Button exitButton;

    private bool startSelected = true;


    void Start()
    {
        startButton.onClick.AddListener(() => {StartPressed();});
        exitButton.onClick.AddListener(() => {ExitPressed();});
    }


    void Update()
    {
        if (Input.GetButtonDown("Vertical"))
        {
            startSelected = !startSelected;
        }
                
        if(startSelected)
        {
            selector.transform.SetParent(startButton.transform, false);
        }
        else
        {
            selector.transform.SetParent(exitButton.transform, false);
        }

        if (Input.GetButtonDown("Jump"))
        {
            if (startSelected) 
            {
                StartPressed();
            }
            else
            {
                ExitPressed();
            }
        }
    }


    private void StartPressed()
    {
        SceneManager.LoadScene("GamePlayScene", LoadSceneMode.Single);
    }

    private void ExitPressed()
    {
        Application.Quit();
    }

}
