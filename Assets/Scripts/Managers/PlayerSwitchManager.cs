using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSwitchManager : MonoBehaviour
{
    public static PlayerSwitchManager instance;

    [Header("References")]
    [SerializeField] private SwitchParticle switchParticlePrefab;

    private List<Player> activePlayers;
    private List<SwitchParticle> activeParticles;
    private int activePlayerIndex;
    private PlayerInput playerInput;

    private void Awake()
    {
        if (instance != null) Destroy(gameObject);
        else instance = this;
        activePlayers = new List<Player>();
        activeParticles = new List<SwitchParticle>();
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
        if (activePlayers.Count <= 1) return;

        Player current = activePlayers[activePlayerIndex];
        current.DisableInput();
        activePlayerIndex = (activePlayerIndex + 1) % activePlayers.Count;
        Player next = activePlayers[activePlayerIndex];
        SpawnParticles(current.transform.position, next.transform);
        next.EnableInput();
    }

    private void SpawnParticles(Vector3 origin, Transform target)
    {
        ClearParticles();
        SwitchParticle particlesThrwwn = Instantiate(switchParticlePrefab, origin, Quaternion.identity);
        particlesThrwwn.ThrowRay(target);
        activeParticles.Add(particlesThrwwn);
    }

    private void ClearParticles()
    {
        foreach (var particles in activeParticles)
            if (particles != null) Destroy(particles.gameObject);
        activeParticles.Clear();
    }

    public void Add(Player player)
    {
        activePlayers.Add(player);
        player.SetInput(playerInput);
        player.DisableInput();
        if (activePlayers.Count == 1)
        {
            activePlayerIndex = 0;
            if (player.IsActive)
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
        Destroy(player.gameObject);

        if (activePlayers.Count == 0)
        {
            RoomManager.instance.PlayersDead();
            return;
        }

        if (index < activePlayerIndex)
            activePlayerIndex--;
        else if (wasCurrent)
            activePlayerIndex %= activePlayers.Count;

        Player newCurrent = activePlayers[activePlayerIndex];
        if (activePlayers.Count > 0 && wasCurrent)
            SpawnParticles(player.transform.position, newCurrent.transform);
        newCurrent.EnableInput();
    }

    public void DisableAll()
    {
        playerInput.Disable();
    }

    public void EnableAll()
    {
        playerInput.Enable();
    }
}
