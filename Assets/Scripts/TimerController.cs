using TMPro;
using UnityEngine;

public class TimerController : MonoBehaviour
{
    [Tooltip("Timer in seconds.")]
    [SerializeField] private float timer;
    [Tooltip("TimerText is in the GamePlayUI Prefab")]
    [SerializeField] private TMP_InputField timerText;
    private float currentTimer;

    private void Start()
    {
        currentTimer = timer;
    }

    private void Update()
    {
        if (currentTimer < 0)
        {
            GameManager.singleton.OnLose();
        }
        else
        {
            currentTimer -= Time.deltaTime;
            timerText.text = currentTimer.ToString("F2");
        }
    }
}
