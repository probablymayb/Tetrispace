using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// 게임잼용 초간단 AudioManager (AudioMixerGroup 없는 버전)
/// 
/// 🎯 특징:
/// - AudioMixerGroup 의존성 제거
/// - 더 빠른 설정과 사용
/// - 게임잼에 최적화
/// 
/// 📋 사용법:
/// 1. PlayOneShot(clip) - 효과음 재생
/// 2. PlayBGM(clip) - 배경음 재생
/// 3. SetMasterVolume(volume) - 볼륨 조절
/// </summary>
public class SFXManager : Singleton<SFXManager>
{
    [Header("볼륨 설정")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Range(0f, 1f)] public float bgmVolume = 1f;

    [Header("3D 사운드 설정")]
    [SerializeField] private float defaultMinDistance = 1f;
    [SerializeField] private float defaultMaxDistance = 500f;

    [Header("디버그")]
    [SerializeField] private bool showDebugInfo = false;

    // 원샷 사운드용 AudioSource 풀
    private Queue<AudioSource> audioSourcePool = new Queue<AudioSource>();
    private List<AudioSource> activeAudioSources = new List<AudioSource>();

    // 루핑 사운드 관리
    private Dictionary<string, AudioSource> loopingSounds = new Dictionary<string, AudioSource>();

