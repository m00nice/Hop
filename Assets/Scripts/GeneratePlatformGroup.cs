using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratePlatformGroup : MonoBehaviour
{

    [SerializeField] private GameObject[] platformGroups;


    void Start()
    {
        GeneratePlatformGroups();
    }

    void GeneratePlatformGroups()
    {
        Instantiate(platformGroups[Random.Range(0, platformGroups.Length)], transform, false);
    }
}
