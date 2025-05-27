using System.Collections;
using UnityEngine;

public class AmmoRestore : MonoBehaviour
{
    public float respawnTime = 5f;
    private bool isAvailable = true;
    public GameObject platform;
    private Renderer _renderer;
    private Collider _collider;
    private Material _material;
    private Color _originalColor;
    private float _originalSmoothness;
    private bool anotherWorld = false;

    void Start()
    {
        _renderer = GetComponent<Renderer>();
        _collider = GetComponent<Collider>();

        if (_renderer != null)
        {
            _material = _renderer.material;
            _originalColor = _material.color;
            _originalSmoothness = _material.GetFloat("_Smoothness");
        }

        SetAnotherWorldVisible(anotherWorld);
    }

    void OnTriggerEnter(Collider other)
    {
        if (isAvailable && other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();
            if (player != null && player.GetAnotherWorldState() == true)
            {
                player.sfxSource.PlayOneShot(player.recoveryEnergySound);
                player.ammo = true;
                player.ui.SetAmmo(true);
                StartCoroutine(Respawn());
            }
        }
    }

    private IEnumerator Respawn()
    {
        isAvailable = false;
        SetVisible(false);

        yield return new WaitForSecondsRealtime(respawnTime);

        isAvailable = true;
        if (anotherWorld) SetVisible(true);
    }

    private void SetVisible(bool visible)
    {
        if (_material != null)
        {
            Color color = _originalColor;
            color.a = visible ? 1f : 0f;
            _material.color = color;
            _material.SetFloat("_Smoothness", visible ? _originalSmoothness : 0f);
        }

        if (_renderer != null)
            _renderer.enabled = visible;

        if (_collider != null)
            _collider.enabled = visible;
    }

    public void SetAnotherWorldVisible(bool visible)
    {
        anotherWorld = visible;
        if (platform != null) platform.SetActive(visible);
        if (isAvailable)
        {
            SetVisible(visible);
        }
        else
        {
            SetVisible(false);
        }
    }
}