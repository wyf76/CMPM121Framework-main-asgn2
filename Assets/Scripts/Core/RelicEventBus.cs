using UnityEngine;
using System;

public static class RelicEventBus
{
    public static event Action<int> OnPlayerDamaged;
    public static event Action<GameObject> OnEnemyKilled;
    public static event Action OnPlayerMoved;
    public static event Action<float> OnPlayerMovedDistance; // New event
    public static event Action OnSpellMissed; // New event

    public static void PlayerTookDamage(int amount)
    {
        OnPlayerDamaged?.Invoke(amount);
    }

    public static void EnemyKilled(GameObject obj)
    {
        OnEnemyKilled?.Invoke(obj);
    }

    public static void PlayerMoved()
    {
        OnPlayerMoved?.Invoke();
    }
    
    // New method to invoke the move distance event
    public static void PlayerMovedDistance(float distance)
    {
        OnPlayerMovedDistance?.Invoke(distance);
    }

    // New method to invoke the spell missed event
    public static void SpellMissed()
    {
        OnSpellMissed?.Invoke();
    }
}