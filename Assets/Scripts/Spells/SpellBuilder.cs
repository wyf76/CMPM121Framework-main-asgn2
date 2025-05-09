using UnityEngine;
<<<<<<< HEAD
<<<<<<< HEAD
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;


// Chooses a base spell and random modifiers based on the current wave.
=======
using System.Collections.Generic;
>>>>>>> 1e7a3e8 (some updates)


public static class SpellBuilder
{
<<<<<<< HEAD
    private readonly Dictionary<string, JObject> catalog;
    private readonly System.Random rng = new System.Random();

    static readonly string[] BaseKeys     = {"arcane_bolt","arcane_spray","magic_missile","arcane_blast", "knockback_bolt"};
    static readonly string[] ModifierKeys = {"splitter","doubler","damage_amp","speed_amp","chaos","homing","frost_spike_modifier","vampiric_essence_modifier"};

    public SpellBuilder()
    {
        var ta = Resources.Load<TextAsset>("spells");
        catalog = ta != null
            ? JsonConvert.DeserializeObject<Dictionary<string,JObject>>(ta.text)
            : new Dictionary<string,JObject>();
    }

    public Spell Build(SpellCaster owner)
    {
        int wave = GetCurrentWave();
        var vars = new Dictionary<string,float> { ["power"]=owner.spellPower, ["wave"]=wave };

        if (wave <= 1)
            return BuildBase(owner, "arcane_bolt", vars);

        // Pick random base
        int bidx = rng.Next(BaseKeys.Length);
        Spell s = BuildBase(owner, BaseKeys[bidx], vars);

        // Random number of modifiers
        int modCount = rng.NextDouble() < 0.3 ? 2 : rng.Next(2);
        for (int i = 0; i < modCount; i++)
            s = ApplyModifier(s, ModifierKeys[rng.Next(ModifierKeys.Length)], vars);

        return s;
    }

    private Spell BuildBase(SpellCaster owner, string key, Dictionary<string,float> vars)
    {
        Spell s = key switch
        {
            "arcane_bolt"     => new ArcaneBolt(owner),
            "arcane_spray"    => new ArcaneSpray(owner),
            "magic_missile"   => new MagicMissile(owner),
            "arcane_blast"    => new ArcaneBlast(owner),
            "knockback_bolt"  => new KnockbackSpell(owner),  
            _                 => new ArcaneBolt(owner)
        };

        if (catalog.TryGetValue(key, out var json))
            s.LoadAttributes(json, vars);

        return s;
    }


