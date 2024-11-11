using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace GameBaseSystem.SpawnSystem
{
    [System.Serializable]
    public class SpawnEntry
    {
        public float spawnFactor;
        public Pickup pickup;
    }


    public class PickupSpawnerController : MonoBehaviour
    {
        // Start is called before the first frame update
        public static PickupSpawnerController instance;

        public bool spawn = false;
        public float gridSize = 1;
        public float pickupScale = .7f;
        public float spawnSeparationMeanTime;
        public float minSpawnTimeSeparation = 3;
        public int maxPickups = 2;
        public float pickupPlayerMinDistance = 0.7f;
        public PickupSpawner spawnerTemplate; 

        public List<SpawnEntry> pickupEntries;
        public List<PickupSpawner> spawners;

        public List<Pickup> pickups = new List<Pickup>();
        public List<Pickup> currentlyActive = new List<Pickup>();
        private float lastSpawn = -100;
        void Awake()
        {
            if (instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Object.DontDestroyOnLoad(gameObject);
            instance = this;

            float summedFactors = 0;
            for (int i = 0; i < pickupEntries.Count; i++)
            {
                summedFactors += pickupEntries[i].spawnFactor;
            }

            for (int i = 0; i < pickupEntries.Count; i++)
            {
                int amount = Mathf.RoundToInt((pickupEntries[i].spawnFactor / summedFactors) * 100);
                for (int j = 0; j < amount; j++)
                {
                    pickups.Add(pickupEntries[i].pickup);
                }
            }

        }

        public void ResetSpawnController()
        {
            currentlyActive.Clear();
            spawners.Clear();
            spawn = false;
        }

        public void SetupSpawners()
        {
            int numOfHeight = Mathf.RoundToInt((Camera.main.orthographicSize * 2) / gridSize);
            int numOfWidth = Mathf.RoundToInt((Camera.main.orthographicSize * 2 * Camera.main.aspect) / gridSize);
            
            Vector2 start = new Vector2(Camera.main.transform.position.x - ((float)(numOfWidth-1) / 2), Camera.main.transform.position.y - ((float)(numOfHeight-1) / 2));

            Vector2 current = start;

            for (int x = 0; x < numOfWidth; x++)
            {
                for (int y = 0; y < numOfHeight; y++)
                {
                    GameObject newSpawner = Instantiate(spawnerTemplate.gameObject);
                    newSpawner.transform.position = current;
                    newSpawner.transform.localScale = Vector3.one * pickupScale;
                    current.y += gridSize;
                }
                current.y = start.y;
                current.x += gridSize;
            }
            spawn = true;
        }
        // Update is called once per frame
        void Update()
        {
            if (spawn && spawners.Count > 0 && lastSpawn + minSpawnTimeSeparation <= Time.time)
            {
                float random = Random.Range(0f, 1f);

                if (random <= (1f / spawnSeparationMeanTime) * Time.deltaTime)
                {
                    int maxTrials = 100;
                    int trial = 0;
                    int preActive = currentlyActive.Count;
                    while (!spawners[Random.Range(0, spawners.Count)].Spawn() && trial < maxTrials)
                    {
                        trial++;
                    }
                    if (preActive != currentlyActive.Count)
                    {
                        lastSpawn = Time.time;
                    }
                }

            }
        }
    }
}

