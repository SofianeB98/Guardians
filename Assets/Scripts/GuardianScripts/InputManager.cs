using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    private enum InputState
    {
        PC,
        PLAYSTATION,
        XBOX
    };
    [SerializeField] private InputState currentInput = InputState.PLAYSTATION;

    private void Awake()
    {
        AssignInput();
    }

    private void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            currentInput = InputState.PC;
            AssignInput();
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            currentInput = InputState.XBOX;
            AssignInput();
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            currentInput = InputState.PLAYSTATION;
            AssignInput();
        }
    }

    void AssignInput()
    {
        switch (currentInput)
        {
            case InputState.PC:
                InputName.Init("?");
                break;

            case InputState.PLAYSTATION:
                InputName.Init("PS");
                break;

            case InputState.XBOX:
                InputName.Init("XB");
                break;
        }
    }
}
