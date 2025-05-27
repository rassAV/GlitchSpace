using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class RangeAI : Enemy
{
    public GameObject bulletPrefab;
    public Transform bulletPoint;
    public float attackRange = 10f;
    public float shootCooldown = 1f;
    public float shootDelay = 0.25f;
    public int bulletDamage = 2;
    protected bool _canShoot = true;
    protected Animator _anim;
    public AudioClip swingSound;
    public AudioClip shootSound;

    protected override void Start()
    {
        base.Start();
        _anim = GetComponent<Animator>();
    }

    protected override void Act()
    {
        LookAtPlayer();
        TryShoot();
    }

    protected void LookAtPlayer()
    {
        if (!isStunned) {
            Vector3 direction = (_target.position - transform.position).normalized;
            direction.y = 0;
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    protected void TryShoot()
    {
        float distance = Vector3.Distance(transform.position, _target.position);
        if (distance <= attackRange && _canShoot)
        {
            StartCoroutine(ShootWithDelay());
        }
    }

    protected IEnumerator ShootWithDelay()
    {
        if (!isStunned) {
            _anim.SetBool("isSwing", true);
            _canShoot = false;
            sfxSource.PlayOneShot(swingSound);
            yield return new WaitForSecondsRealtime(shootDelay);
            sfxSource.PlayOneShot(shootSound);
            GameObject bullet_obj = Instantiate(bulletPrefab, bulletPoint.position, Quaternion.identity);
            bullet_obj.GetComponent<Bullet>().position = _target.position;
            bullet_obj.GetComponent<Bullet>().damage = bulletDamage;
            StartCoroutine(ShootCooldown());
            _anim.SetBool("isSwing", false);
        }
    }

    protected IEnumerator ShootCooldown()
    {
        yield return new WaitForSecondsRealtime(shootCooldown);
        _canShoot = true;
    }

    public void Stunned() {
        isStunned = true;
        PlayStunParticles();
        StartCoroutine(RecoverFromStun());
    }

    protected IEnumerator RecoverFromStun() {
        yield return new WaitForSecondsRealtime(stunnedTime);
        isStunned = false;
        StopStunParticles();
    }
}
