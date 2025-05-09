using UnityEngine;
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
    {
        SpellLibrary lib = Object.FindAnyObjectByType<SpellLibrary>();
        if (lib == null)
        {
            Debug.LogError("SpellLibrary not found. Ensure spells.json is loaded.");
            return null;
        }
        List<SpellData> allSpells = lib.GetAllSpellData();
        if (allSpells == null || allSpells.Count == 0)
        {
            Debug.LogError("No spell data available.");
            return null;
        }

        // Pick a random spell definition
        SpellData data = allSpells[Random.Range(0, allSpells.Count)];
        if (IsModifier(data))
        {
            // Recursively generate an inner spell until we get a base
            Spell innerSpell = GenerateRandomSpell(currentWave, playerSpellPower);
            if (innerSpell == null || IsModifier(data) == false)
            {
                innerSpell = BuildBaseSpell(data, currentWave, playerSpellPower);
            }
            return BuildModifierSpell(data, innerSpell, currentWave, playerSpellPower);
        }
        else
        {
            return BuildBaseSpell(data, currentWave, playerSpellPower);
        }
    }

    private static bool IsModifier(SpellData data)
    {
        return string.IsNullOrEmpty(data.mana_cost) && (data.damage == null || string.IsNullOrEmpty(data.damage.amount));
    }

    private static BaseSpell BuildBaseSpell(SpellData data, int wave, int power)
    {
        Dictionary<string, float> vars = new Dictionary<string, float> { { "wave", wave }, { "power", power } };

        // Evaluate RPN formulas for base stats
        int baseDamage = 0;
        string damageType = "physical";
        if (data.damage != null && !string.IsNullOrEmpty(data.damage.amount))
        {
            baseDamage = Mathf.RoundToInt(RPNEvaluator.Evaluate(data.damage.amount, vars));
            damageType = data.damage.type;
        }
        int secondaryDamage = 0;
        if (!string.IsNullOrEmpty(data.secondary_damage))
        {
            secondaryDamage = Mathf.RoundToInt(RPNEvaluator.Evaluate(data.secondary_damage, vars));
        }
        int manaCost = 0;
        if (!string.IsNullOrEmpty(data.mana_cost))
        {
            manaCost = Mathf.RoundToInt(RPNEvaluator.Evaluate(data.mana_cost, vars));
        }
        float cooldown = 0f;
        if (!string.IsNullOrEmpty(data.cooldown))
        {
            cooldown = RPNEvaluator.Evaluate(data.cooldown, vars);
        }

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
    }
}
