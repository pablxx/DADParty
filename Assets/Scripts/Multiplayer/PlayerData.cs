using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

[System.Serializable]
public class PlayerData
{
    public int playerIndex;
    public InputUser user;
    public PlayerControls controls; // The Generated C# Class
    public InputDevice device;

    // Character Selection state
    public int characterID = 0;
    public bool isReady = false;

    public void Cleanup()
    {
        controls.Disable();
        user.UnpairDevicesAndRemoveUser();
    }
}