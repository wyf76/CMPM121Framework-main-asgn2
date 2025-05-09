using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public sealed class ArcaneBlast : Spell
{
    // JSON-loaded spell metadata
    private string _displayName;
    private string _description;
    private int _iconIndex;
    private float _baseManaCost;
    private float _baseCooldownTime;
    private string _primaryTrajectory;
    private int _primaryProjectileSprite;

    // RPN expressions for dynamic stats
    private string _damageExpression;
    private string _speedExpression;
    private string _secondaryDamageExpression;
    private string _secondaryCountExpression;

    // Secondary projectile settings
    private string _secondaryTrajectory;
    private float _secondarySpeed;
    private float _secondaryLifetime;
    private int _secondaryProjectileSprite;

    public ArcaneBlast(SpellCaster owner) : base(owner) { }

    public override string DisplayName => _displayName;
    public override int IconIndex => _iconIndex;

    protected override float BaseDamage =>
        RPNEvaluator.SafeEvaluateFloat(_damageExpression, GetEvaluationVariables(), 20f);

    protected override float BaseSpeed =>
        RPNEvaluator.SafeEvaluateFloat(_speedExpression, GetEvaluationVariables(), 12f);

    protected override float BaseMana => _baseManaCost;
    protected override float BaseCooldown => _baseCooldownTime;

    // Collect variables for RPN evaluation
    private Dictionary<string, float> GetEvaluationVariables()
    {
        return new Dictionary<string, float>
        {
            ["power"] = owner.spellPower,
            ["wave"]  = GetCurrentWave()
        };
    }

    // Retrieve current wave from EnemySpawner, defaulting to 1
    private float GetCurrentWave()
    {
        var spawner = Object.FindFirstObjectByType<EnemySpawnerController>();
        return spawner != null ? spawner.currentWave : 1f;
    }

    // Load JSON attributes into fields
    public override void LoadAttributes(JObject json, Dictionary<string, float> initialVars)
    {
        // Basic metadata
        _displayName = json.Value<string>("name");
        _description = json.Value<string>("description") ?? string.Empty;
        _iconIndex   = json.Value<int>("icon");

        // Primary stats expressions
        _damageExpression = json["damage"]["amount"].Value<string>();
        _speedExpression  = json["projectile"]["speed"].Value<string>();

        // Mana & cooldown
        _baseManaCost     = RPNEvaluator.SafeEvaluateFloat(json.Value<string>("mana_cost"), initialVars, _baseManaCost);
        _baseCooldownTime = RPNEvaluator.SafeEvaluateFloat(json.Value<string>("cooldown"),  initialVars, _baseCooldownTime);

        // Primary projectile settings
        _primaryTrajectory       = json["projectile"].Value<string>("trajectory");
        _primaryProjectileSprite = json["projectile"].Value<int?>("sprite") ?? _primaryProjectileSprite;

        // Secondary settings
        _secondaryCountExpression  = json.Value<string>("N");
        _secondaryDamageExpression = json.Value<string>("secondary_damage");

        if (json.TryGetValue("secondary_projectile", out JToken secToken) && secToken is JObject sec)
        {
            _secondaryTrajectory       = sec.Value<string>("trajectory") ?? _primaryTrajectory;
            _secondarySpeed            = sec.TryGetValue("speed", out JToken spd)
                ? RPNEvaluator.SafeEvaluateFloat(spd.Value<string>(), initialVars, BaseSpeed * 0.8f)
                : BaseSpeed * 0.8f;
            _secondaryLifetime         = sec.TryGetValue("lifetime", out JToken lt)
                ? float.Parse(lt.Value<string>())
                : 0.3f;
            _secondaryProjectileSprite = sec.Value<int?>("sprite") ?? _primaryProjectileSprite;
        }
        else
        {
            // Defaults if no secondary_projectile section
            _secondaryTrajectory       = _primaryTrajectory;
            _secondarySpeed            = BaseSpeed * 0.8f;
            _secondaryLifetime         = 0.3f;
            _secondaryProjectileSprite = _primaryProjectileSprite;
        }
    }

    // Cast the spell: primary and secondary projectiles
    protected override IEnumerator Cast(Vector3 origin, Vector3 targetPosition)
    {
        // Evaluate final values once at cast time
        float primaryDamage = Damage;
        float secondaryDamage = !string.IsNullOrEmpty(_secondaryDamageExpression)
            ? RPNEvaluator.SafeEvaluateFloat(_secondaryDamageExpression, GetEvaluationVariables(), primaryDamage * 0.25f)
            : primaryDamage * 0.25f;
        int secondaryCount = !string.IsNullOrEmpty(_secondaryCountExpression)
            ? Mathf.RoundToInt(RPNEvaluator.SafeEvaluateFloat(_secondaryCountExpression, GetEvaluationVariables(), 8f))
            : 8;

        Debug.Log($"[{_displayName}] Casting: dmg={primaryDamage:F1}, spd={Speed:F1}, secDmg={secondaryDamage:F1}x{secondaryCount}");

        // Launch primary projectile
        GameManager.Instance.projectileManager.CreateProjectile(
            _primaryProjectileSprite,
            _primaryTrajectory,
            origin,
            (targetPosition - origin).normalized,
            Speed,
            (hit, impactPos) =>
            {
                if (hit.team != owner.team)
                {
                    int dmgAmt = Mathf.RoundToInt(primaryDamage);
                    hit.Damage(new global::Damage(dmgAmt, global::Damage.Type.ARCANE));
                    Debug.Log($"[{_displayName}] Primary hit {hit.owner.name} for {dmgAmt}");

                    // Spawn secondary shards
                    SpawnSecondaryProjectiles(impactPos, secondaryDamage, secondaryCount);
                }
            }
        );

        yield return null;
    }

    // Spawn secondary projectiles in a circular pattern
    private void SpawnSecondaryProjectiles(Vector3 center, float damage, int count)
    {
        float angleStep = 360f / count;
        for (int i = 0; i < count; i++)
        {
            float angleRad = (i * angleStep) * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad), 0f);

            GameManager.Instance.projectileManager.CreateProjectile(
                _secondaryProjectileSprite,
                _secondaryTrajectory,
                center,
                dir.normalized,
                _secondarySpeed,
                (hit, _) =>
                {
                    if (hit.team != owner.team)
                    {
                        int dmgAmt = Mathf.RoundToInt(damage);
                        hit.Damage(new global::Damage(dmgAmt, global::Damage.Type.ARCANE));
                        Debug.Log($"[{_displayName}] Secondary hit {hit.owner.name} for {dmgAmt}");
                    }
                },
                _secondaryLifetime
            );
        }
    }
}