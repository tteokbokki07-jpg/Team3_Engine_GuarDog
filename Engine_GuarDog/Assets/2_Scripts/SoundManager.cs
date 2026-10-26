using UnityEngine;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    public AudioSource audioSource;
    public AudioClip titletextSound;
    public AudioClip revieveSound;
    public AudioClip hitSound;
    public AudioClip playerhitSound;
    public AudioClip homehitSound;
    public AudioClip popSound;
    public AudioClip swingSound;
    public AudioClip enemyshootSound;
    public AudioClip cardSound;
    public AudioClip cardSelectSound;
    public AudioClip completeSound;
    [Range(0f, 1f)]
    public float masterVolume = 1f; // 전체 효과음 음량
    public Slider volumeSlider;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        volumeSlider.value = SoundManager.Instance.masterVolume;

        volumeSlider.onValueChanged.AddListener(SetVolume);
    }
    public void SetVolume(float volume)
    {
        SoundManager.Instance.masterVolume = volume;
    }

    public void PlaySFX(AudioClip clip, float volume = 0.6f, float pitch = 1f)
    {
        if (clip == null)
            return;

        audioSource.pitch = pitch;
        audioSource.PlayOneShot(clip, volume * masterVolume);
    }
}