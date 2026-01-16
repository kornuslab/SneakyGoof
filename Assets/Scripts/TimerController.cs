using TMPro;
using UnityEngine;

public class TimerController : MonoBehaviour
{
    [Tooltip("Timer in seconds.")]
    [SerializeField] private float timer;
    [Tooltip("TimerText is in the GamePlayUI Prefab")]
    [SerializeField] private TMP_InputField timerText;
    private float currentTimer;
    private bool timerEnabled = true;

    private void Start()
    {
        currentTimer = timer;
    }

    private void Update()
    {
        EnableTimerControl();
        if (!timerEnabled) return;

        if (currentTimer < 0)
        {
            GameManager.singleton.OnLose();
            Debug.Log("You lose because you get out of time!");
        }
        else
        {
            currentTimer -= Time.deltaTime;
            timerText.text = currentTimer.ToString("F2");
        }
    }

    private void EnableTimerControl()
    {
        if (!timerEnabled)
        {
            timerText.gameObject.SetActive(false);
        }
        else
        {
            if (!timerText.gameObject.activeSelf)
                timerText.gameObject.SetActive(true);
        }
    }

    public void EnableTimer(bool enable = true)
    {
        timerEnabled = enable;
    }
}
