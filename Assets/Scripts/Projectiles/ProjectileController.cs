using UnityEngine;
using System;
using System.Collections;

public class ProjectileController : MonoBehaviour
{
    [Header("Lifetime")]
    [Tooltip("Time in seconds before this projectile auto-destroys.")]
    public float lifetime;

    [Header("Piercing")]
    [Tooltip("Number of times this projectile can hit before being destroyed.")]
    public int PierceCount = 0;

    [Tooltip("Allows infinite piercing when true.")]
    public bool infinitePiercing = false;

    [Header("Movement")]
    [Tooltip("Movement logic for this projectile.")]
    public ProjectileMovement movement;
    public event Action<Hittable, Vector3> OnHit;

    void Start()
    {
        if (movement == null)
        {
            movement = GetComponent<ProjectileMovement>();
            if (movement == null)
            {
                Debug.LogWarning("ProjectileController: Missing ProjectileMovement component.");
            }
        }

        if (lifetime > 0)
        {
            StartCoroutine(Expire());
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
            Debug.LogWarning("ProjectileController: Cannot move projectile, movement is null.");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Ignore collisions with other projectiles
        if (collision.gameObject.CompareTag("projectile"))
            return;

        // Handle hitting units
        if (collision.gameObject.CompareTag("unit"))
        {
            var hittable = collision.gameObject.GetComponent<Hittable>()
                           ?? collision.gameObject.GetComponent<PlayerController>()?.hp;
            if (hittable != null)
                OnHit?.Invoke(hittable, transform.position);
        }

        // Determine whether to destroy based on piercing settings
        if (infinitePiercing)
        {
            // never destroy
            return;
        }

        if (PierceCount > 0)
        {
            PierceCount--;
            return;
        }

        // No piercing left — destroy
        Destroy(gameObject);
    }

    public void SetLifetime(float newLifetime)
    {
        lifetime = newLifetime;
        StopAllCoroutines();
        if (lifetime > 0)
            StartCoroutine(Expire());
    }

    IEnumerator Expire()
    {
        yield return new WaitForSeconds(lifetime);
        Destroy(gameObject);
    }
}