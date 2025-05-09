using UnityEngine;
using System;

public class ProjectileManager : MonoBehaviour
{
    public GameObject[] projectiles; 

    public static ProjectileManager Instance { get; private set; }

    private void Awake()
    {
        // so that Instance is non‑null when we call it elsewhere
        Instance = this;
    }
    
    void Start()
    {
        GameManager.Instance.projectileManager = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CreateProjectile(int prefabIndex, string trajectory, Vector3 spawnPosition, Vector3 direction, float speed, float lifetime, Action<Hittable, Vector3> onHitCallback)
    {
        GameObject new_projectile = Instantiate(projectiles[prefabIndex], spawnPosition + direction.normalized * 1.1f, Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg));
        new_projectile.GetComponent<ProjectileController>().movement = MakeMovement(trajectory, speed);
        new_projectile.GetComponent<ProjectileController>().OnHit += onHitCallback;
        new_projectile.GetComponent<ProjectileController>().SetLifetime(lifetime);
    }

    public ProjectileMovement MakeMovement(string trajectoryName, float speed)
    {
        if (string.IsNullOrEmpty(trajectoryName)) trajectoryName = "straight";
        string lowerName = trajectoryName.ToLower();

        if (lowerName == "straight")
        {

            return new StraightProjectileMovement(speed);
        }
        if (lowerName == "homing")
        {

            return new HomingProjectileMovement(speed); 
        }
        if (lowerName == "spiraling")
        {

            return new SpiralingProjectileMovement(speed); 
        }
        Debug.LogWarning($"ProjectileManager.MakeMovement: Unknown trajectory type '{trajectoryName}'. Defaulting to Straight.");
        return new StraightProjectileMovement(speed); 
    }
}