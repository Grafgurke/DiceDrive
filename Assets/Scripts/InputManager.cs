using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [SerializeField] private PlayerInput player1Input;
    [SerializeField] private PlayerInput player2Input;
    
    [SerializeField] private InputDevice defaultPlayer1Device;
    [SerializeField] private InputDevice defaultPlayer2Device;

    private void Start()
    {
        SetupPlayerDevices();
    }

    private void SetupPlayerDevices()
    {
        // Get all available devices
        var gamepads = Gamepad.all;
        
        // If we have at least one gamepad connected
        if (gamepads.Count > 0)
        {
            // Assign first gamepad to player 1
            player1Input.SwitchCurrentControlScheme("Gamepad1", gamepads[0]);
            
            // If we have a second gamepad, assign it to player 2
            if (gamepads.Count > 1)
            {
                player2Input.SwitchCurrentControlScheme("Gamepad2", gamepads[1]);
            }
            else
            {
                // Only one gamepad, player 2 uses keyboard
                player2Input.SwitchCurrentControlScheme("Keyboard2", Keyboard.current);
            }
        }
        else
        {
            // No gamepads, both players use keyboard
            player1Input.SwitchCurrentControlScheme("Keyboard1", Keyboard.current);
            player2Input.SwitchCurrentControlScheme("Keyboard2", Keyboard.current);
        }
    }

    // For debugging/testing

}