using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MeleeEnemy : Enemy
{
    Animator _anim;
    private NavMeshAgent _agent;
    private NavMeshPath _path;
    public Transform hitPoint;
    public float hitRadius = 3f;
    public int damage = 2;
    public float swingTime = 2f;
    public float reloadTime = 0.5f;
    private bool isReloaded = true;
    public AudioClip swingSound;

    protected override void Start()
    {
        base.Start();
        _agent = GetComponent<NavMeshAgent>();
        _path = new NavMeshPath();
        _anim = GetComponent<Animator>();
    }

    protected override void Act()
    {
        _agent.CalculatePath(_target.position, _path);
        _agent.SetDestination(_target.position);
        _anim.SetBool("isRun", _agent.velocity.magnitude > 0.1f);

        if (isReloaded && !isStunned) {
            Collider[] colliders = Physics.OverlapSphere(hitPoint.position, hitRadius);
            for (int i = 0; i < colliders.Length; i++) {
                if (colliders[i].TryGetComponent(out Player player)) {
                    _agent.isStopped = true;
                    _agent.velocity = Vector3.zero;
                    StartCoroutine(Swing());
                    sfxSource.PlayOneShot(swingSound);
                }
            }
            StartCoroutine(Reloaded());
        }
    }

    private IEnumerator Swing() {
        _anim.SetBool("isSwing", true);
        yield return new WaitForSecondsRealtime(swingTime);
        _anim.SetBool("isSwing", false);
        yield return new WaitForSecondsRealtime(1f);
        Collider[] colliders = Physics.OverlapSphere(hitPoint.position, hitRadius);
        for (int i = 0; i < colliders.Length; i++) {
            if (colliders[i].TryGetComponent(out Player player)) {
                player.GetDamage(damage);
            }
        }
        _agent.isStopped = false;
    }

    private IEnumerator Reloaded() {
        isReloaded = false;
        yield return new WaitForSecondsRealtime(reloadTime);
        isReloaded = true;
    }

    public void Stunned() {
        _anim.SetBool("isSwing", false);
        StopAllCoroutines();

        isStunned = true;
        _agent.isStopped = true;
        PlayStunParticles();
        StartCoroutine(RecoverFromStun());
    }

    private IEnumerator RecoverFromStun() {
        yield return new WaitForSecondsRealtime(stunnedTime);
        isStunned = false;
        _agent.isStopped = false;
        StopStunParticles();
    }
}
