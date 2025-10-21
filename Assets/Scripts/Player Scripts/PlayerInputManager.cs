using UnityEngine;

public class PlayerInputManager : MonoBehaviour
{
    public static PlayerInputManager Instance;

    private InputSystem_Actions inputActions;

    [Header("Movement")]
    public Vector2 movementInput;

    [Header("Look")]
    public Vector2 lookInput;

    [Header("Actions")]
    public bool attackInput;
    public bool aimInput;
    public bool sprintInput;
    public bool jumpInput;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    
        inputActions = new InputSystem_Actions();

        // Register callbacks for player actions (gameplay)
        inputActions.Player.Move.performed += context => movementInput = context.ReadValue<Vector2>();
        inputActions.Player.Look.performed += context => lookInput = context.ReadValue<Vector2>();
        inputActions.Player.Attack.performed += context => attackInput = context.ReadValueAsButton();
        inputActions.Player.Aim.performed += context => aimInput = context.ReadValueAsButton();
        inputActions.Player.Sprint.performed += context => sprintInput = context.ReadValueAsButton();
        inputActions.Player.Jump.performed += context => jumpInput = context.ReadValueAsButton();

        // TODO: Deal with this when I want controller/keyboard UI navigation support.
        // Register callbacks for UI actions

        // By default we start with UI.
        DisableGameplayInput();
        EnableUIInput();
        UnlockCursor();
    }

    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void EnableGameplayInput()
    {
        inputActions.Player.Enable();
    }

    public void DisableGameplayInput()
    { 
        inputActions.Player.Disable();
        // We also reset all the input to prevent movement and camera scripts from behaving weird. 
        movementInput = Vector2.zero;
        lookInput = Vector2.zero;
        attackInput = false;
        aimInput = false;
        sprintInput = false;
        jumpInput = false;
    }

    public void EnableUIInput()
    { 
        inputActions.UI.Enable();
    }

    public void DisableUIInput()
    {
        inputActions.UI.Disable();
    }
}
