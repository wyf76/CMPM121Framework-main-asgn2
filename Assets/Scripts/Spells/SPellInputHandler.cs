using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerController))]
public class SpellInputHandler : MonoBehaviour
{
    private PlayerController playerController;

    void Awake()
    {
        playerController = GetComponent<PlayerController>();
        if (playerController == null)
        {
            Debug.LogError("PlayerController component not found on this GameObject!");
        }
    }

    void Update()
    {
        if (Input.GetButtonDown("Fire1")) 
        {
            if (playerController == null || playerController.spellcaster == null) 
            {
                Debug.LogWarning("PlayerController or its SpellCaster not ready for input.");
                return;
            }

            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0;

            foreach (var s in playerController.spells) 
            {
                if (s != null && playerController.spellcaster.mana >= s.GetManaCost() && s.IsReady())
                {
                    playerController.spellcaster.mana -= s.GetManaCost();
                    StartCoroutine(s.Cast(transform.position, mouseWorld, playerController.spellcaster.casterTeam));
                    break;
                }
            }
        }
    }

}