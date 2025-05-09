using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public sealed class ArcaneSpray : Spell
{
    //JSON-loaded spell attributes
    private string _displayName;
    private string _description;
    private int _iconIndex;
    private float _baseManaCost;
    private float _baseCooldownTime;
    private string _trajectory;
    private int _projectileSprite;

    //RPN expressions and spray settings
    private string _damageExpression;
    private string _speedExpression;
    private string _lifetimeExpression;
    private string _countExpression;
    private float _sprayAngleDegrees;

    public ArcaneSpray(SpellCaster owner) : base(owner) { }

    public override string DisplayName => _displayName;
    public override int IconIndex => _iconIndex;

    protected override float BaseDamage => RPNEvaluator.SafeEvaluateFloat(_damageExpression, GetVars(), 3f);
    protected override float BaseSpeed => RPNEvaluator.SafeEvaluateFloat(_speedExpression, GetVars(), 8f);
    protected override float BaseMana => _baseManaCost;
    protected override float BaseCooldown => _baseCooldownTime;

    // Prepare variables for RPN evaluation
    private Dictionary<string, float> GetVars()
    {
        return new Dictionary<string, float>
        {
            ["power"] = owner.spellPower,
            ["wave"]  = GetCurrentWave()
        };
    }

    // Retrieve the current wave, defaulting to 1 if not found
    private float GetCurrentWave()
    {
        var spawner = Object.FindFirstObjectByType<EnemySpawnerController>();
        return spawner != null ? spawner.currentWave : 1f;
    }

    public override void LoadAttributes(JObject json, Dictionary<string, float> initialVars)
    {
        // Identity fields
        _displayName = json.Value<string>("name");
        _description = json.Value<string>("description") ?? string.Empty;
        _iconIndex   = json.Value<int>("icon");

        // Damage and speed expressions
        _damageExpression   = json["damage"]["amount"].Value<string>();
        _speedExpression    = json["projectile"]["speed"].Value<string>();
        _lifetimeExpression = json["projectile"].Value<string>("lifetime");
        _countExpression    = json.Value<string>("N") ?? _countExpression ?? "7";

        // Mana and cooldown
        _baseManaCost     = RPNEvaluator.SafeEvaluateFloat(json.Value<string>("mana_cost"), initialVars, _baseManaCost);
        _baseCooldownTime = RPNEvaluator.SafeEvaluateFloat(json.Value<string>("cooldown"),  initialVars, _baseCooldownTime);

        // Projectile visuals
        _trajectory = json["projectile"].Value<string>("trajectory");
        _projectileSprite = json["projectile"].Value<int?>("sprite") ?? _projectileSprite;

        // Spray cone angle (in degrees)
        if (json.TryGetValue("spray", out var sprayToken) && float.TryParse(sprayToken.Value<string>(), out var sprayVal))
        {
            _sprayAngleDegrees = sprayVal;
        }
        else
        {
            _sprayAngleDegrees = 60f;
        }
    }

    protected override IEnumerator Cast(Vector3 origin, Vector3 targetPosition)
    {
        // Evaluate dynamic parameters
        int count       = Mathf.RoundToInt(RPNEvaluator.SafeEvaluateFloat(_countExpression, GetVars(), 7f));
        float lifetime  = RPNEvaluator.SafeEvaluateFloat(_lifetimeExpression, GetVars(), 0.5f);
        float damageVal = Damage;
        float speedVal  = Speed;

        Debug.Log($"[{_displayName}] Spray ▶ dmg={damageVal:F1}, spd={speedVal:F1}, life={lifetime:F2}, cnt={count}");

        // Calculate spray angles
        Vector3 dir = (targetPosition - origin).normalized;
        float baseAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        float step = (count > 1) ? _sprayAngleDegrees / (count - 1) : 0f;
        float start = baseAngle - (_sprayAngleDegrees * 0.5f);

        for (int i = 0; i < count; i++)
        {
            float angle = start + step * i;
            Vector3 projDir = new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad),
                0f
            );

            // Spawn the projectile
            GameManager.Instance.projectileManager.CreateProjectile(
                _projectileSprite,
                _trajectory,
                origin,
                projDir,
                speedVal,
                (hit, _) =>
                {
                    if (hit.team != owner.team)
                    {
                        int amt = Mathf.RoundToInt(damageVal);
                        hit.Damage(new global::Damage(amt, global::Damage.Type.ARCANE));
                        Debug.Log($"[{_displayName}] Hit {hit.owner.name} for {amt}");
                    }
                },
                lifetime
            );

            // Slight delay between spawns for visual effect
            yield return new WaitForSeconds(0.02f);
        }

        yield return null;
    }
}
