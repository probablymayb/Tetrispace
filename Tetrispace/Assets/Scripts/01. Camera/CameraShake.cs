using UnityEngine;
using Unity.Cinemachine;

public class CameraShake : MonoBehaviour
{
    [Header("Unity 6 - Cinemachine 3 카메라 셰이크")]
    public static CameraShake Instance { get; private set; }

    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private float defaultShakeIntensity = 1f;
    [SerializeField] private float defaultShakeTime = 0.5f;

    private CinemachineBasicMultiChannelPerlin noise;
    private float shakeTimer;
    private float currentIntensity;

    private void Awake()
    {
        // 싱글톤 패턴
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // CinemachineCamera 컴포넌트 가져오기
        if (cinemachineCamera == null)
        {
            cinemachineCamera = GetComponent<CinemachineCamera>();
        }

        if (cinemachineCamera == null)
        {
            Debug.LogError("CinemachineCamera 컴포넌트를 찾을 수 없습니다! " +
                          "GameObject에 CinemachineCamera 컴포넌트를 추가해주세요.");
            return;
        }

        // Noise 컴포넌트 가져오기 (Unity 6/Cinemachine 3 방식)
        SetupNoise();
    }

    private void SetupNoise()
    {
        // Cinemachine 3에서는 Noise를 Extension으로 추가
        noise = cinemachineCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();

        if (noise == null)
        {
            // 자동으로 Noise Extension 추가
            noise = cinemachineCamera.gameObject.AddComponent<CinemachineBasicMultiChannelPerlin>();
            Debug.Log("CinemachineBasicMultiChannelPerlin이 자동으로 추가되었습니다.");
        }

        // 기본 Noise Settings 설정
        if (noise.NoiseProfile == null)
        {
            // 기본 노이즈 프로파일 로드 시도
            var defaultProfile = Resources.Load<NoiseSettings>("6D Shake");
            if (defaultProfile != null)
            {
                noise.NoiseProfile = defaultProfile;
            }
            else
            {
                Debug.LogWarning("기본 Noise Profile을 찾을 수 없습니다. " +
                               "Inspector에서 Noise Profile을 수동으로 설정해주세요.");
            }
        }
    }

    private void Start()
    {
        StopShake();
    }

    private void Update()
    {
        // 테스트용 키 입력
        if (Input.GetKeyDown(KeyCode.L))
        {
            ShakeCamera();
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            ShakeCamera(2f, 1f); // 강한 셰이킹
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            StopShake();
        }

        // 셰이킹 타이머 처리
        if (shakeTimer > 0)
        {
            shakeTimer -= Time.deltaTime;

            // 점진적으로 셰이킹 강도 감소 (선택사항)
            if (noise != null)
            {
                float progress = shakeTimer / defaultShakeTime;
                noise.AmplitudeGain = currentIntensity * progress;
            }

            if (shakeTimer <= 0)
            {
                StopShake();
            }
        }
    }

    /// <summary>
    /// 기본 설정으로 카메라 셰이킹 시작
    /// </summary>
    public void ShakeCamera()
    {
        ShakeCamera(defaultShakeIntensity, defaultShakeTime);
    }

    /// <summary>
    /// 커스텀 설정으로 카메라 셰이킹 시작
    /// </summary>
    /// <param name="intensity">셰이킹 강도</param>
    /// <param name="duration">지속 시간</param>
    public void ShakeCamera(float intensity, float duration)
    {
        if (noise != null)
        {
            currentIntensity = intensity;
            noise.AmplitudeGain = intensity;
            shakeTimer = duration;
        }
        else
        {
            Debug.LogError("Noise 컴포넌트가 설정되지 않았습니다!");
        }
    }

    /// <summary>
    /// 카메라 셰이킹 즉시 중지
    /// </summary>
    public void StopShake()
    {
        if (noise != null)
        {
            noise.AmplitudeGain = 0f;
            shakeTimer = 0f;
            currentIntensity = 0f;
        }
    }

    /// <summary>
    /// 현재 셰이킹 중인지 확인
    /// </summary>
    public bool IsShaking()
    {
        return shakeTimer > 0;
    }

    /// <summary>
    /// 노이즈 프로파일 설정 (런타임에서 변경 가능)
    /// </summary>
    public void SetNoiseProfile(NoiseSettings profile)
    {
        if (noise != null)
        {
            noise.NoiseProfile = profile;
        }
    }
}