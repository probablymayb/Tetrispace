//using UnityEngine;
//using Cinemachine;

//public class CameraShake : MonoBehaviour
//{
//    public static CameraShake Instance { get; private set; }

//    private CinemachineVirtualCamera CinemachineVirtualCamera;
//    private CinemachineBasicMultiChannelPerlin _cbmcp;

//    [SerializeField] private float fShakeIntensity = 1f;
//    [SerializeField] private float fShakeTime = 0.5f;
//    private float fTimer = 0f;

//    private void Awake()
//    {
//        if (Instance == null)
//            Instance = this;
//        else
//            Destroy(gameObject);
//        CinemachineVirtualCamera = GetComponent<CinemachineVirtualCamera>();
     
//    }

//    private void Start()
//    {
//        StopShake();
//    }
//    public void ShakeCamera()
//    {
//        CinemachineBasicMultiChannelPerlin _cbmcp = CinemachineVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

//        if (_cbmcp == null)
//        {
//            Debug.LogError("CinemachineBasicMultiChannelPerlin component not found!");
//        }
//        if (_cbmcp != null)
//        {
//            _cbmcp.m_AmplitudeGain = fShakeIntensity;
//            fTimer = fShakeTime;
//        }
//    }

//    public void StopShake()
//    {
//        CinemachineBasicMultiChannelPerlin _cbmcp = CinemachineVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

//        if (_cbmcp == null)
//        {
//            Debug.LogError("CinemachineBasicMultiChannelPerlin component not found!");
//        }
//        if (_cbmcp != null)
//        {
//            _cbmcp.m_AmplitudeGain = 0f;
//            fTimer = 0f;
//        }
//    }

//    void Update()
//    {
//        if (Input.GetKey(KeyCode.L))
//        {
//            ShakeCamera();
//        }

//        if (fTimer > 0)
//        {
//            fTimer -= Time.deltaTime;
//            if (fTimer <= 0)
//            {
//                StopShake();
//            }
//        }
//    }
//}