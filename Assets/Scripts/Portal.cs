using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    public int health = 3;
    public bool isOpen = false;
    public GameObject sphereMesh;
    public ParticleSystem closedParticles;
    public ParticleSystem openParticles;
    public AudioSource sfxSource;
    public AudioClip portalSound;

    private void Start()
    {
        if (closedParticles != null) closedParticles.Play();
        if (openParticles != null) openParticles.Stop();
    }

    public void GetDamage(int damage)
    {
        if (isOpen) return;

        health -= damage;
        if (health <= 0)
        {
            OpenPortal();
        }
    }

    private void OpenPortal()
    {
        isOpen = true;

        sfxSource.clip = portalSound;
        sfxSource.loop = true;
        sfxSource.Play();

        if (sphereMesh != null)
            Destroy(sphereMesh);

        if (closedParticles != null)
            closedParticles.Stop();

        if (openParticles != null)
            openParticles.Play();
    }
}