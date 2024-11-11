using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager2 : MonoBehaviour
{

    public GameObject text;
    public float timeTillRestartable = 5;


    void Start()
    {
        text.SetActive(false);
        timeTillRestartable = 5;
    }


    void Update()
    {
        timeTillRestartable -= Time.deltaTime;

        if(timeTillRestartable < 0)
        {
            text.SetActive(true);

            if (Input.anyKey)
            {
                SceneManager.LoadScene("Menu", LoadSceneMode.Single);
            }
        }
    }
}
