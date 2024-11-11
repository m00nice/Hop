using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    [SerializeField] private List<GameObject> platformGroups;
    [SerializeField] private GameObject[] randomStages;

    private void Start()
    {
        GeneratePlatformGroups();
    }


    private void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            StartCoroutine(ExitGame());
        }
    }


    void GeneratePlatformGroups()
    {
        foreach (GameObject stage in randomStages)
        {
            int rando = Random.Range(0, platformGroups.Count);
            Instantiate(platformGroups[rando], stage.transform, false);
            platformGroups.Remove(platformGroups[rando]);
        }
    }


    private IEnumerator ExitGame()
    {
        yield return new WaitForSeconds(3f);
        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

}
