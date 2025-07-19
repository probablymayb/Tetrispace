using System.Collections.Generic;
using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
    
    [SerializeField] private float scrollSpeed = 2f;
    [SerializeField] private float tileHeight = 10f;

    private List<Transform> bgTiles = new List<Transform>();

    private void Start()
    {
        // 자식 배경 타일 가져오기
        for (int i = 0; i < transform.childCount; i++)
        {
            bgTiles.Add(transform.GetChild(i));
        }

        // Y 위치 기준 정렬 (위 → 아래)
        bgTiles.Sort((a, b) => b.position.y.CompareTo(a.position.y));
    }

    private void Update()
    {
        // 아래로 이동
        foreach (Transform tile in bgTiles)
        {
            tile.position += Vector3.down * (scrollSpeed * Time.deltaTime);
        }

        // 가장 아래에 있는 타일이 기준 이하로 내려가면 맨 위로 재배치
        Transform bottomTile = bgTiles[^1];
        if (bottomTile.position.y < -tileHeight)
        {
            Transform topTile = bgTiles[0];

            bottomTile.position = new Vector3(
                bottomTile.position.x,
                topTile.position.y + tileHeight,
                bottomTile.position.z
            );

            // 순서 갱신
            bgTiles.RemoveAt(bgTiles.Count-1);
            bgTiles.Insert(0, bottomTile);
        }
    }
}
    