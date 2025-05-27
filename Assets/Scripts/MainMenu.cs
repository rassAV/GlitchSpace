using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public AudioSource musicSource;
    public AudioClip backgroundMusic;

    private void Start()
    {
        Screen.fullScreen = true;
        if (musicSource != null && backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    public void Play() {
        SceneManager.LoadScene(1, LoadSceneMode.Single);
    }

    public void Continue() {
        int savedLevel = PlayerPrefs.GetInt("CurrentLevel", 0);
        if (savedLevel >= 2 && savedLevel <= 12) SceneManager.LoadScene(savedLevel, LoadSceneMode.Single);
    }

    public void Exit()
    {
        Application.Quit();
    }
}
