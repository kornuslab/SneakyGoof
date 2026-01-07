using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using Unity.VisualScripting;

public class UIOnGame : MonoBehaviour
{
    [SerializeField] private int menuIndexScene;
    [SerializeField] private int nextLevelIndexScene;
    [Header("SoundDatas")]
    [SerializeField] private SoundData click;

    public void BackToMenu()
    {
        SceneManager.LoadScene(menuIndexScene);
        PlayClick();
    }
    public void LoadThisLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        GameManager.singleton.ResetParams();
        PlayClick();
    }
    public void LoadNextLevel()
    {
        SceneManager.LoadScene(nextLevelIndexScene);
        PlayClick();
    }
    public void Continue()
    {
        GameManager.singleton.OnPause();
        PlayClick();
    }

    private void PlayClick()
    {
        AudioManager.Instance.Play(click, SoundType.UI);
    }
}
