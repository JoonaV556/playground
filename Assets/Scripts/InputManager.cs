using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputData GetInput()
    {
        var input = new InputData
        {
            Move = _input.Move,
            Look = _input.Look,
            Jump = _input.Jump,
            Sprint = _input.Sprint
        };

        return input;
    }

    public InputActionAsset InputActions;

    private static InputData _input = new InputData()
    {
        Move = Vector2.zero,
        Look = Vector2.zero,
        Sprint = false,
        Jump = false
    };

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        // enable the input actions
        InputActions.Enable();

        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    private void Update()
    {
        _input.Jump = false;    // reset the jump input

        _input.Move = InputActions["Move"].ReadValue<Vector2>();
        _input.Look = InputActions["Look"].ReadValue<Vector2>();
        _input.Jump = InputActions["Jump"].WasPerformedThisFrame();
        _input.Sprint = InputActions["Sprint"].IsPressed();
    }
}
