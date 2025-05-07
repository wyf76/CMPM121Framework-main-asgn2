using UnityEngine;

public class DropButton : MonoBehaviour
{
    public int index; // set this in Inspector

    public void OnClickDrop()
    {
        GameManager.Instance.player.GetComponent<PlayerController>().DropSpellAt(index);
    }
}
