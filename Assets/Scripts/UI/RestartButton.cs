using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartButton : MonoBehaviour
{
    // This method shows up automatically in the Button’s OnClick dropdown:
    public void OnRestartPressed()
    {
        // Either reload the active scene:
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        // —or load a named scene:
        // SceneManager.LoadScene("Main");
    }
}
