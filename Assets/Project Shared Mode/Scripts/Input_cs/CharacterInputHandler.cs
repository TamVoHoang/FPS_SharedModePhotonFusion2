using System;
using UnityEngine;

public class CharacterInputHandler : MonoBehaviour
{
    [Header("Variable")]
    Vector2 move;
    Vector2 aimDir;
    bool isJumped = false;
    bool isFired = false;
    bool isRocketFired = false;
    bool isGrenadeFired = false;
    bool is3rdPersonCam = false;
    bool isDropWeapon = false;
    bool isSwitchWeapon = false;
    bool isTutorialActived = false;
    bool isExitPanel = false;
    bool isRealtimeResultPanel = false;


    public Vector2 Move{get => move;}
    public Vector2 AimDir {get => aimDir;}
    public bool IsFired{get => isFired;}
    public bool IsGrenadeFired{get => isGrenadeFired;}
    public bool IsRocketFired{get => isRocketFired;}
    // others
    InputActions inputActions;
    CharacterMovementHandler characterMovementHandler;

    public Action OnJump;
    public Action<bool> OnDropWeapon;
    public Action<bool> OnSwitchWeapon;
    public Action<bool> OnSwitchCamera;
    public Action<bool> OnTutorial;
    public Action<bool> OnExitTable;
    public Action<bool> OnRealtimeResultTable;

    private void Awake() {
        inputActions = new InputActions();

        // move
        characterMovementHandler = GetComponent<CharacterMovementHandler>();

        //switch camera
        inputActions.PlayerMovement.SwitchCam.started += _=> SwitchCamera();

        // jump
        inputActions.PlayerMovement.Jump.started += _=> Jump();
        inputActions.PlayerMovement.Jump.canceled -= _=> Jump();

        // combat
        inputActions.Combat.Fire.started += _ => isFired = true;
        inputActions.Combat.Fire.canceled += _ => isFired = false;

        inputActions.Combat.Grenade.started += _ => isGrenadeFired = true;
        inputActions.Combat.Grenade.canceled += _ => isGrenadeFired = false;

        inputActions.Combat.Rocket.started += _ => isRocketFired = true;
        inputActions.Combat.Rocket.canceled += _ => isRocketFired = false;

        inputActions.Combat.SwapGun.started += _ => SwitchWeapon();
        inputActions.Combat.SwapGun.canceled -= _ => SwitchWeapon();

        inputActions.Combat.DropGun.started += _ => DropWeapon();
        inputActions.Combat.DropGun.canceled -= _ => DropWeapon();

        inputActions.UI.Tutorial.started += _ => Tutorial();
        inputActions.UI.Tutorial.canceled += _ => Tutorial();

        inputActions.UI.ExitPanel.started += _ => ExitPanel();
        inputActions.UI.ExitPanel.canceled -= _ => ExitPanel();

        inputActions.UI.RealtimeTable.started += _ => RealtimeTable();
        inputActions.UI.RealtimeTable.canceled -= _ => RealtimeTable();

    }

    private void RealtimeTable()
    {
        isRealtimeResultPanel = !isRealtimeResultPanel;
        OnRealtimeResultTable?.Invoke(isRealtimeResultPanel);
    }

    private void ExitPanel() {
        isExitPanel = !isExitPanel;
        OnExitTable?.Invoke(isExitPanel);
    }

    private void Tutorial() {
        isTutorialActived = !isTutorialActived;
        OnTutorial?.Invoke(isTutorialActived);
    }

    private void SwitchCamera() {
        is3rdPersonCam = !is3rdPersonCam;
        OnSwitchCamera?.Invoke(is3rdPersonCam);
    }

    private void Jump() {
        OnJump?.Invoke();
    }

    private void SwitchWeapon() {
        OnSwitchWeapon?.Invoke(true);
    }

    private void DropWeapon() {
        OnDropWeapon?.Invoke(true);
    }

    private void OnEnable() {
        inputActions.PlayerMovement.Move.Enable();
        inputActions.PlayerMovement.Look.Enable();
        inputActions.PlayerMovement.Jump.Enable();
        inputActions.PlayerMovement.SwitchCam.Enable();

        inputActions.Combat.Fire.Enable();
        inputActions.Combat.Grenade.Enable();
        inputActions.Combat.Rocket.Enable();

        inputActions.Combat.DropGun.Enable();
        inputActions.Combat.SwapGun.Enable();

        inputActions.UI.Tutorial.Enable();
        inputActions.UI.ExitPanel.Enable();

        inputActions.UI.RealtimeTable.Enable();
    }

    private void OnDisable() {
        inputActions.PlayerMovement.Move.Disable();
        inputActions.PlayerMovement.Look.Disable();
        inputActions.PlayerMovement.Jump.Disable();
        inputActions.PlayerMovement.SwitchCam.Disable();

        inputActions.Combat.Fire.Disable();
        inputActions.Combat.Grenade.Disable();
        inputActions.Combat.Rocket.Disable();

        inputActions.Combat.DropGun.Disable();
        inputActions.Combat.SwapGun.Disable();

        inputActions.UI.Tutorial.Disable();
        inputActions.UI.ExitPanel.Disable();

        inputActions.UI.RealtimeTable.Disable();
    }

    private void Update() {
        move = inputActions.PlayerMovement.Move.ReadValue<Vector2>();
        move.Normalize();

        aimDir = inputActions.PlayerMovement.Look.ReadValue<Vector2>();
    }
}