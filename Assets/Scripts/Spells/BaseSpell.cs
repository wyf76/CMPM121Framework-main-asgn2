using UnityEngine;
using System.Collections;

public class BaseSpell : Spell
{
    private SpellData data;
    private int wave;
    private int power;

    public BaseSpell(SpellCaster owner, SpellData data, int wave, int power) : base(owner)
    {
        this.data = data;
        this.wave = wave;
        this.power = power;
    }

    public override IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team)
{
    int manaCost = RPNEvaluatorInt.Evaluate(data.mana_cost, wave, power);
    if (owner.mana < manaCost) yield break;

    owner.mana -= manaCost;

    float speed = RPNEvaluatorFloat.Evaluate(data.projectile.speed, wave, power);
    int damage = RPNEvaluatorInt.Evaluate(data.damage.amount, wave, power);
    string trajectory = data.projectile.trajectory;
    int sprite = data.projectile.sprite;

    GameManager.Instance.projectileManager.CreateProjectile(
        sprite, trajectory, where, target - where, speed,
        (Hittable other, Vector3 impact) =>
        {
            if (other.team != team)
            {
                other.Damage(new Damage(damage, Damage.Type.ARCANE));
            }
        });

    yield return new WaitForEndOfFrame();
}

    public override int GetManaCost() =>
        RPNEvaluatorInt.Evaluate(data.mana_cost, wave, power);

    public override int GetDamage() =>
        RPNEvaluatorInt.Evaluate(data.damage.amount, wave, power);

    public override float GetCooldown() =>
        RPNEvaluatorFloat.Evaluate(data.cooldown, wave, power);

    public override int GetIcon() => data.icon;
}
