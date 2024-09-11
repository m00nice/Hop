using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformSpawner : MonoBehaviour
{

    [SerializeField] private int difficulty;
    [SerializeField] private PlatformSetup[] platformSetups;
    [SerializeField] private GameObject[] platforms;


    void Start()
    {
        GetRandomPlatformSetup();
    }

    void GetRandomPlatformSetup()
    {
        if (difficulty == 0)
        {

        }
        else if (difficulty == 1)
        {

        }
        else
        {

        }
    }

}
