using UnityEngine;
using System;

public class ProjectileManager : MonoBehaviour
{
    public GameObject[] projectiles; 

    public static ProjectileManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (GameManager.Instance != null)
        {

        }
    }

    public void CreateProjectile(int prefabIndex, string trajectory, Vector3 spawnPosition, Vector3 direction, float speed, Action<Hittable, Vector3> onHitCallback)
    {
        if (projectiles == null || prefabIndex < 0 || prefabIndex >= projectiles.Length || projectiles[prefabIndex] == null)
        {
            Debug.LogError($"ProjectileManager (6-arg): Invalid prefabIndex {prefabIndex} or projectile prefab not set.");
            return;
        }
        Vector3 normalizedDirection = direction.normalized;
        if (normalizedDirection == Vector3.zero) normalizedDirection = Vector3.up;
        GameObject newProjectileGO = Instantiate(projectiles[prefabIndex], 
                                                 spawnPosition + normalizedDirection * 1.1f, 
                                                 Quaternion.Euler(0, 0, Mathf.Atan2(normalizedDirection.y, normalizedDirection.x) * Mathf.Rad2Deg - 90f));
        ProjectileController projController = newProjectileGO.GetComponent<ProjectileController>();
        if (projController != null)
        {
            projController.movement = MakeMovement(trajectory, speed, normalizedDirection);
            projController.OnHit += onHitCallback;
        }
        else
        {
            Debug.LogError($"ProjectileManager: Prefab '{projectiles[prefabIndex].name}' is missing ProjectileController script!");
            Destroy(newProjectileGO);
        }
    }


    public void CreateProjectile(int prefabIndex, string trajectory, Vector3 spawnPosition, Vector3 direction, float speed, Action<Hittable, Vector3> onHitCallback, float lifetime)
    {
        if (projectiles == null || prefabIndex < 0 || prefabIndex >= projectiles.Length || projectiles[prefabIndex] == null)
        {
            Debug.LogError($"ProjectileManager (7-arg): Invalid prefabIndex {prefabIndex} or projectile prefab not set.");
            return;
        }
        Vector3 normalizedDirection = direction.normalized;
        if (normalizedDirection == Vector3.zero) normalizedDirection = Vector3.up;
        GameObject newProjectileGO = Instantiate(projectiles[prefabIndex], 
                                                 spawnPosition + normalizedDirection * 1.1f, 
                                                 Quaternion.Euler(0, 0, Mathf.Atan2(normalizedDirection.y, normalizedDirection.x) * Mathf.Rad2Deg - 90f));
        ProjectileController projController = newProjectileGO.GetComponent<ProjectileController>();
        if (projController != null)
        {
            projController.movement = MakeMovement(trajectory, speed, normalizedDirection);
            projController.OnHit += onHitCallback;
            projController.SetLifetime(lifetime);
        }
        else
        {
            Debug.LogError($"ProjectileManager: Prefab '{projectiles[prefabIndex].name}' is missing ProjectileController script!");
            Destroy(newProjectileGO);
        }
    }

    public void CreateProjectile(
        int prefabIndex,              
        string trajectory,
        Vector3 spawnPosition,
        Vector3 direction,
        float speed,
        float lifetime,
        Action<Hittable, Vector3> onHitCallback,
        SpellData spellDataForProjectile
    )
    {
        if (projectiles == null || prefabIndex < 0 || prefabIndex >= projectiles.Length || projectiles[prefabIndex] == null)
        {
            Debug.LogError($"ProjectileManager (8-arg): Invalid prefabIndex {prefabIndex} for projectiles array. Spell: {spellDataForProjectile?.name}");
            return;
        }

        Vector3 normalizedDirection = direction.normalized;
        if (normalizedDirection == Vector3.zero) {
            normalizedDirection = Vector3.up;
            Debug.LogWarning("CreateProjectile (8-arg) received zero direction. Defaulting to Vector3.up.");
        }

        Vector3 actualSpawnPosition = spawnPosition + normalizedDirection * 1.1f;
        Quaternion spawnRotation = Quaternion.Euler(0, 0, Mathf.Atan2(normalizedDirection.y, normalizedDirection.x) * Mathf.Rad2Deg - 90f);

        // Instantiate the specific prefab using prefabIndex
        GameObject newProjectileGO = Instantiate(projectiles[prefabIndex], actualSpawnPosition, spawnRotation);
        ProjectileController projController = newProjectileGO.GetComponent<ProjectileController>();

        if (projController != null)
        {
            projController.movement = MakeMovement(trajectory, speed, normalizedDirection);
            projController.OnHit += onHitCallback;
            projController.SetLifetime(lifetime);
            projController.InitializeSpellData(spellDataForProjectile);
        }
        else
        {
            Debug.LogError($"ProjectileManager: Prefab '{projectiles[prefabIndex].name}' (for spell '{spellDataForProjectile?.name}') is missing ProjectileController script!");
            Destroy(newProjectileGO);
        }
    }

    public ProjectileMovement MakeMovement(string trajectoryName, float speed, Vector3 initialDirection)
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