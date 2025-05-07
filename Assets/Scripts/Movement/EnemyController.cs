using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    public Transform target;
    public int speed;
    public int damage;
    public Hittable hp;
    public HealthBar healthui; 
    public bool dead = false;
    public float last_attack;

    public Unit unit;
    private int originalSpeed;
    private bool isSlowed = false;
    private Coroutine slowCoroutine;
    private float attackCooldown = 2f;

    void Start()
    {
        unit = GetComponent<Unit>();
        if (unit == null) Debug.LogError($"EnemyController on {gameObject.name} is missing a Unit component.");

        if (GameManager.Instance != null && GameManager.Instance.player != null)
        {
            target = GameManager.Instance.player.transform;
        }

        if (hp != null)
        {
            hp.OnDeath += Die;
            if (healthui != null) healthui.SetHealth(hp);
        }
        else
        {
            Debug.LogError($"EnemyController on {gameObject.name} does not have hp (Hittable) assigned by Spawner!");
        }
        
        originalSpeed = this.speed;
        dead = false;
    }

    void Update()
    {
        if (dead)
        {
            if (unit != null) unit.movement = Vector2.zero;
            return;
        }

        if (GameManager.Instance.state == GameManager.GameState.GAMEOVER || Time.timeScale == 0f)
        {
            if (unit != null) unit.movement = Vector2.zero;
            return;
        }

        if (target != null && unit != null)
        {
            Vector3 direction = target.position - transform.position;
            float currentAttackRange = 2f;

            if (direction.magnitude > currentAttackRange)
            {
                unit.movement = direction.normalized * this.speed;

            }
            else
            {
                unit.movement = Vector2.zero;

                DoAttack();
            }
        }
        else if (unit != null)
        {
            unit.movement = Vector2.zero;
        }
    }
    
    void DoAttack()
    {
        if (dead || target == null) return;

        if (Time.time > last_attack + attackCooldown)
        {
            last_attack = Time.time;
            PlayerController pc = target.gameObject.GetComponent<PlayerController>();
            if (pc != null && pc.hp != null)
            {
                pc.hp.Damage(new Damage(this.damage, Damage.Type.PHYSICAL)); 
            }
        }
    }

    void Die()
    {
        if (!dead)
        {
            dead = true;
            if (slowCoroutine != null)
            {
                StopCoroutine(slowCoroutine);
                this.speed = originalSpeed;
            }
            isSlowed = false;

            if (GameManager.Instance != null) GameManager.Instance.RemoveEnemy(gameObject);
            Destroy(gameObject);
        }
    }


    public void ApplySlow(float slowPercentage, float duration)
    {
        if (dead) return;

        if (isSlowed && slowCoroutine != null)
        {
            StopCoroutine(slowCoroutine);
        } else if (!isSlowed) {
            originalSpeed = this.speed; // Store original speed only when not already slowed
        }

        this.speed = Mathf.Max(0, Mathf.RoundToInt(originalSpeed * (1f - Mathf.Clamp01(slowPercentage))));
        isSlowed = true;
        
        slowCoroutine = StartCoroutine(RevertSpeedAfterDuration(duration));
    }

    private IEnumerator RevertSpeedAfterDuration(float duration)
    {
        yield return new WaitForSeconds(duration);
        if (!dead) // Only revert if not dead during the slow
        {
            this.speed = originalSpeed;
            isSlowed = false;
        }
        slowCoroutine = null;
    }
}