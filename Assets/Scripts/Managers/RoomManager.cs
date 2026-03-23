using System.Collections;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public static RoomManager instance;

    [SerializeField] private Player initialPlayer;
    [SerializeField] private Transform SpawnPoint;
    [SerializeField] private float respawnWait;

    private void Awake()
    {
        if (instance != null)
            Destroy(gameObject);
        else
            instance = this;
    }

    private void Start()
    {
        initialPlayer.transform.position = SpawnPoint.position;
        initialPlayer.SetNormalizedScale(1f);
        initialPlayer.gameObject.SetActive(true);
    }

    public void KillPlayer(Player player, bool isLast)
    {
        if (isLast)
            StartCoroutine(PlayerDiesCo(player));
        else
        {
            player.gameObject.SetActive(false);
            Destroy(player);
        }
    }

    private IEnumerator PlayerDiesCo(Player player)
    {
        player.gameObject.SetActive(false);
        yield return new WaitForSeconds(respawnWait);
        player.transform.position = SpawnPoint.position;
        player.gameObject.SetActive(true);
    }
}
