using UnityEngine;
using UnityEngine.SceneManagement;

public class Crystal : MonoBehaviour
{
    public Renderer crystalRenderer;
    public Color activatedColor = Color.yellow;
    public ParticleSystem cureParticles;
    public AudioSource sfxSource;
    public AudioClip cureSound;

    public void CureOrigin()
    {
        sfxSource.PlayOneShot(cureSound);
        if (crystalRenderer != null) crystalRenderer.material.color = activatedColor;
        if (cureParticles != null) cureParticles.Play();
    }
}
