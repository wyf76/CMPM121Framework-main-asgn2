using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

// Fires a straight‐flying bolt whose damage and speed scale via RPN formulas.

public sealed class ArcaneBolt : Spell
{
    private string _displayName;
    private Damage.Type _damageType;
    private int _iconIndex;
    private float _baseManaCost;
    private float _baseCooldown;
    private string _trajectory;
    private int _spriteIndex;
    private string _damageExpression;
    private string _speedExpression;

    public ArcaneBolt(SpellCaster caster) : base(caster) { }

    public override string DisplayName => _displayName;
    public override int IconIndex => _iconIndex;

    protected override float BaseDamage => RPNEvaluator.SafeEvaluateFloat(
        _damageExpression, GetVars(), 10f);
    protected override float BaseSpeed => RPNEvaluator.SafeEvaluateFloat(
        _speedExpression, GetVars(), 8f);
    protected override float BaseMana => _baseManaCost;
    protected override float BaseCooldown => _baseCooldown;

    private Dictionary<string, float> GetVars()
    {
        return new Dictionary<string, float>
        {
            ["power"] = Owner.spellPower,
            ["wave"]  = GetWave()
        };
    }

    private float GetWave()
    {
        var sp = UnityEngine.Object.FindAnyObjectByType<EnemySpawnerController>();
        return sp != null ? sp.currentWave : 1f;
    }

    public override void LoadAttributes(JObject json, Dictionary<string, float> vars)
    {
        _displayName      = json.Value<string>("name");
        _iconIndex        = json.Value<int>("icon");
        _damageExpression = json["damage"]["amount"].Value<string>();
        _damageType       = Enum.Parse<Damage.Type>(
                                json["damage"]["type"].Value<string>(),
                                true);
        _baseManaCost     = RPNEvaluator.SafeEvaluateFloat(
                                json.Value<string>("mana_cost"), vars, _baseManaCost);
        _baseCooldown     = RPNEvaluator.SafeEvaluateFloat(
                                json.Value<string>("cooldown"),  vars, _baseCooldown);
        _trajectory       = json["projectile"]["trajectory"].Value<string>();
        _speedExpression  = json["projectile"]["speed"].Value<string>();
        _spriteIndex      = json["projectile"]["sprite"].Value<int>();
    }

    protected override IEnumerator Cast(Vector3 origin, Vector3 target)
    {
        float dmg   = Damage;
        float spd   = Speed;
        Vector3 dir = (target - origin).normalized;

        Debug.Log($"[{_displayName}] Bolt → dmg={dmg:F1}, spd={spd:F1}");

        GameManager.Instance.projectileManager.CreateProjectile(
            _spriteIndex,
            _trajectory,
            origin,
            dir,
            spd,
            (hit, _) =>
            {
                if (hit.team != Owner.team)
                {
                    int amount = Mathf.RoundToInt(dmg);
                    hit.Damage(new global::Damage(amount, _damageType));
                    Debug.Log($"[{_displayName}] Hit {hit.owner.name} for {amount} ({_damageType})");
                }
            });

        yield return null;
    }
}