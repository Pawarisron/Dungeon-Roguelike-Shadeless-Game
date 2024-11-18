using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapVisualizer : MonoBehaviour
{
    [Header("Tilemap")]
    [SerializeField] private Tilemap tilemapForeground;
    [SerializeField] private Tilemap tilemapBackground;
    [SerializeField] private Tilemap tilemapDebug;
    [SerializeField] private Tilemap tilemapCollider;

    [Header("RuleTile")]
    [SerializeField] private RuleTile floorTile;
    [SerializeField] private RuleTile wallTile;
    [SerializeField] private RuleTile wallEdgeTile;

    [Header("DefaultTile")]
    [SerializeField] private Tile defaultTile;

    public Tile DefaultTile { get => defaultTile; set => defaultTile = value; }

    public void PaintFloorTiles(IEnumerable<Vector2Int> floorPositions)
    {
        PaintTiles(floorPositions, tilemapBackground, floorTile);
    }
    public void PaintWallTiles(IEnumerable<Vector2Int> wallPositions)
    {
        PaintTiles(wallPositions, tilemapBackground, wallTile);
    }
    public void PaintWallsEdgeTiles(IEnumerable<Vector2Int> wallPositions)
    {
        PaintTiles(wallPositions, tilemapBackground, wallEdgeTile);
    }
    public void PaintColliderTiles(IEnumerable<Vector2Int> wallPositions)
    {
        PaintTiles(wallPositions, tilemapCollider, defaultTile);
        //tilemapCollider.RefreshAllTiles();
    }
    public void PaintDebugTiles(Vector3Int[] tilePositions, TileBase[] tiles)
    {
        tilemapDebug.SetTiles(tilePositions, tiles);
    }
    public void PaintDebugTiles(IEnumerable<Vector2Int> tilePositions, TileBase tile)
    {
        PaintTiles(tilePositions, tilemapDebug, tile);
    }
    public Tilemap GetMainTilemap()
    {
        return tilemapBackground;
    }

    private void PaintTiles(IEnumerable<Vector2Int> positions, Tilemap tilemap, TileBase tile)
    {
        foreach (var position in positions)
        {
            PaintSingleTile(position, tilemap, tile);
        }
    }
    private void PaintTiles(IEnumerable<Vector2Int> positions, Tilemap tilemap, TileBase[] tiles)
    {
        //Convert a collection of Vector2Int to Vector3Int via LINQ
        Vector3Int[] vector3Qrt = positions.Select(v2 => new Vector3Int(v2.x, v2.y, 0)).ToArray();
        tilemap.SetTiles(vector3Qrt, tiles);
    }
    private void PaintSingleTile(Vector2Int position, Tilemap tilemap, TileBase tile)
    {
        var tilePosition = tilemap.WorldToCell((Vector3Int)position);
        tilemap.SetTile(tilePosition, tile);
    }

    public void Clear()
    {
        tilemapForeground.ClearAllTiles();
        tilemapBackground.ClearAllTiles();
        tilemapCollider.ClearAllTiles();
        tilemapDebug.ClearAllTiles();
    }

    
}
