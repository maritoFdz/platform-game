using System.Collections;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public static RoomManager instance;

    [SerializeField] private Player player;
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
        player.transform.position = SpawnPoint.position;
        player.gameObject.SetActive(true);
    }

    public void KillPlayer()
    {
        StartCoroutine(PlayerDiesCo());
    }

    private IEnumerator PlayerDiesCo()
    {
        player.gameObject.SetActive(false);
        yield return new WaitForSeconds(respawnWait);
        player.transform.position = SpawnPoint.position;
        player.gameObject.SetActive(true);
    }
}
