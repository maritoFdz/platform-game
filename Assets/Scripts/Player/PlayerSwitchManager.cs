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
        current.SwitchState(current.waitingState);
        activePlayerIndex = (activePlayerIndex + 1) % activePlayers.Count;
        current = activePlayers[activePlayerIndex];
        current.SwitchState(current.idleState);
    }

    public void Add(Player player)
    {
        activePlayers.Add(player);
        player.SetInput(playerInput);
        if (activePlayers.Count == 1)
        {
            activePlayerIndex = 0;
            player.SwitchState(player.idleState);
        }
        else
            player.SwitchState(player.waitingState);
    }

    public void Erase(Player player)
    {
        int index = activePlayers.IndexOf(player);
        bool wasCurrent = index == activePlayerIndex;
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
        newCurrent.SwitchState(newCurrent.idleState);

        RoomManager.instance.KillPlayer(player, false);
    }
}
