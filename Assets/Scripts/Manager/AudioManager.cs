using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Range(0f, 1f)] [SerializeField] private float masterVolume = 1f;
    [Range(0f, 1f)] [SerializeField] private float musicVolume = 0.8f;
    [Range(0f, 1f)] [SerializeField] private float sfxVolume = 1f;

    private Coroutine _fadeRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.spatialBlend = 0f;
        }

        ApplyVolumes();
    }

    public void PlayMusic(AudioClip clip, float fadeDuration = 0.4f)
    {
        if (clip == null)
        {
            return;
        }

        if (_fadeRoutine != null)
        {
            StopCoroutine(_fadeRoutine);
        }

        _fadeRoutine = StartCoroutine(FadeMusicRoutine(clip, fadeDuration));
    }

    public void PlaySfx(AudioClip clip, Vector3? worldPosition = null)
    {
        if (clip == null)
        {
            return;
        }

        if (worldPosition.HasValue)
        {
            AudioSource.PlayClipAtPoint(clip, worldPosition.Value, masterVolume * sfxVolume);
            return;
        }

        sfxSource.PlayOneShot(clip, masterVolume * sfxVolume);
    }

    public void SetVolumes(float master, float music, float sfx)
    {
        masterVolume = Mathf.Clamp01(master);
        musicVolume = Mathf.Clamp01(music);
        sfxVolume = Mathf.Clamp01(sfx);
        ApplyVolumes();
    }

    private IEnumerator FadeMusicRoutine(AudioClip nextClip, float duration)
    {
        var start = musicSource.volume;
        var timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(start, 0f, timer / duration);
            yield return null;
        }

        musicSource.clip = nextClip;
        musicSource.Play();

        timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0f, masterVolume * musicVolume, timer / duration);
            yield return null;
        }

        ApplyVolumes();
        _fadeRoutine = null;
    }

    private void ApplyVolumes()
    {
        if (musicSource != null)
        {
            musicSource.volume = masterVolume * musicVolume;
        }

        if (sfxSource != null)
        {
            sfxSource.volume = masterVolume * sfxVolume;
        }
    }
}
