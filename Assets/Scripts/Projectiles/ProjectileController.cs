using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic; // For List (to track hit targets)

public class ProjectileController : MonoBehaviour
{
    // public float lifetime; // Lifetime is now set via SetLifetime
    public event Action<Hittable, Vector3> OnHit;
    public ProjectileMovement movement;

    // --- NEW FIELDS FOR PIERCE ---
    private SpellData spellData; // To access effects like pierce
    private int pierceCountMax = 0;
    private int currentPierceCount = 0;
    private List<GameObject> hitTargets; // To prevent hitting the same target multiple times with one pierce

    void Awake() // Changed from Start to Awake for earlier initialization if needed
    {
        hitTargets = new List<GameObject>();
    }

    // --- NEW METHOD ---
    public void InitializeSpellData(SpellData sData)
    {
        this.spellData = sData;
        if (this.spellData != null && this.spellData.effects != null)
        {
            EffectData pierceEffect = this.spellData.effects.Find(e => e.type.ToLower() == "pierce");
            if (pierceEffect != null && !string.IsNullOrEmpty(pierceEffect.count))
            {
                // Assuming wave and power are not directly needed for pierce_count here,
                // or that they are pre-calculated into the string if it's simple.
                // If pierce_count is an RPN string needing wave/power, this needs SpellCaster context.
                // For simplicity, let's assume pierceEffect.count is a simple integer string for now.
                int.TryParse(pierceEffect.count, out pierceCountMax);
            }
        }
    }

    void Update()
    {
        if (movement != null)
        {
            movement.Movement(transform);
        }
        else
        {
            Debug.LogWarning("ProjectileController: Movement strategy not set!");
            // Destroy(gameObject); // Optionally destroy if it can't move
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Prevent hitting other projectiles or non-hittable things unless specified
        if (collision.gameObject.CompareTag("projectile")) return; // Example tag

        Hittable hittableComponent = collision.gameObject.GetComponent<Hittable>();
        if (hittableComponent == null) // Also try EnemyController or PlayerController if direct Hittable isn't there
        {
            EnemyController ec = collision.gameObject.GetComponent<EnemyController>();
            if (ec != null) hittableComponent = ec.hp;
            else
            {
                PlayerController pc = collision.gameObject.GetComponent<PlayerController>();
                if (pc != null) hittableComponent = pc.hp;
            }
        }


        if (hittableComponent != null)
        {
            // Prevent hitting the same target multiple times with the same piercing projectile
            if (hitTargets.Contains(collision.gameObject))
            {
                return; // Already hit this target
            }
            hitTargets.Add(collision.gameObject);

            OnHit?.Invoke(hittableComponent, collision.contacts[0].point); // Invoke the hit event

            currentPierceCount++;
            if (currentPierceCount > pierceCountMax) // pierceCountMax is "additional pierces"
            {
                Destroy(gameObject); // Destroy after max pierces reached
                return;
            }
            // If it pierces, it continues. Some games reduce damage on subsequent pierces.
        }
        else
        {
            // Hit something not hittable (like a wall)
            // For some spells, you might want OnHit to trigger anyway (e.g. AoE on wall impact)
            // For now, let's assume only hitting Hittable objects matters most, or destroy on any other solid collision
            if (!collision.gameObject.CompareTag("Player") && !collision.gameObject.CompareTag("Untagged")) // Avoid destroying on hitting player layer if it's a player projectile, or untagged boundaries
            {
                 // If it's not a trigger collider, it implies a solid object.
                if (!collision.collider.isTrigger) {
                    // Debug.Log("Projectile hit a non-hittable solid object: " + collision.gameObject.name);
                    OnHit?.Invoke(null, collision.contacts[0].point); // Invoke with null hittable for effects like AoE on impact.
                    Destroy(gameObject);
                }
            }
        }
    }

    public void SetLifetime(float newLifetime) // Renamed parameter for clarity
    {
        // lifetime = newLifetime; // Not strictly needed if only used in coroutine
        StartCoroutine(Expire(newLifetime));
    }

    IEnumerator Expire(float duration) // Renamed parameter
    {
        yield return new WaitForSeconds(duration);
        if (this.gameObject != null) // Check if not already destroyed
        {
            Destroy(gameObject);
        }
    }
}