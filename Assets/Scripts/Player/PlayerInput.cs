using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;


[CreateAssetMenu(fileName = "InputReader", menuName = "PlayerInput/InputReader")]
public class PlayerInput : ScriptableObject, PlayerControls.IPlayerActions
{

    public event UnityAction<bool> Jump = delegate { };
    public event UnityAction<bool> RightRotate = delegate { };
    public event UnityAction<bool> LeftRotate = delegate { };
    public event UnityAction<bool> RightWalk = delegate { };
    public event UnityAction<bool> LeftWalk = delegate { };

    private PlayerControls playerControls;

    private void OnEnable()
    {
        if (playerControls == null)
        {
            playerControls = new PlayerControls();
            playerControls.Player.SetCallbacks(this);
        }
    }

    private void OnDisable()
    {
        
    }

    public void EnablePlayerActions()
    {
        playerControls.Enable();
    }



    public void OnJump(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Performed:
                Jump.Invoke(true);
                break;

            case InputActionPhase.Canceled:
                Jump.Invoke(false);
                break;
        }
    }


    void PlayerControls.IPlayerActions.OnWalkRight(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Performed:
                RightWalk.Invoke(true);
                break;

            case InputActionPhase.Canceled:
                RightWalk.Invoke(false);
                break;
        }
    }

    void PlayerControls.IPlayerActions.OnWalkLeft(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Performed:
                LeftWalk.Invoke(true);
                break;

            case InputActionPhase.Canceled:
                LeftWalk.Invoke(false);
                break;
        }
    }
}
