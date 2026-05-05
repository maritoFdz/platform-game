using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private string mainSceneName;

    private void Start()
    {
        StartCoroutine(InitCo());
        DontDestroyOnLoad(gameObject);
    }

    private IEnumerator InitCo()
    {
        yield return new WaitUntil(() =>
        SettingsManager.instance != null
        && DataManager.instance != null
        && SettingsManager.instance.initialized
        && DataManager.instance.initialized);
        SceneManager.LoadScene(mainSceneName);
    }
}
