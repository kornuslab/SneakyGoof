using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Timeline;
using UnityEngine.UI;

public enum Controller
{
    Xbox,
    Playstation,
    Keyboard
}

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
    private Camera mainCamera;
    private bool alreadyWon = false;
    private bool alreadyLost;
    public bool pause { get; private set; }
    public Controller currentController = Controller.Playstation;
    void Awake()
    {
        if (singleton == null)
        {
            singleton = this;
        }
        else if (singleton != this)
        {
            Debug.LogWarning("Multiple instances of GameManager detected. Destroying duplicate.");
            Destroy(gameObject);
        }
 
        alreadyWon = false;

        mainCamera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
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

    public void SetGameOnTutoMode(bool tutoMode = true)
    {
        EnableCameraMode(!tutoMode);
        EnableNoise(!tutoMode);
        EnableTimer(!tutoMode);
    }

    public void EnableNoise(bool enable = true)
    {
        if (PlayerController.GetComponent<NoiseEmitter>() == null)
        {
            Debug.LogWarning("Player does not have NoiseEmitter component.");
            return;
        } 
        PlayerController.GetComponent<NoiseEmitter>().EnableNoise(enable);

                

        if (EyeController == null)
        {
            Debug.LogWarning("No Eye_Behaviour found in the GameManager gameObject.");
        }
        else
        {
            EyeController.EnableNoise(enable);
        }
    }

    public void EnableCameraMode(bool enable = true)
    {
        if (mainCamera.GetComponent<CameraMovement>() == null)
        {
            Debug.LogWarning("Current camera does not have CameraMovement component.");
        }
        else
        {
            mainCamera.GetComponent<CameraMovement>().EnableCameraMode(enable);    
        } 
    }

    public void EnableTimer(bool enable = true)
    {
        TimerController timerController = GetComponent<TimerController>();
        if (timerController == null)
        {
            Debug.LogWarning("No TimerController found in the GameManager gameObject.");
            return;
        }
        timerController.EnableTimer(enable);
    }
}