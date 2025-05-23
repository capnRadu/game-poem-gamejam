using UnityEngine;
using UnityEngine.SceneManagement;

public class Scene_Switch: MonoBehaviour
{
    public void scene_changer(string scene_name)
    {
        SceneManager.LoadScene(scene_name);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit");
    }
}
