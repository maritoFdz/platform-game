using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSwitchManager : MonoBehaviour
{
    public static PlayerSwitchManager instance;
    private List<Player> activePlayers;
    private int activePlayerIndex;
    private PlayerInput playerInput;

    private void Awake()
    {
        if (instance != null)
            Destroy(gameObject);
        else
            instance = this;
       activePlayers = new List<Player>();
       playerInput = new();
    }

    private void OnEnable()
    {
        playerInput.Player.Enable();
        playerInput.Player.Switch.performed += Switch;
    }

    private void OnDisable()
    {
        playerInput.Player.Disable();
        playerInput.Player.Switch.performed -= Switch;
    }

    private void Switch(InputAction.CallbackContext callback)
    {
        Player current = activePlayers[activePlayerIndex];
        current.DisableInput();
        activePlayerIndex = (activePlayerIndex + 1) % activePlayers.Count;
        Player next = activePlayers[activePlayerIndex];
        next.EnableInput();
    }

    public void Add(Player player)
    {
        activePlayers.Add(player);
        player.SetInput(playerInput);
        player.DisableInput();
        if (activePlayers.Count == 1)
        {
            activePlayerIndex = 0;
            player.EnableInput();
        }
        player.SwitchState(player.idleState);
    }

    public void Erase(Player player)
    {
        int index = activePlayers.IndexOf(player);
        bool wasCurrent = index == activePlayerIndex;
        player.DisableInput();
        activePlayers.Remove(player);

        bool isLast = activePlayers.Count == 0;
        if (isLast)
        {
            RoomManager.instance.KillPlayer(player, true);
            return;
        }

        if (index < activePlayerIndex)
            activePlayerIndex--;
        else if (wasCurrent)
            activePlayerIndex %= activePlayers.Count;

        Player newCurrent = activePlayers[activePlayerIndex];
        newCurrent.EnableInput();

        RoomManager.instance.KillPlayer(player, false);
    }
}