    // 오디오 소스 부모 오브젝트
    private Transform audioSourceParent;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);

        // 오디오 소스들을 정리할 부모 오브젝트 생성
        GameObject audioParent = new GameObject("AudioSources");
        audioParent.transform.SetParent(transform);
        audioSourceParent = audioParent.transform;

        // 초기 오디오 소스 풀 생성
        CreateInitialAudioSourcePool(10);
    }

    void Update()
    {
        // 완료된 원샷 사운드들 정리
        CleanupCompletedAudioSources();
    }

    void OnDestroy()
    {
        StopAllSounds();
    }

    #region 게임잼용 간편 메서드들

    /// <summary>
    /// 🎯 게임잼용: 가장 간단한 효과음 재생
    /// </summary>
    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        PlayOneShot(clip, volume);
    }

    /// <summary>
    /// 🎯 게임잼용: 배경음악 재생 (기존 BGM 자동 교체)
    /// </summary>
    public void PlayBGM(AudioClip clip, float volume = 1f, float fadeTime = 1f)
    {
        // 기존 BGM 중지
        StopLoopingSound("BGM", fadeTime);

        // 새 BGM 재생
        if (clip != null)
        {
            StartCoroutine(PlayBGMDelayed(clip, volume, fadeTime));
        }
    }

    /// <summary>
    /// 🎯 게임잼용: BGM 중지
    /// </summary>
    public void StopBGM(float fadeTime = 1f)
    {
        StopLoopingSound("BGM", fadeTime);
    }

    /// <summary>
    /// 🎯 게임잼용: 3D 효과음 (오브젝트 위치에서)
    /// </summary>
    public void PlaySFXAt(AudioClip clip, GameObject target, float volume = 1f)
    {
        if (target != null)
        {
            PlayOneShot3D(clip, target.transform.position, volume);
        }
        else
        {
            PlaySFX(clip, volume);
        }
    }

    private IEnumerator PlayBGMDelayed(AudioClip clip, float volume, float delay)
    {
        yield return new WaitForSeconds(delay);
        PlayLooping(clip, "BGM", volume, delay);
    }

    #endregion

    #region 원샷 사운드 재생

    /// <summary>
    /// 2D 원샷 사운드 재생
    /// </summary>
    public AudioSource PlayOneShot(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null)
        {
            Debug.LogWarning("AudioManager: 오디오 클립이 null입니다");
            return null;
        }

        AudioSource audioSource = GetAudioSource();

        // 2D 사운드 설정
        audioSource.clip = clip;
        audioSource.volume = sfxVolume * masterVolume * volumeScale;
        audioSource.spatialBlend = 0f; // 2D
        audioSource.loop = false;

        audioSource.Play();
        activeAudioSources.Add(audioSource);

        return audioSource;
    }

    /// <summary>
    /// 3D 원샷 사운드 재생
    /// </summary>
    public AudioSource PlayOneShot3D(AudioClip clip, Vector3 position, float volumeScale = 1f,
                                     float minDistance = -1f, float maxDistance = -1f)
    {
        if (clip == null)
        {
            Debug.LogWarning("AudioManager: 오디오 클립이 null입니다");
            return null;
        }

        AudioSource audioSource = GetAudioSource();

        // 3D 사운드 설정
        audioSource.transform.position = position;
        audioSource.clip = clip;
        audioSource.volume = sfxVolume * masterVolume * volumeScale;
        audioSource.spatialBlend = 1f; // 3D
        audioSource.loop = false;

        // 거리 설정
        audioSource.minDistance = minDistance > 0 ? minDistance : defaultMinDistance;
        audioSource.maxDistance = maxDistance > 0 ? maxDistance : defaultMaxDistance;
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;

        audioSource.Play();
        activeAudioSources.Add(audioSource);

        return audioSource;
    }

    #endregion

    #region 루핑 사운드 관리

    /// <summary>
    /// 루핑 사운드 재생 (배경음악용)
    /// </summary>
    public AudioSource PlayLooping(AudioClip clip, string key, float volumeScale = 1f, float fadeInTime = 0f)
    {
        if (clip == null)
        {
            Debug.LogWarning("AudioManager: 오디오 클립이 null입니다");
            return null;
        }

        // 기존 루핑 사운드가 있으면 중지
        StopLoopingSound(key);

        AudioSource audioSource = GetAudioSource();

        // 루핑 사운드 설정
        audioSource.clip = clip;
        audioSource.volume = bgmVolume * masterVolume * volumeScale;
        audioSource.spatialBlend = 0f; // 2D (BGM은 보통 2D)
        audioSource.loop = true;

        audioSource.Play();
        loopingSounds[key] = audioSource;

        // 페이드인 효과
        if (fadeInTime > 0f)
        {
            StartCoroutine(FadeIn(audioSource, bgmVolume * masterVolume * volumeScale, fadeInTime));
        }

        return audioSource;
    }

    /// <summary>
    /// 루핑 사운드 중지
    /// </summary>
    public void StopLoopingSound(string key, float fadeOutTime = 0f)
    {
        if (loopingSounds.TryGetValue(key, out AudioSource audioSource))
        {
            if (fadeOutTime > 0f)
            {
                StartCoroutine(FadeOutAndStop(audioSource, fadeOutTime));
            }
            else
            {
                audioSource.Stop();
                ReturnAudioSource(audioSource);
            }

            loopingSounds.Remove(key);
        }
    }

    /// <summary>
    /// 모든 루핑 사운드 중지
    /// </summary>
    public void StopAllLoopingSounds(float fadeOutTime = 0f)
    {
        var keys = new List<string>(loopingSounds.Keys);
        foreach (string key in keys)
        {
            StopLoopingSound(key, fadeOutTime);
        }
    }

    #endregion

    #region 볼륨 제어

    /// <summary>
    /// 마스터 볼륨 설정
    /// </summary>
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateAllVolumes();
    }

    /// <summary>
    /// SFX 볼륨 설정
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
    }

    /// <summary>
    /// BGM 볼륨 설정
    /// </summary>
    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        UpdateBGMVolumes();
    }

    private void UpdateAllVolumes()
    {
        UpdateBGMVolumes();
    }

    private void UpdateBGMVolumes()
    {
        foreach (var audioSource in loopingSounds.Values)
        {
            if (audioSource != null)
            {
                audioSource.volume = bgmVolume * masterVolume;
            }
        }
    }

    #endregion

    #region 일시정지/재개

    /// <summary>
    /// 모든 사운드 일시정지
    /// </summary>
    public void PauseAllSounds()
    {
        foreach (var audioSource in activeAudioSources)
        {
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Pause();
            }
        }

        foreach (var audioSource in loopingSounds.Values)
        {
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Pause();
            }
        }
    }

    /// <summary>
    /// 모든 사운드 재개
    /// </summary>
    public void ResumeAllSounds()
    {
        foreach (var audioSource in activeAudioSources)
        {
            if (audioSource != null && !audioSource.isPlaying && audioSource.time > 0)
            {
                audioSource.UnPause();
            }
        }

        foreach (var audioSource in loopingSounds.Values)
        {
            if (audioSource != null && !audioSource.isPlaying)
            {
                audioSource.UnPause();
            }
        }
    }

    /// <summary>
    /// 모든 사운드 완전 중지
    /// </summary>
    public void StopAllSounds()
    {
        foreach (var audioSource in activeAudioSources)
        {
            if (audioSource != null)
            {
                audioSource.Stop();
            }
        }
        activeAudioSources.Clear();

        StopAllLoopingSounds();
        ReturnAllAudioSources();
    }

    #endregion

    #region AudioSource 풀 관리

    private void CreateInitialAudioSourcePool(int count)
    {
        for (int i = 0; i < count; i++)
        {
            CreateNewAudioSource();
        }
    }

    private AudioSource CreateNewAudioSource()
    {
        GameObject audioObject = new GameObject("PooledAudioSource");
        audioObject.transform.SetParent(audioSourceParent);

        AudioSource audioSource = audioObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        audioSourcePool.Enqueue(audioSource);
        return audioSource;
    }

    private AudioSource GetAudioSource()
    {
        if (audioSourcePool.Count == 0)
        {
            CreateNewAudioSource();
        }

        AudioSource audioSource = audioSourcePool.Dequeue();
        audioSource.gameObject.SetActive(true);

        // 기본값으로 초기화
        audioSource.clip = null;
        audioSource.volume = 1f;
        audioSource.pitch = 1f;
        audioSource.spatialBlend = 0f;
        audioSource.loop = false;
        audioSource.transform.position = Vector3.zero;

        return audioSource;
    }

    private void ReturnAudioSource(AudioSource audioSource)
    {
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.clip = null;
            audioSource.gameObject.SetActive(false);
            audioSourcePool.Enqueue(audioSource);
        }
    }

    private void ReturnAllAudioSources()
    {
        foreach (var audioSource in activeAudioSources)
        {
            ReturnAudioSource(audioSource);
        }
        activeAudioSources.Clear();

        foreach (var audioSource in loopingSounds.Values)
        {
            ReturnAudioSource(audioSource);
        }
        loopingSounds.Clear();
    }

    private void CleanupCompletedAudioSources()
    {
        for (int i = activeAudioSources.Count - 1; i >= 0; i--)
        {
            if (i >= activeAudioSources.Count) continue;

            AudioSource audioSource = activeAudioSources[i];

            if (audioSource == null || !audioSource.isPlaying)
            {
                if (audioSource != null)
                {
                    ReturnAudioSource(audioSource);
                }
                activeAudioSources.RemoveAt(i);
            }
        }
    }

    #endregion

    #region 코루틴 헬퍼

    private IEnumerator FadeIn(AudioSource audioSource, float targetVolume, float fadeTime)
    {
        audioSource.volume = 0f;
        float currentTime = 0f;

        while (currentTime < fadeTime && audioSource != null)
        {
            currentTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, targetVolume, currentTime / fadeTime);
            yield return null;
        }

        if (audioSource != null)
        {
            audioSource.volume = targetVolume;
        }
    }

    private IEnumerator FadeOutAndStop(AudioSource audioSource, float fadeTime)
    {
        float startVolume = audioSource.volume;
        float currentTime = 0f;

        while (currentTime < fadeTime && audioSource != null)
        {
            currentTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, currentTime / fadeTime);
            yield return null;
        }

        if (audioSource != null)
        {
            audioSource.Stop();
            ReturnAudioSource(audioSource);
        }
    }

    #endregion

    #region 디버그

    void OnGUI()
    {
        if (!showDebugInfo) return;

        GUILayout.BeginArea(new Rect(10, 10, 300, 250));
        GUILayout.Label("=== 게임잼 AudioManager ===");
        GUILayout.Label($"마스터 볼륨: {masterVolume:F2}");
        GUILayout.Label($"SFX 볼륨: {sfxVolume:F2}");
        GUILayout.Label($"BGM 볼륨: {bgmVolume:F2}");
        GUILayout.Label($"활성 사운드: {activeAudioSources.Count}");
        GUILayout.Label($"루핑 사운드: {loopingSounds.Count}");

        GUILayout.Space(10);

        if (GUILayout.Button("모든 사운드 중지"))
        {
            StopAllSounds();
        }

        if (GUILayout.Button("BGM 중지"))
        {
            StopBGM();
        }

        GUILayout.EndArea();
    }

    #endregion
}