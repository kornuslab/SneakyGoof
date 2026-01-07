using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Timeline;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager singleton;
    [SerializeField] private PlayerController PlayerController;
    [SerializeField] private Eye_Behaviour EyeController;
    [SerializeField] private GameObject GameOverPanel;
    [SerializeField] private GameObject WinPanel;
    [SerializeField] private GameObject PausePanel;
    [SerializeField] private float WinDistance;
    [Header("SoundDatas")]
    [SerializeField] private SoundData winSoundData;
    [SerializeField] private SoundData loseSoundData;
    private bool alreadyWon = false;
    private bool alreadyLost;

    public bool pause { get; private set; }

    void Start()
    {
        if (singleton == null)
        {
            singleton = this;
        }
        else if (singleton != this)
        {
            Destroy(gameObject);
        }
 
        alreadyWon = false;
    }

    void Update()
    {
        if (WinConditions() && !alreadyWon)
        {
            alreadyWon = true;
            AudioManager.Instance.Play(winSoundData, SoundType.UI);
            OnWin();
        }
    }

    public void OnPause()
    {
        pause = !pause;
        PausePanel.SetActive(pause);
        if (pause)
        {
            SetSelectedGameObject(PausePanel.transform.GetChild(0).GetChild(0).gameObject);
        }
        PauseTheGame(pause);
    }

    private bool WinConditions()
    {
        return Vector3.Distance(PlayerController.transform.position, EyeController.transform.position) < WinDistance;
    }
    public void OnWin()
    {
        PauseTheGame(true);
        WinPanel.SetActive(true);
        SetSelectedGameObject(WinPanel.transform.GetChild(0).GetChild(0).gameObject);
    }

    public void OnLose()
    {
        if (!alreadyLost)
        {
            AudioManager.Instance.Play(loseSoundData, SoundType.UI);
        }
        alreadyLost = true;
        PauseTheGame(true);
        GameOverPanel.SetActive(true);
        SetSelectedGameObject(GameOverPanel.transform.GetChild(0).GetChild(0).gameObject);
    }

    private void PauseTheGame(bool _pause)
    {
        pause = _pause;
        PlayerController.enabled = !_pause;
        EyeController.enabled = !_pause;
    }

    public void SetSelectedGameObject(GameObject gameObject)
    {
        EventSystem.current.SetSelectedGameObject(gameObject);
    }

    public void ResetParams()
    {
        alreadyLost = false;
        alreadyWon = false;
    }
}
