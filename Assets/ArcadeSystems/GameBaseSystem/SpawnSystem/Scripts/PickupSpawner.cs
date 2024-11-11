using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameBaseSystem.SpawnSystem;

namespace GameBaseSystem.SpawnSystem
{
    public class PickupSpawner : MonoBehaviour
    {
        // Start is called before the first frame update

        public Pickup pickup = null;
        void Start()
        {
            PickupSpawnerController.instance.spawners.Add(this);
        }

        public bool Spawn ()
        {
            if (pickup != null || PickupSpawnerController.instance.currentlyActive.Count >= PickupSpawnerController.instance.maxPickups)
            {
                return false;
            }

            for (int i = 0; i < GameController.instance.Players.Count; i++)
            {
                if (Vector2.SqrMagnitude(GameController.instance.Players[i].transform.position - transform.position) < PickupSpawnerController.instance.pickupPlayerMinDistance* PickupSpawnerController.instance.pickupPlayerMinDistance)
                {
                    return false;
                }
            }

            GetComponent<Collider2D>().enabled = false;

            int random = Random.Range(0, PickupSpawnerController.instance.pickups.Count);

            pickup = Instantiate(PickupSpawnerController.instance.pickups[random]);
            pickup.transform.position = transform.position;
            pickup.transform.localScale = Vector3.one*.7f;
            pickup.spawner = this;
            PickupSpawnerController.instance.currentlyActive.Add(pickup);


            return true;
        }
        

        // Update is called once per frame
        private void OnTriggerEnter2D(Collider2D collision)
        {
            //Debug.Log("Wtf?");
            PickupSpawnerController.instance.spawners.Remove(this);
            Destroy(this.gameObject);
        }
    }
}

