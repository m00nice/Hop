using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace GameBaseSystem.SpawnSystem
{
    public class Pickup : MonoBehaviour
    {
        public ArcadeInputType activation = ArcadeInputType.ButtonRed;
        public bool requireActivation = true;
        public bool removeAfterAffect = false;
        public string pickupName = "";
        public bool keepEffectAfterLifeTime = false;
        public float lifeTime = 3;
        public bool continuousAdd = false;
        public float affectDelay = 1;
        public bool isDestroyedOnImpact = true;

        
        public PlayerController owner;
        public PickupSpawner spawner = null;
        protected float addTime = 0;
        protected float lastAffect = 0;
        protected bool isActivated = false;
        

        public float TimeLeft
        {
            get
            {
                if (lifeTime > 0)
                {
                    return lifeTime - (Time.time - addTime);
                }
                return lifeTime;

            }
        }
        public virtual void Add(PlayerController controller)
        {
            Pickup clone = Instantiate(gameObject).GetComponent<Pickup>();
            clone.GetComponent<Collider2D>().enabled = false;
            controller.AddPickup(clone);
            clone.owner = controller;
            clone.isDestroyedOnImpact = true;
            if (!clone.requireActivation)
            {
                clone.ActivatePickup(controller, activation);
            }
        }

        public virtual void Remove(PlayerController controller)
        {
            controller.RemovePickup(this);

            if (isActivated)
            {
                
            }

            if (isDestroyedOnImpact)
            {
                Destroy(gameObject);
            }

        }

        public virtual void ActivatePickup (PlayerController controller, ArcadeInputType activation)
        {
            if (this.activation == activation && !isActivated)
            {
                isActivated = true;
                addTime = Time.time;
                lastAffect = addTime - affectDelay;
            }
            
        }
        public virtual void Affect(PlayerController controller)
        {
            if (isActivated)
            {
                if ((Time.time - addTime >= lifeTime && lifeTime > 0) || removeAfterAffect)
                {
                    Remove(controller);
                    return;
                }
            }
        }

        public virtual void OnTriggerEnter2D(Collider2D collision)
        {
            Rigidbody2D triangleRB = collision.attachedRigidbody;

            if (triangleRB != null)
            {
                PlayerController controller = triangleRB.GetComponent<PlayerController>();
                if (controller != null)
                {
                    Add(controller);

                    if (isDestroyedOnImpact)
                    {
                        PickupSpawnerController.instance.currentlyActive.Remove(this);
                        if (spawner != null)
                        {
                            spawner.pickup = null;
                        }
                        Destroy(this.gameObject);
                    }
                }
            }
        }
    }
}

