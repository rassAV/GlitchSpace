using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class BeginScene : MonoBehaviour
{
    public GameObject textWindow;
    public AudioSource sfxSource;
    public AudioClip cyberspaceSound;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            textWindow.SetActive(false);
            sfxSource.PlayOneShot(cyberspaceSound);
            StartCoroutine(LoadSceneDelayed());
        }
    }

    private IEnumerator LoadSceneDelayed()
    {
        Debug.Log("[BeginScene] Starting coroutine");
        yield return new WaitForSecondsRealtime(1.5f);
        Debug.Log("[BeginScene] Loading Level 1");
        SceneManager.LoadScene(2, LoadSceneMode.Single);
    }
}