using UnityEngine;

public class DropSpellButton : MonoBehaviour
{
	public PlayerController player;

	public void DropSpell(int index)
	{
		player.DropSpell(index);
	}
}
