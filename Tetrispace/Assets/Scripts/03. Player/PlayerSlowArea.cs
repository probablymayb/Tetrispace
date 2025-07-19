using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PlayerSlowArea : MonoBehaviour
{
    public static readonly string Tag = "SlowArea";

    [SerializeField] private List<GameObject> effects = new List<GameObject>();
    private GameObject effectField;
    private CircleCollider2D coll;
    private PlayerStat stat;
    
    private float radius;
    private float Radius
    {
        get => radius;
        set
        {
            radius = value;
            coll.radius = radius;
        }
    }
    private void Awake()
    {
        coll = GetComponent<CircleCollider2D>();
        coll.isTrigger = true;
    }

    public void Init(PlayerStat playerStat)
    {
        stat = playerStat;
        Radius = playerStat.GetStat(PlayerEnforcement.Area)[0];
        playerStat.OnLevelUp -= OnSlowAreaLevelUp;
        playerStat.OnLevelUp += OnSlowAreaLevelUp;
        effectField = Instantiate(effects[playerStat.GetLevel(PlayerEnforcement.Area) - 1], transform.position, transform.rotation);
        effectField.transform.SetParent(transform);
    }

    private void OnSlowAreaLevelUp(PlayerEnforcement enforcement)
    {
        if (enforcement != PlayerEnforcement.Area) return;
        Radius = stat.GetStat(PlayerEnforcement.Area)[0];
        if (effectField)
        {
            Destroy(effectField);
        }
        effectField = Instantiate(effects[stat.GetLevel(PlayerEnforcement.Area) - 1], transform.position, transform.rotation);
        effectField.transform.SetParent(transform);
    }
}