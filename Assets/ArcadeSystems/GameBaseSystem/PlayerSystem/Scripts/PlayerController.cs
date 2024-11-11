using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using GameBaseSystem.SpawnSystem;
namespace GameBaseSystem
{

    public class PlayerController : MonoBehaviour
    {
        public GameState activeGameState = GameState.Play;
        public int playerIndex;
        public bool alive = true;
        protected Rigidbody2D rb;
        protected List<SpawnSystem.Pickup> pickups = new List<SpawnSystem.Pickup>();

        public List<SpawnSystem.Pickup> Pickups
        {
            get
            {
                return pickups;
            }
        }

        public Rigidbody2D RB
        {
            get
            {
                return rb;
            }
        }
        protected virtual void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            GameController.instance.SubscribePlayer(this);

        }

        protected virtual void OnDestroy()
        {
            GameController.instance.UnsubscribePlayer(this);
        }

        protected virtual void Update()
        {
            if (GameController.instance.Current == activeGameState && alive)
            {
                TestActivationInputs();
                EvaluatePickups();
            }
        }

        protected virtual void EvaluatePickups()
        {
            for (int i = 0; i < pickups.Count; i++)
            {
                pickups[i].Affect(this);
            }
        }

        protected virtual void TestActivationInputs()
        {

            if (ArcadeInput.InputInitiated(playerIndex, ArcadeInputType.ButtonA))
            {
                TestPickupActivation(ArcadeInputType.ButtonA);
            }
            if (ArcadeInput.InputInitiated(playerIndex, ArcadeInputType.ButtonB))
            {
                TestPickupActivation(ArcadeInputType.ButtonB);
            }
            if (ArcadeInput.InputInitiated(playerIndex, ArcadeInputType.ButtonC))
            {
                TestPickupActivation(ArcadeInputType.ButtonC);
            }
            if (ArcadeInput.InputInitiated(playerIndex, ArcadeInputType.ButtonD))
            {
                TestPickupActivation(ArcadeInputType.ButtonD);
            }
            if (ArcadeInput.InputInitiated(playerIndex, ArcadeInputType.ButtonE))
            {
                TestPickupActivation(ArcadeInputType.ButtonE);
            }
            if (ArcadeInput.InputInitiated(playerIndex, ArcadeInputType.ButtonF))
            {
                TestPickupActivation(ArcadeInputType.ButtonF);
            }
        }

        protected virtual void TestPickupActivation(ArcadeInputType input)
        {
            for (int i = 0; i < pickups.Count; i++)
            {
                pickups[i].ActivatePickup(this, input);
            }
        }

        public virtual void AddPickup(SpawnSystem.Pickup pickup)
        {
            pickups.Add(pickup);
        }

        public virtual void RemovePickup(SpawnSystem.Pickup pickup)
        {
            pickups.Remove(pickup);
        }
    }
}

 