using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Testing : MonoBehaviour
{

    [SerializeField]
    private bool showGizmo = true;

    private DungeonData dungeonData;

    public void Awake()
    {
        dungeonData = FindAnyObjectByType<DungeonData>();
    }

    public void ProcessTesting()
    {
        if (dungeonData == null)
        {
            Debug.LogError("DungeonData is null," + this.name);
            return;
        }
    }
    private void OnDrawGizmosSelected()
    {
        
        if (showGizmo == false || dungeonData == null)
            return;

        foreach (var position in dungeonData.AvailablePositions)
        {
            Color color = Color.green;
            color.a = 0.3f;
            Gizmos.color = color;
            Gizmos.DrawCube(position + Vector2.one * 0.5f, Vector2.one);
        }
    }
}
