using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PlayerSlowArea : MonoBehaviour
{
    public static readonly string Tag = "SlowArea";

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
        Radius = playerStat.GetStat(PlayerEnforcement.Area);
        playerStat.OnLevelUp -= OnSlowAreaLevelUp;
        playerStat.OnLevelUp += OnSlowAreaLevelUp;
    }

    private void OnSlowAreaLevelUp(PlayerEnforcement enforcement)
    {
        if (enforcement != PlayerEnforcement.Area) return;
        Radius = stat.GetStat(PlayerEnforcement.Area);
    }
}