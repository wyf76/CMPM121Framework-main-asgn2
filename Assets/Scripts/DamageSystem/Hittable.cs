using UnityEngine;
using System;

public class Hittable
{
    public enum Team { PLAYER, MONSTERS }
    public Team team;

    public int hp;
    public int max_hp;

    public GameObject owner; // The GameObject this Hittable component belongs to

    // Existing Damage method
    public void Damage(Damage damage)
    {
        if (owner == null) // Safety check
        {
            Debug.LogError("Hittable.Damage: Owner GameObject is null!");
            return;
        }
        EventBus.Instance.DoDamage(owner.transform.position, damage, this); // Assuming EventBus and DoDamage exist
        hp -= damage.amount;
        // Debug.Log($"{owner.name} took {damage.amount} {damage.type} damage, HP: {hp}/{max_hp}");
        if (hp <= 0)
        {
            hp = 0;
            OnDeath?.Invoke(); // Use null-conditional operator for safety
        }
    }

    public event Action OnDeath;
    // --- NEW ---
    public event Action<int> OnHeal; // Optional: event for when healing occurs

    public Hittable(int hp, Team team, GameObject owner)
    {
        this.max_hp = hp; // Set max_hp first
        this.hp = hp;     // Then current hp
        this.team = team;
        this.owner = owner;
    }

    public void SetMaxHP(int new_max_hp) // Renamed parameter for clarity
    {
        if (new_max_hp <= 0) new_max_hp = 1; // Prevent division by zero or invalid max_hp
        
        float currentHpPercentage = (max_hp > 0) ? ((float)this.hp / this.max_hp) : 1f;
        this.max_hp = new_max_hp;
        this.hp = Mathf.RoundToInt(currentHpPercentage * this.max_hp);
        this.hp = Mathf.Clamp(this.hp, 0, this.max_hp); // Ensure HP is within bounds
    }

    // --- NEW Heal METHOD ---
    public void Heal(int amount)
    {
        if (amount <= 0) return; // Don't heal for non-positive amounts

        hp += amount;
        if (hp > max_hp)
        {
            hp = max_hp;
        }
        OnHeal?.Invoke(amount); // Invoke optional heal event
        // Debug.Log($"{owner?.name} healed for {amount}, HP: {hp}/{max_hp}");
    }
}