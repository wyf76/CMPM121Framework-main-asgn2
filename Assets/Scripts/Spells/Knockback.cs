using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

// Base spell that launches a projectile and applies knockback on hit.

public sealed class KnockbackSpell : Spell
{
    private string _displayName;
    private int    _iconIndex;
    private string _damageExpr;
    private float  _baseManaCost;
    private float  _baseCooldown;
    private string _trajectory;
    private string _speedExpr;
    private int    _projectileSprite;
    private string _forceExpr;

    public KnockbackSpell(SpellCaster owner) : base(owner) { }

    public override string DisplayName => _displayName;
    public override int    IconIndex   => _iconIndex;

    protected override float BaseDamage   =>
        RPNEvaluator.SafeEvaluateFloat(_damageExpr, GetVars(), 8f);
    protected override float BaseMana     => _baseManaCost;
    protected override float BaseCooldown => _baseCooldown;
    protected override float BaseSpeed    =>
        RPNEvaluator.SafeEvaluateFloat(_speedExpr, GetVars(), 10f);

    private Dictionary<string,float> GetVars() => new Dictionary<string,float>
    {
        ["power"] = owner.spellPower,
        ["wave"]  = Object.FindFirstObjectByType<EnemySpawnerController>()?.CurrentWave ?? 1
    };

    public override void LoadAttributes(JObject json, Dictionary<string,float> vars)
    {
        _displayName      = json["name"].Value<string>();
        _iconIndex        = json["icon"].Value<int>();
        _damageExpr       = json["damage"]["amount"].Value<string>();
        _baseManaCost     = RPNEvaluator.SafeEvaluateFloat(json["mana_cost"].Value<string>(), vars, 12f);
        _baseCooldown     = RPNEvaluator.SafeEvaluateFloat(json["cooldown"].Value<string>(), vars, 1.2f);
        _trajectory       = json["projectile"]["trajectory"].Value<string>();
        _speedExpr        = json["projectile"]["speed"].Value<string>();
        _projectileSprite = json["projectile"]["sprite"].Value<int>();
        _forceExpr        = json["effects"][0]["force"].Value<string>();
    }

    protected override IEnumerator Cast(Vector3 origin, Vector3 target)
    {
        float dmg   = Damage;
        float spd   = Speed;
        float force = RPNEvaluator.SafeEvaluateFloat(_forceExpr, GetVars(), 5f);

        GameManager.Instance.projectileManager.CreateProjectile(
            _projectileSprite,
            _trajectory,
            origin,
            (target - origin).normalized,
            spd,
            (hit, pos) =>
            {
                if (hit.team != owner.team)
                {
                    // Damage
                    hit.Damage(new global::Damage(Mathf.RoundToInt(dmg), global::Damage.Type.PHYSICAL));

                    // Knockback
                    var rb = hit.owner.GetComponent<Rigidbody2D>();
                    if (rb != null && rb.bodyType == RigidbodyType2D.Dynamic)
                        rb.AddForce((hit.owner.transform.position - pos).normalized * force,
                                    ForceMode2D.Impulse);
                }
            });

        yield return null;
    }
}