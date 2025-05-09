using UnityEngine;
using System.Collections;

// Controls enemy behavior: movement toward player, attacks, health, and slow effects.

public class EnemyController : MonoBehaviour
{
    [Header("Combat")]
    public int damage;
    public float attackCooldown = 2f;

    [Header("Movement")]
    public int speed;
    private float currentSpeed;

    [Header("Health")]
    public Hittable hp;
    public HealthBar healthui;

    private Transform playerTransform;
    private float lastAttackTime;
    private bool dead;

    private void Start()
    {
        playerTransform = GameManager.Instance.player.transform;
        currentSpeed = speed;

        // Subscribe to death and health change
        hp.OnDeath += Die;
        hp.OnHealthChanged += (_, __) => healthui.SetHealth(hp);
        healthui.SetHealth(hp);
    }

    private void Update()
    {
        if (dead) return;

        Vector3 direction = playerTransform.position - transform.position;
        if (direction.magnitude < 2f)
        {
            TryAttack();
        }
        else
        {
            // Move towards player
            GetComponent<Unit>().movement = direction.normalized * currentSpeed;
        }
    }

    private void TryAttack()
    {
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            // Damage player
            var playerHp = playerTransform.GetComponent<PlayerController>().hp;
            playerHp.Damage(new Damage(damage, Damage.Type.PHYSICAL));
        }
    }

    // <param name="duration">Duration of slow in seconds.</param>
    // <param name="slowFactor">Multiplier to speed (0 to 1).</param>
    public void ApplySlow(float duration, float slowFactor)
    {
        // Prevent stacking slows
        StopAllCoroutines();
        StartCoroutine(SlowRoutine(duration, slowFactor));
    }

    private IEnumerator SlowRoutine(float duration, float slowFactor)
    {
        currentSpeed = speed * slowFactor;
        yield return new WaitForSeconds(duration);
        currentSpeed = speed;
    }

    private void Die()
    {
        if (dead) return;
        dead = true;
        GameManager.Instance.RemoveEnemy(gameObject);
        Destroy(gameObject);
    }
}
