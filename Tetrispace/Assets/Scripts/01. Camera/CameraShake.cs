using UnityEngine;
using Unity.Cinemachine;

public class CameraShake : MonoBehaviour
{
    [Header("Unity 6 - Cinemachine 3 ī�޶� ����ũ")]
    public static CameraShake Instance { get; private set; }

    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private float defaultShakeIntensity = 1f;
    [SerializeField] private float defaultShakeTime = 0.5f;

    private CinemachineBasicMultiChannelPerlin noise;
    private float shakeTimer;
    private float currentIntensity;

    private void Awake()
    {
        // �̱��� ����
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

        // CinemachineCamera ������Ʈ ��������
        if (cinemachineCamera == null)
        {
            cinemachineCamera = GetComponent<CinemachineCamera>();
        }

        if (cinemachineCamera == null)
        {
            Debug.LogError("CinemachineCamera ������Ʈ�� ã�� �� �����ϴ�! " +
                          "GameObject�� CinemachineCamera ������Ʈ�� �߰����ּ���.");
            return;
        }

        // Noise ������Ʈ �������� (Unity 6/Cinemachine 3 ���)
        SetupNoise();
    }

    private void SetupNoise()
    {
        // Cinemachine 3������ Noise�� Extension���� �߰�
        noise = cinemachineCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();

        if (noise == null)
        {
            // �ڵ����� Noise Extension �߰�
            noise = cinemachineCamera.gameObject.AddComponent<CinemachineBasicMultiChannelPerlin>();
            Debug.Log("CinemachineBasicMultiChannelPerlin�� �ڵ����� �߰��Ǿ����ϴ�.");
        }

        // �⺻ Noise Settings ����
        if (noise.NoiseProfile == null)
        {
            // �⺻ ������ �������� �ε� �õ�
            var defaultProfile = Resources.Load<NoiseSettings>("6D Shake");
            if (defaultProfile != null)
            {
                noise.NoiseProfile = defaultProfile;
            }
            else
            {
                Debug.LogWarning("�⺻ Noise Profile�� ã�� �� �����ϴ�. " +
                               "Inspector���� Noise Profile�� �������� �������ּ���.");
            }
        }
    }

    private void Start()
    {
        StopShake();
    }

    private void Update()
    {
        // �׽�Ʈ�� Ű �Է�
        if (Input.GetKeyDown(KeyCode.L))
        {
            ShakeCamera();
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            ShakeCamera(2f, 1f); // ���� ����ŷ
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            StopShake();
        }

        // ����ŷ Ÿ�̸� ó��
        if (shakeTimer > 0)
        {
            shakeTimer -= Time.deltaTime;

            // ���������� ����ŷ ���� ���� (���û���)
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
    /// �⺻ �������� ī�޶� ����ŷ ����
    /// </summary>
    public void ShakeCamera()
    {
        ShakeCamera(defaultShakeIntensity, defaultShakeTime);
    }

    /// <summary>
    /// Ŀ���� �������� ī�޶� ����ŷ ����
    /// </summary>
    /// <param name="intensity">����ŷ ����</param>
    /// <param name="duration">���� �ð�</param>
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
            Debug.LogError("Noise ������Ʈ�� �������� �ʾҽ��ϴ�!");
        }
    }

    /// <summary>
    /// ī�޶� ����ŷ ��� ����
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
    /// ���� ����ŷ ������ Ȯ��
    /// </summary>
    public bool IsShaking()
    {
        return shakeTimer > 0;
    }

    /// <summary>
    /// ������ �������� ���� (��Ÿ�ӿ��� ���� ����)
    /// </summary>
    public void SetNoiseProfile(NoiseSettings profile)
    {
        if (noise != null)
        {
            noise.NoiseProfile = profile;
        }
    }
}