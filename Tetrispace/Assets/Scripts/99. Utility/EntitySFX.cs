using UnityEngine;
//using FMODUnity;

public class EntitySFX : MonoBehaviour
{
    // [Header("FMOD Events")]
    [SerializeField] AudioClip BGM;
    // [Header("FMOD Events")]
    private void Awake()
    {
        SFXManager.Instance.PlayBGM(BGM, 1F, 1F);
    }
}