using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public class PlayerData
{
    public int PlayerIndex;
    public InputUser User;
    public PlayerControls Controls; // The Generated C# Class
    public InputDevice Device;

    // Character Selection state
    public int CharacterID = 0;
    public bool IsReady = false;

    public void Cleanup()
    {
        Controls.Disable();
        User.UnpairDevicesAndRemoveUser();
    }
}