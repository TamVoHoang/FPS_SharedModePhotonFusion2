using UnityEngine;

public class CharacterInputHandler : MonoBehaviour
{
    [Header("Variable")]
    Vector2 move;
    bool isJumped = false;
    bool isFired = false;
    bool isRocketFired = false;
    bool isGrenadeFired = false;

    public Vector2 Move{get => move;}
    public bool IsJumped{get => isJumped;}
    public bool IsFired{get => isFired;}
    public bool IsGrenadeFired{get => isGrenadeFired;}
    public bool IsRocketFired{get => isRocketFired;}
    // others
    InputActions inputActions;
    CharacterMovementHandler characterMovementHandler;


    private void Awake() {
        inputActions = new InputActions();

        // move
        characterMovementHandler = GetComponent<CharacterMovementHandler>();

        // jump
        inputActions.PlayerMovement.Jump.started += _=> isJumped = true;
        inputActions.PlayerMovement.Jump.canceled += _=> isJumped = false;

        // combat
        inputActions.Combat.Fire.started += _=> isFired = true;
        inputActions.Combat.Fire.canceled += _=> isFired = false;

        inputActions.Combat.Grenade.started += _=> isGrenadeFired = true;
        inputActions.Combat.Grenade.canceled += _=> isGrenadeFired = false;

        inputActions.Combat.Rocket.started += _=> isRocketFired = true;
        inputActions.Combat.Rocket.canceled += _=> isRocketFired = false;

    }


    private void OnEnable() {
        inputActions.PlayerMovement.Move.Enable();
        inputActions.PlayerMovement.Jump.Enable();

        inputActions.Combat.Fire.Enable();
        inputActions.Combat.Grenade.Enable();
        inputActions.Combat.Rocket.Enable();
    }

    private void OnDisable() {
        inputActions.PlayerMovement.Move.Disable();
        inputActions.PlayerMovement.Jump.Disable();

        inputActions.Combat.Fire.Disable();
        inputActions.Combat.Grenade.Disable();
        inputActions.Combat.Rocket.Disable();
    }

    private void Start() {
        
    }

    private void Update() {
        move = inputActions.PlayerMovement.Move.ReadValue<Vector2>();
        move.Normalize();
    }

}
