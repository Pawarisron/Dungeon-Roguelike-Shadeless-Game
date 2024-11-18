using System.Collections.Generic;
using UnityEngine;

public class TestPlacingCollider : MonoBehaviour
{
    public TilemapVisualizer tilemapVisualizer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private HashSet<Vector2Int> position = new HashSet<Vector2Int>();
    void Start()
    {
        position.Add(Vector2Int.zero);
        tilemapVisualizer.PaintColliderTiles(position);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
