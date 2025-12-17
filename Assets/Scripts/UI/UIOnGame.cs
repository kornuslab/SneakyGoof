using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class UIOnGame : MonoBehaviour
{
    [SerializeField] private int menuIndexScene;
    [SerializeField] private int nextLevelIndexScene;

    public void BackToMenu()
    {
        SceneManager.LoadScene(menuIndexScene);
    }
    public void LoadThisLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void LoadNextLevel()
    {
        SceneManager.LoadScene(nextLevelIndexScene);
    }
    public void Continue()
    {
        GameManager.singleton.OnPause();
    }
}
