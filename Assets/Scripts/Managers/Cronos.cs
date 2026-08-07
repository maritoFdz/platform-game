using UnityEngine;

public class Cronos : MonoBehaviour
{
    [SerializeField, Range(0f, 1f)] private float timeScale;

    void Update()
    {
        Time.timeScale = timeScale;
    }
}
