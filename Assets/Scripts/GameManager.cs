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


    void GeneratePlatformGroups()
    {
        foreach (GameObject stage in randomStages)
        {
            int rando = Random.Range(0, platformGroups.Count);
            Instantiate(platformGroups[rando], stage.transform, false);
            platformGroups.Remove(platformGroups[rando]);
        }
    }

}
