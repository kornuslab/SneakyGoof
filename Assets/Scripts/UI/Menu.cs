using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    [SerializeField] private int indexSceneToLoad = 0;
    private void Start()
    {
        EventSystem.current.SetSelectedGameObject(transform.GetChild(0).GetChild(0).gameObject);
    }
    public void OnButtonPlay()
    {
        SceneManager.LoadScene(indexSceneToLoad);
    }
    public void OnButtonQuit()
    {
        Application.Quit();
    }
}