using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void PlayGame()
    {
        Debug.Log("Play");
        SceneManager.LoadScene(1);
    }
    public void QuitGame()
    {
        Debug.Log("Exit");
        Application.Quit();
    }
}
