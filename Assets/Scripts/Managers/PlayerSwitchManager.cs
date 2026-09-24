using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSwitchManager : MonoBehaviour
{
    public static PlayerSwitchManager instance;

    [Header("References")]
    [SerializeField] private SwitchParticle switchParticlePrefab;
    [SerializeField] private LayerMask cameraSwitchTriggerLayer;

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
        HandleCameraSwitching(current, next);
        SpawnParticles(current.transform.position, next.transform);
        next.EnableInput();
    }

    private void SpawnParticles(Vector3 origin, Transform target)
    {
        ClearParticles();
        SwitchParticle particlesThrown = Instantiate(switchParticlePrefab, origin, Quaternion.identity);
        particlesThrown.ThrowRay(target);
        activeParticles.Add(particlesThrown);
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
        if (index == -1)
        {
            player.gameObject.SetActive(false);
            return;
        }
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

        if (activePlayerIndex >= activePlayers.Count)
        {
            activePlayerIndex = activePlayers.Count - 1;
        }
        Player newCurrent = activePlayers[activePlayerIndex];
        if (activePlayers.Count > 0 && wasCurrent)
            SpawnParticles(player.transform.position, newCurrent.transform);
        newCurrent.EnableInput();
    }

    private void HandleCameraSwitching(Player player, Player player2)
    {
        Vector2 start = player.transform.position;
        Vector2 end = player2.transform.position;

        RaycastHit2D[] hits = Physics2D.LinecastAll(start, end, cameraSwitchTriggerLayer);
        if (hits.Length == 0) return;

        RaycastHit2D closestTrigger = hits[0];
        float closestDist = Vector2.Distance(closestTrigger.point, end);
        foreach (RaycastHit2D hit in hits)
        {
            float distance = Vector2.Distance(hit.point, end);
            if (distance < closestDist)
            {
                closestDist = distance;
                closestTrigger = hit;
            }
        }

        if (closestTrigger.collider.TryGetComponent<CameraSwitchTrigger>(out var trigger))
            trigger.Activate();
    }

    public bool IsAdded(Player player)
    {
        return activePlayers.Contains(player);
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
