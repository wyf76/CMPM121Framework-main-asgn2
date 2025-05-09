using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

// KnockbackSpell: launches a projectile that deals damage and pushes enemies back on hit.
public sealed class KnockbackSpell : Spell
{
    //JSON-loaded spell fields
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
    protected override float BaseSpeed    =>
        RPNEvaluator.SafeEvaluateFloat(_speedExpr, GetVars(), 10f);
    protected override float BaseMana     => _baseManaCost;
    protected override float BaseCooldown => _baseCooldown;

    // Prepare variables for RPN evaluation
    private Dictionary<string, float> GetVars()
    {
        return new Dictionary<string, float>
        {
            ["power"] = owner.spellPower,
            ["wave"]  = Object.FindFirstObjectByType<EnemySpawnerController>()?.currentWave ?? 1
        };
    }

    // Load JSON attributes
    public override void LoadAttributes(JObject json, Dictionary<string, float> vars)
    {
        // Basic metadata
        _displayName      = json.Value<string>("name")       ?? _displayName;
        _iconIndex        = json.Value<int?>("icon")         ?? _iconIndex;

        // Damage expression
        _damageExpr       = json["damage"]?["amount"]?.Value<string>() ?? _damageExpr;

        // Mana & cooldown
        _baseManaCost     = RPNEvaluator.SafeEvaluateFloat(json.Value<string>("mana_cost"), vars, _baseManaCost);
        _baseCooldown     = RPNEvaluator.SafeEvaluateFloat(json.Value<string>("cooldown"),  vars, _baseCooldown);

        // Projectile visuals and speed
        _trajectory       = json["projectile"]?.Value<string>("trajectory") ?? _trajectory;
        _speedExpr        = json["projectile"]?["speed"]?.Value<string>()       ?? _speedExpr;
        _projectileSprite = json["projectile"]?.Value<int?>("sprite")           ?? _projectileSprite;

        // Read knockback force from effects array
        if (json.TryGetValue("effects", out JToken effTok) && effTok is JArray effects)
        {
            foreach (var e in effects)
            {
                if (e.Value<string>("type") == "knockback")
                {
                    _forceExpr = e.Value<string>("force") ?? _forceExpr;
                    break;
                }
            }
        }
    }

    // Cast logic: spawn one projectile, deal damage, then apply knockback impulse
    protected override IEnumerator Cast(Vector3 origin, Vector3 target)
    {
        float damageValue = Damage;
        float speedValue  = Speed;
        float forceValue  = !string.IsNullOrWhiteSpace(_forceExpr)
            ? RPNEvaluator.SafeEvaluateFloat(_forceExpr, GetVars(), 5f)
            : 5f;

        GameManager.Instance.projectileManager.CreateProjectile(
            _projectileSprite,
            _trajectory,
            origin,
            (target - origin).normalized,
            speedValue,
            (hit, hitPos) =>
            {
                if (hit.team != owner.team)
                {
                    int dmgAmt = Mathf.RoundToInt(damageValue);
                    hit.Damage(new global::Damage(dmgAmt, global::Damage.Type.PHYSICAL));
                    Debug.Log($"[{_displayName}] Hit {hit.owner.name} for {dmgAmt}, knockback {forceValue:F1}");

                    var rb = hit.owner.GetComponent<Rigidbody2D>();
                    if (rb != null && rb.bodyType == RigidbodyType2D.Dynamic)
                    {
                        Vector2 dir = (hit.owner.transform.position - hitPos).normalized;
                        rb.AddForce(dir * forceValue, ForceMode2D.Impulse);
                    }
                }
            }
        );

        yield return null;
    }
}
