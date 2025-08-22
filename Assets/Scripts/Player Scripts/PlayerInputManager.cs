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

        inputActions.Player.Enable();

        //register callbacks
        inputActions.Player.Move.performed += context => movementInput = context.ReadValue<Vector2>();
        inputActions.Player.Look.performed += context => lookInput = context.ReadValue<Vector2>();
        inputActions.Player.Attack.performed += context => attackInput = context.ReadValueAsButton();
        inputActions.Player.Aim.performed += context => aimInput = context.ReadValueAsButton();

        LockCursor();
    }

    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
