using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public Hittable hp;
    public HealthBar healthui;
    public ManaBar manaui;
    public SpellCaster spellcaster;

    public SpellUI spellui, spellui2, spellui3, spellui4;
    public int speed;
    private Unit unit;

    void Awake()
    {
        unit = GetComponent<Unit>();
        GameManager.Instance.player = gameObject;
    }

    public void StartLevel()
    {
        if (spellcaster == null)
            spellcaster = GetComponent<SpellCaster>() ?? gameObject.AddComponent<SpellCaster>();

        hp = new Hittable(100, Hittable.Team.PLAYER, gameObject);
        hp.OnDeath += Die;

        spellcaster.team = Hittable.Team.PLAYER;

        healthui.SetHealth(hp);
        manaui.SetSpellCaster(spellcaster);

        UpdateSpellUI();
    }

    public void UpdateSpellUI()
    {
        var spells = spellcaster?.spells;
        SpellUI[] slots = { spellui, spellui2, spellui3, spellui4 };

        for (int i = 0; i < slots.Length; i++)
        {
            var slot = slots[i];
            if (slot == null) continue;

            bool hasSpell = spells != null && i < spells.Count && spells[i] != null;
            slot.SetSpell(hasSpell ? spells[i] : null);
            slot.gameObject.SetActive(hasSpell);
        }
    }

    void OnAttack(InputValue value)
    {
        var state = GameManager.Instance.state;
        if (state == GameManager.GameState.PREGAME || state == GameManager.GameState.GAMEOVER)
            return;
        if (spellcaster == null) return;

        Vector2 ms = Mouse.current.position.ReadValue();
        Vector3 mw = Camera.main.ScreenToWorldPoint(ms);
        mw.z = 0;

        for (int i = 0; i < spellcaster.spells.Count; i++)
        {
            var sp = spellcaster.spells[i];
            if (sp != null)
                StartCoroutine(spellcaster.CastSlot(i, transform.position, mw));
        }
    }

    void OnMove(InputValue value)
    {
        var state = GameManager.Instance.state;
        if (state == GameManager.GameState.PREGAME || state == GameManager.GameState.GAMEOVER)
            return;

        unit.movement = value.Get<Vector2>() * speed;
    }

    void Die()
    {
        Debug.Log("Player died");
        GameManager.Instance.IsPlayerDead = true;
        GameManager.Instance.state = GameManager.GameState.GAMEOVER;
    }
}
