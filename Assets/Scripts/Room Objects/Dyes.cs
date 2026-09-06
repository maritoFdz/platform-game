using UnityEngine;

[RequireComponent (typeof(BoxCollider2D))]
[RequireComponent (typeof(Rigidbody2D))]
public class Dyes : MonoBehaviour
{
    [SerializeField] private ColorPallete palleteUnlocked;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        if (DataManager.instance != null)
            DataManager.instance.AddUnlockedPallete(palleteUnlocked);
        if (SettingsManager.instance != null)
            SettingsManager.instance.SetPalleteById(palleteUnlocked.palleteId);
        Destroy(gameObject);
    }
}
