using UnityEngine;
using UnityEngine.InputSystem;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    public Hittable hp;
    public HealthBar healthui;
    public ManaBar manaui;

    public SpellCaster spellcaster;
    public SpellUI spellui;

    public int speed;

    public Unit unit;

    public Spell[] spells = new Spell[4]; // max 4 spells
    public SpellUIContainer spellUIContainer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        unit = GetComponent<Unit>();
        GameManager.Instance.player = gameObject;
    }

    public void StartLevel()
    {
        
        spellcaster = new SpellCaster(125, 8, Hittable.Team.PLAYER, 10, 1);
        StartCoroutine(spellcaster.ManaRegeneration());

        Spell startingSpell = new SpellBuilder().Build("arcane_bolt", spellcaster, 1, spellcaster.spell_power);
        spells[0] = startingSpell;
        spellUIContainer.spellUIs[0].SetActive(true);
        spellUIContainer.spellUIs[0].GetComponent<SpellUI>().SetSpell(startingSpell);
        
        hp = new Hittable(100, Hittable.Team.PLAYER, gameObject);
        hp.OnDeath += Die;
        hp.team = Hittable.Team.PLAYER;

        // tell UI elements what to show
        healthui.SetHealth(hp);
        manaui.SetSpellCaster(spellcaster);
        spellui.SetSpell(spellcaster.spell);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnAttack(InputValue value)
    {
        if (GameManager.Instance.state == GameManager.GameState.PREGAME || GameManager.Instance.state == GameManager.GameState.GAMEOVER) return;
            Vector2 mouseScreen = Mouse.current.position.value;
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(mouseScreen);
            mouseWorld.z = 0;

        foreach (var s in spells)
        {
            if (s != null && spellcaster.mana >= s.GetManaCost() && s.IsReady())
            {
                spellcaster.mana -= s.GetManaCost();
                StartCoroutine(s.Cast(transform.position, mouseWorld, Hittable.Team.PLAYER));
            }
        }
    }

    void OnMove(InputValue value)
    {
        if (GameManager.Instance.state == GameManager.GameState.PREGAME || GameManager.Instance.state == GameManager.GameState.GAMEOVER) return;
        unit.movement = value.Get<Vector2>()*speed;
    }

    void Die()
    {
        GameManager.Instance.EndGame(false);  // false = player lost
    }

    public void ApplyWaveStats(int wave)
    {
        int newMaxHP = RPNEvaluatorInt.Evaluate("95 wave 5 * +", wave, 0);
        int newMana = RPNEvaluatorInt.Evaluate("90 wave 10 * +", wave, 0);
        int newRegen = RPNEvaluatorInt.Evaluate("10 wave +", wave, 0);
        int newPower = RPNEvaluatorInt.Evaluate("wave 10 *", wave, 0);
        int newSpeed = RPNEvaluatorInt.Evaluate("5", wave, 0);

        // Let SetMaxHP handle HP scaling with current percent
        hp.SetMaxHP(newMaxHP);

        spellcaster.mana = newMana;
        spellcaster.max_mana = newMana;
        spellcaster.mana_reg = newRegen;
        spellcaster.spell_power = newPower;
        speed = newSpeed;

        Debug.Log($"[Wave {wave}] Stats: HP={newMaxHP}, Mana={newMana}, Regen={newRegen}, Power={newPower}, Speed={newSpeed}");
    }

    public void ReplaceSpell(Spell newSpell)
    {
        // Find first empty slot
        for (int i = 0; i < spells.Length; i++)
        {
            if (spells[i] == null)
            {
                spells[i] = newSpell;
                spellUIContainer.spellUIs[i].SetActive(true);
                spellUIContainer.spellUIs[i].GetComponent<SpellUI>().SetSpell(newSpell);
                return;
            }
        }

        // Otherwise replace first spell (or show UI selection if you prefer)
        spells[0] = newSpell;
        spellUIContainer.spellUIs[0].GetComponent<SpellUI>().SetSpell(newSpell);
    }

    public void DropSpellAt(int index)
    {
        if (index < 0 || index >= spells.Length || spells[index] == null) return;

        spells[index] = null;
        spellUIContainer.spellUIs[index].SetActive(false);
    }
}