    private Spell ApplyModifier(Spell inner, string mkey, Dictionary<string,float> vars)
    {
        Spell mod = mkey switch
        {
            "splitter"                 => new Splitter(inner),
            "doubler"                  => new Doubler(inner),
            "damage_amp"               => new DamageMagnifier(inner),
            "speed_amp"                => new SpeedModifier(inner),
            "chaos"                    => new ChaoticModifier(inner),
            "homing"                   => new HomingModifier(inner),
            "vampiric_essence_modifier"=> new VampiricEssenceModifier(inner),
            "frost_spike_modifier"     => new FrostSpikeModifier(inner),
            _                          => inner
        };

        if (catalog.TryGetValue(mkey, out var json))
            mod.LoadAttributes(json, vars);

        return mod;
    }
    private int GetCurrentWave()
    {
        var sp = UnityEngine.Object.FindFirstObjectByType<EnemySpawnerController>();
        return sp != null ? sp.CurrentWave : 1;
=======

    public static Spell GenerateRandomSpell(int currentWave, int playerSpellPower)
=======
using System;
using System.Collections.Generic;

public static class SpellBuilder
{
    static List<SpellData> defs;
    public static void Init(string json)
>>>>>>> 22ff77c (getting there)
    {
        defs = JsonUtility.FromJson<SpellDataList>(json).spells;
    }

    public static Spell BuildRandom(int wave, int power)
    {
        if (defs == null || defs.Count == 0)
        {
            Debug.LogError("SpellBuilder: No spell definitions loaded.");
            return null;
        }
        var vars = new Dictionary<string, float> { { "wave", wave }, { "power", power } };
        var d = defs[UnityEngine.Random.Range(0, defs.Count)];
        if (IsModifier(d))
        {
            Spell inner = BuildRandom(wave, power);
            return BuildMod(d, inner, vars);
        }
        return BuildBase(d, vars);
    }

    static bool IsModifier(SpellData d) => d.damage == null;

    static Spell BuildBase(SpellData d, Dictionary<string, float> v)
    {
        int dmg = RPNEvaluator.EvalInt(d.damage.amount, v);
        int cost = RPNEvaluator.EvalInt(d.mana_cost, v);
        float cd = RPNEvaluator.Eval(d.cooldown, v);
        Damage.Type dt;
        Enum.TryParse(d.damage.type, true, out dt);
        int N = string.IsNullOrEmpty(d.N) ? 1 : RPNEvaluator.EvalInt(d.N, v);

        return new BaseSpell(
            d.name, d.description, d.icon,
            dmg,
            string.IsNullOrEmpty(d.secondary_damage) ? 0 : RPNEvaluator.EvalInt(d.secondary_damage, v),
            cost, cd, dt,
            d.projectile.trajectory,
            RPNEvaluator.Eval(d.projectile.speed, v),
            string.IsNullOrEmpty(d.projectile.lifetime) ? 0f : float.Parse(d.projectile.lifetime),
            d.projectile.sprite,
            N
        );
    }

    static Spell BuildMod(SpellData d, Spell inner, Dictionary<string, float> v)
    {
        ValueModifier dm = null, mm = null, sm = null;
        if (!string.IsNullOrEmpty(d.damage_multiplier))
            dm = new ValueModifier(ModType.Multiplicative, RPNEvaluator.Eval(d.damage_multiplier, v));
        if (!string.IsNullOrEmpty(d.mana_multiplier))
            mm = new ValueModifier(ModType.Multiplicative, RPNEvaluator.Eval(d.mana_multiplier, v));
        if (!string.IsNullOrEmpty(d.speed_multiplier))
            sm = new ValueModifier(ModType.Multiplicative, RPNEvaluator.Eval(d.speed_multiplier, v));

        bool split = d.name.ToLower().Contains("split");
        bool dbl = d.name.ToLower().Contains("double");
        float del = dbl && !string.IsNullOrEmpty(d.delay) ? RPNEvaluator.Eval(d.delay, v) : 0f;

<<<<<<< HEAD
        // Primary projectile properties
        string trajectory = (data.projectile != null) ? data.projectile.trajectory : "straight";
        float speed = (data.projectile != null && !string.IsNullOrEmpty(data.projectile.speed))
                        ? RPNEvaluator.Evaluate(data.projectile.speed, vars) : 0f;
        float lifetime = (data.projectile != null && !string.IsNullOrEmpty(data.projectile.lifetime))
                        ? RPNEvaluator.Evaluate(data.projectile.lifetime, vars) : 0f;
        int sprite = (data.projectile != null) ? data.projectile.sprite : 0;

        int projCount = 1;
        int secondaryCount = 0;
        if (!string.IsNullOrEmpty(data.N))
        {
            int Nval = Mathf.RoundToInt(RPNEvaluator.Evaluate(data.N, vars));
            if (data.secondary_projectile != null)
            {
                secondaryCount = Nval;   // N is number of secondary projectiles on hit
            }
            else
            {
                projCount = Nval;       // N is number of projectiles to spawn initially
            }
        }

        BaseSpell baseSpell = new BaseSpell(data.name, data.description, data.icon,
                                           baseDamage, secondaryDamage,
                                           manaCost, cooldown, damageType,
                                           trajectory, speed, lifetime, sprite,
                                           projCount, secondaryCount);
        return baseSpell;
    }

    private static Spell BuildModifierSpell(SpellData data, Spell innerSpell, int wave, int power)
    {
        Dictionary<string, float> vars = new Dictionary<string, float> { { "wave", wave }, { "power", power } };

        // Prepare value modifiers for damage, mana, and speed
        ValueModifier dmgMod = null;
        if (!string.IsNullOrEmpty(data.damage_multiplier))
        {
            float factor = RPNEvaluator.Evaluate(data.damage_multiplier, vars);
            dmgMod = new ValueModifier(ValueModifier.ModType.Multiplicative, factor);
        }
        if (!string.IsNullOrEmpty(data.damage_add))
        {
            float addVal = RPNEvaluator.Evaluate(data.damage_add, vars);

            if (dmgMod == null)
                dmgMod = new ValueModifier(ValueModifier.ModType.Additive, addVal);
            else
                Debug.Log("Damage add and multiply both present - they will be applied in sequence.");
        }
        ValueModifier manaMod = null;
        if (!string.IsNullOrEmpty(data.mana_multiplier))
        {
            float mFactor = RPNEvaluator.Evaluate(data.mana_multiplier, vars);
            manaMod = new ValueModifier(ValueModifier.ModType.Multiplicative, mFactor);
        }
        if (!string.IsNullOrEmpty(data.mana_add))
        {
            float mAdd = RPNEvaluator.Evaluate(data.mana_add, vars);
            if (manaMod == null)
                manaMod = new ValueModifier(ValueModifier.ModType.Additive, mAdd);
            else
                Debug.Log("Mana add and multiply both present - they will be applied in sequence.");
        }
        ValueModifier speedMod = null;
        if (!string.IsNullOrEmpty(data.speed_multiplier))
        {
            float sFactor = RPNEvaluator.Evaluate(data.speed_multiplier, vars);
            speedMod = new ValueModifier(ValueModifier.ModType.Multiplicative, sFactor);
        }

        SpellData.ProjectileInfo projOverride = data.projectile;

        bool splitter = data.name.ToLower().Contains("split");
        bool doubler = data.name.ToLower().Contains("double") && !splitter;
        float delay = 0f;
        if (doubler)
        {
            delay = 0.2f;
        }

        // Create the ModifierSpell instance
        ModifierSpell modSpell = new ModifierSpell(data.name, data.description, data.icon,
                                                  innerSpell,
                                                  dmgMod, manaMod, speedMod,
                                                  projOverride,
                                                  splitter, doubler, delay);
        return modSpell;
>>>>>>> 1e7a3e8 (some updates)
=======
        return new ModifierSpell(
            d.name, d.description, d.icon,
            inner, dm, mm, sm,
            split, dbl, del
        );
>>>>>>> 22ff77c (getting there)
    }
}

[System.Serializable]
class SpellDataList { public List<SpellData> spells; }