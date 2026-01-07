using UnityEngine;
using System;

public class NoiseEmitter : MonoBehaviour
{
    [SerializeField] private float baseNoiseLevel = 2f;
    [SerializeField] private SoundData stepSoundData;
    public void MakeNoise(float multiplier = 1f)
    {
        AudioManager.Instance.Play(stepSoundData, SoundType.Player);
        float noise = baseNoiseLevel * multiplier;
        Eye_Behaviour.OnNoiseEmitted?.Invoke(transform.position, noise);
        // Debug.Log("Noise emitted at position: " + transform.position + " with intensity: " + noise);
    }
}
