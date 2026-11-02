using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputs : MonoBehaviour
{
    public InputActionAsset inputActionsAsset;
    public InputActionMap playerMap;
    public InputAction moveAction;
    public InputAction lookAction;
    public InputAction interactAction;
    public InputAction reloadAction;
    public InputAction nextAction;
    public InputAction prevAction;
    public InputAction sprintAction;
    public InputAction aimAction;
    public InputAction shootAction;
    public InputAction skillAction;
    public InputAction inventoryAction;
    public InputAction lastWeaponAction;
    public InputAction pauseMenu;
    private void Start()
    {

        playerMap = inputActionsAsset.FindActionMap("Player");

        moveAction = playerMap.FindAction("Move");
        lookAction = playerMap.FindAction("Look");
        interactAction = playerMap.FindAction("Interact");
        reloadAction = playerMap.FindAction("Reload");
        nextAction = playerMap.FindAction("Next");
        prevAction = playerMap.FindAction("Previous");
        sprintAction = playerMap.FindAction("Sprint");
        aimAction = playerMap.FindAction("Aim");
        shootAction = playerMap.FindAction("Shoot");
        skillAction = playerMap.FindAction("Skill");
        inventoryAction = playerMap.FindAction("Inventory");
        lastWeaponAction = playerMap.FindAction("LastWeapon");
        pauseMenu = playerMap.FindAction("PauseMenu");

        playerMap.Enable();
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnEnable()
    {
        playerMap.Enable();
    }

    private void OnDisable()
    {
        playerMap.Disable();
    }

}
