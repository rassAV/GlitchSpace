using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    protected Transform _target;
    protected int health;
    public int startHealth;
    protected float stunnedTime = 3f;
    protected bool isStunned = false;
    public bool typeCrystal = false;
    public bool typeInvisible = false;
    public ParticleSystem stunParticles;
    public ParticleSystem deathParticles;
    public AudioSource sfxSource;
    public AudioClip deathSound;

    protected virtual void Start()
    {
       GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _target = player.transform;
        }
        else
        {
            Debug.LogError("Player not found");
        }
        health = startHealth;
        ApplyTransparency();
    }

    protected void ApplyTransparency()
    {
        Renderer renderer = GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            Color color = renderer.material.color;

            if (typeInvisible)
            {
                color.a = 0.0f;
                renderer.material.SetFloat("_Smoothness", 0f);
            } else if (typeCrystal)
            {
                color.a = 0.6f;
            }

            renderer.material.color = color;
        }
    }

    protected virtual void Update()
    {
        if (_target != null) Act();
    }

    protected abstract void Act();

    public void GetDamage(int damage, bool rangeAttack) {
        if (rangeAttack && typeCrystal) damage = 0;
        health -= damage;
        if (health < 0) Death();
    }

    public void Death() {
        Debug.Log("DEATH");
        sfxSource.PlayOneShot(deathSound);
        if (deathParticles != null)
        {
            deathParticles.transform.SetParent(null);
            deathParticles.Play();
            Destroy(deathParticles.gameObject, 1f);
        }
        Destroy(gameObject);
    }

    public void PlayStunParticles() {
        if (stunParticles != null) stunParticles.Play();
    }

    public void StopStunParticles() {
        if (stunParticles != null) stunParticles.Stop();
    }
}