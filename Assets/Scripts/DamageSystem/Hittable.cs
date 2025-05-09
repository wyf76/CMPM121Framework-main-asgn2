using UnityEngine;
using System;

public class Hittable
{
    public enum Team { PLAYER, MONSTERS }
    public Team team;

    public int hp;
    public int max_hp;

    public GameObject owner; 

    public void Damage(Damage damage)
    {
        if (owner == null) 
        {
            Debug.LogError("Hittable.Damage: Owner GameObject is null!");
            return;
        }
        EventBus.Instance.DoDamage(owner.transform.position, damage, this);
        hp -= damage.amount;
        if (hp <= 0)
        {
            hp = 0;
            OnDeath?.Invoke();
        }
    }

    public event Action OnDeath;
    public event Action<int> OnHeal; 

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

    public void Heal(int amount)
    {
        if (amount <= 0) return; // Don't heal for non-positive amounts

        hp += amount;
        if (hp > max_hp)
        {
            hp = max_hp;
        }
        OnHeal?.Invoke(amount); 
    }
}