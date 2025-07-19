using UnityEngine;
using System.Collections;

/// <summary>
/// ������ UselessBlock - �׸��忡 ��ϵ����� ���� Ŭ������� ����
/// </summary>
public class UselessBlock : MonoBehaviour
{
    [Header("=== UselessBlock ���� ===")]
    [SerializeField] private float lifetime = 10f;
    [SerializeField] private bool enableDebug = false;

    private bool isDestroying = false;

    void Start()
    {

        //// �÷��̾ �θ�� ���� (�÷��̾� Transform ã��)
        //Transform playerTransform = FindObjectOfType<PlayerController>().transform; // �Ǵ� �ٸ� ������� �÷��̾� ����
        //this.transform.SetParent(playerTransform);

        //// ���� ��ġ�� ���� (�θ� ���� ��� ��ġ)
        //this.transform.localPosition = new Vector3(1.5f * 0.28f, 0.5f * 0.28f, 0f);

        // UselessBlock �±� ���� (���� Ŭ���� ���ܿ�)
        gameObject.tag = "UselessBlock";

        // �ڽ� ���ϵ鵵 UselessBlock �±� ����
        SetupBlockParts();

        // �ڵ� ���� Ÿ�̸� ����
        StartCoroutine(LifetimeCoroutine());

        if (enableDebug)
        {
            Debug.Log($"UselessBlock ����: {name} - {lifetime}�� �� ���� ����");
        }
    }

    /// <summary>
    /// I���� ���� ���� ��� ����
    /// </summary>
    void SetupBlockParts()
    {
        // �ڽ� ������Ʈ�鵵 UselessBlock �±� ����
        Transform[] children = GetComponentsInChildren<Transform>();

        foreach (Transform child in children)
        {
            if (child != transform) // �ڱ� �ڽ� ����
            {
                child.tag = "UselessBlock";

                // �ݶ��̴� ����
                Collider2D collider = child.GetComponent<Collider2D>();
                if (collider != null)
                {
                    collider.enabled = true;
                }
            }
        }

        if (enableDebug)
        {
            Debug.Log($"UselessBlock �ڽ� ���ϵ� ���� �Ϸ�");
        }
    }

    /// <summary>
    /// ���� ���� �ڷ�ƾ
    /// </summary>
    private IEnumerator LifetimeCoroutine()
    {
        float timer = 0f;

        while (timer < lifetime && !isDestroying)
        {
            timer += Time.deltaTime;

            // 2�ʸ��� ���� �ð� �α�
            if (enableDebug && Mathf.FloorToInt(timer) % 2 == 0 && timer >= 2f)
            {
                float remainingTime = lifetime - timer;
                if (remainingTime > 0)
                {
                    Debug.Log($"UselessBlock {name} - ���� �ð�: {remainingTime:F1}��");
                }
            }

            yield return null;
        }

        // �ð� ���� �� ����
        if (!isDestroying)
        {
            DestroyUselessBlock();
        }
    }

    /// <summary>
    /// UselessBlock ���� ó��
    /// </summary>
    public void DestroyUselessBlock()
    {
        if (isDestroying) return;
        isDestroying = true;

        if (enableDebug)
        {
            Debug.Log($"UselessBlock {name} ���� ����");
        }

        // TetriminoManager���� �׸��� ����
        if (TetriminoManager.Instance != null)
        {
            TetriminoManager.Instance.RemoveUselessBlockFromGrid(this);
        }

        TetriminoManager.Instance.ClearLine(0);
        // ������Ʈ �ı�
        Destroy(gameObject);
    }

    /// <summary>
    /// ��� ���� (�ܺο��� ȣ�� ����)
    /// </summary>
    public void ForceDestroy()
    {
        if (enableDebug)
        {
            Debug.Log($"UselessBlock {name} ���� ����");
        }

        StopAllCoroutines();
        DestroyUselessBlock();
    }

    /// <summary>
    /// ���� ���� ��ȯ
    /// </summary>
    public float GetRemainingLifetime()
    {
        return Mathf.Max(0f, lifetime - (Time.time - Time.time)); // ��Ȯ�� ��� �ʿ�� ����
    }

    void Update()
    {
        

    }
    void OnDestroy()
    {
        StopAllCoroutines();
    }
}