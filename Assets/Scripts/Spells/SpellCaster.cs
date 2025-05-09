using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Handles mana, spell slots, and the actual casting process.

public class SpellCaster : MonoBehaviour
{
    [Header("Mana Settings")] public int max_mana;
    [Header("Mana Settings")] public int mana;
    [Header("Mana Settings")] public int mana_reg;

    [Header("Team")] public Hittable.Team team;
    [Header("Spells")] public int spellPower;
    [Header("Spells")] public List<Spell> spells = new(4);

    void Awake()
    {
        StartCoroutine(ManaRegeneration());
        InitializeSlots();
    }

    private void InitializeSlots()
    {
        var builder = new SpellBuilder();
        spells[0] = builder.Build(this);
        for (int i = 1; i < spells.Count; i++)
            spells[i] = null;
    }

    private IEnumerator ManaRegeneration()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            mana = Mathf.Min(max_mana, mana + mana_reg);
        }
    }

    public IEnumerator CastSlot(int slot, Vector3 from, Vector3 to)
    {
        if (slot < 0 || slot >= spells.Count) yield break;
        var s = spells[slot];
        if (s == null || !s.IsReady || mana < s.Mana) yield break;

        Debug.Log($"[SpellCaster] Casting {s.DisplayName} (Mana: {mana}/{s.Mana})");
        mana -= Mathf.RoundToInt(s.Mana);
        s.lastCast = Time.time;
        yield return s.TryCast(from, to);
    }
}
