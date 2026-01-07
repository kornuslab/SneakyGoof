using UnityEngine;
using UnityEngine.Audio;
public enum SoundType
{
    UI,
    Player,
    Eye,
    Music
}
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Sources")]
    [Tooltip("Click, Noise Threshold, GameOver, Win")]
    [SerializeField] private AudioSource uiSource;
    [SerializeField] private AudioSource sfxPlayerSource;
    [SerializeField] private AudioSource sfxEyeSource;
    [SerializeField] private AudioSource musicSource;
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    public void Play(SoundData sound, SoundType type)
    {
        AudioSource src = GetSource(type);
        if (!src) return;
        src.pitch = sound.pitch;
        src.PlayOneShot(sound.clip, sound.volume);
    }

    public void SetVolume(float volume, SoundType type)
    {
        AudioSource src = GetSource(type);
        if (!src) return;
        src.volume = volume;
    }

    public void Stop(SoundType type)
    {
        AudioSource src = GetSource(type);
        if (!src) return;
        src.Stop();
    }

    private AudioSource GetSource(SoundType type)
    {
        switch (type)
        {
            case SoundType.UI:
                return uiSource;
            case SoundType.Player:
                return sfxPlayerSource;
            case SoundType.Eye:
                return sfxEyeSource;
            case SoundType.Music:
                return musicSource;
        }
        return null;
    }
}
