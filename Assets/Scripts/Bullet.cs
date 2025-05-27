using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public Vector3 position;
    public int damage;
    public float speed = 50f;

    private void Update() {
        float step = speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, position, step);
        if (transform.position == position) Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Obstacle") || other.CompareTag("Door")) {
            Destroy(gameObject);
            return;
        }

        if (damage < 6) {
            if (other.TryGetComponent<Player>(out var player)) {
                speed = damage > 0 ? 50f : 10f;

                if (damage > 0) player.GetDamage(damage);
                else player.Stunned();

                Destroy(gameObject);
            }
        }
        else if (other.CompareTag("Enemy")) {
            System.Type[] enemyTypes = { typeof(MeleeEnemy), typeof(RangeEnemy), typeof(SpecialEnemy) };

            foreach (var type in enemyTypes) {
                var enemy = other.GetComponent(type);
                if (enemy != null) {
                    speed = damage > 0 ? 50f : 10f;

                    if (damage > 0) {
                        type.GetMethod("GetDamage").Invoke(enemy, new object[] { damage, true });
                    } else {
                        type.GetMethod("Stunned").Invoke(enemy, null);
                    }
                    Destroy(gameObject);
                    break;
                }
            }
        }
    }
}
