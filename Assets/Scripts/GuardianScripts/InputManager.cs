using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    private enum InputState
    {
        PC = 0,
        PLAYSTATION = 1,
        XBOX = 2
    };
    [SerializeField] private InputState currentInput = InputState.PLAYSTATION;
    private int inputCurrent = 0;

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
        if (PlayerPrefs.HasKey("Input"))
        {
            inputCurrent = PlayerPrefs.GetInt("Input");
            switch (inputCurrent)
            {
                case 0:
                    currentInput = InputState.PC;
                    break;

                case 1:
                    currentInput = InputState.PLAYSTATION;
                    break;

                case 2:
                    currentInput = InputState.XBOX;
                    break;
            }
        }

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
