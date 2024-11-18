using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapVisualizer : MonoBehaviour
{
    //public void GenerateHeatMap(Vector2Int startPosition, HashSet<Vector2Int> floorPositions, TilemapVisualizer tilemapVisualizer, Gradient heatMapGradient = null)
    //{
    //    if (heatMapGradient == null)
    //    {
    //        var gradient = new Gradient();   
    //        gradient.colorKeys = new GradientColorKey[]
    //        {
    //        new GradientColorKey(Color.green, 0f), 
    //        new GradientColorKey(Color.red, 1f)
    //        };
    //        heatMapGradient = gradient;
    //    }


    //    // Disable rendering temporarily to avoid visual glitches during update

    //    //heatmapTilemap.GetComponent<TilemapRenderer>().enabled = false;
    //    //heatmapTilemap.ClearAllTiles();
        


    //    // Perform BFS to calculate distances
    //    var distanceMap = SearchUtility.CreateDistancesMapAt(startPosition, floorPositions, Direction2D.cardinalDirectionsList);

    //    int maxDistance = SearchUtility.FindMaxDistanceIn(distanceMap);
    //    TileBase[] tilesToSet = new TileBase[distanceMap.Count];
    //    Vector3Int[] positions = new Vector3Int[distanceMap.Count];

    //    int index = 0;
    //    foreach (var item in distanceMap)
    //    {
    //        int distance = item.Value;
    //        float normalizedDistance = Mathf.InverseLerp(0, maxDistance, distance);
    //        Color heatColor = heatMapGradient.Evaluate(normalizedDistance);

    //        Tile heatTile = GetTileWithColor(heatColor, tilemapVisualizer.DefaultTile);
    //        tilesToSet[index] = heatTile;
    //        positions[index] = (Vector3Int)item.Key;
    //        index++;
    //    }
        

    //    tilemapVisualizer.PaintDebugTiles(positions, tilesToSet);
    //    //heatmapTilemap.GetComponent<TilemapRenderer>().enabled = true; // Re-enable rendering
    //}
    //private Tile GetTileWithColor(Color color, Tile defaultHeatTile)
    //{
    //    var tile = ScriptableObject.CreateInstance<Tile>();
    //    tile.sprite = ((Tile)defaultHeatTile).sprite; // Copy the sprite from the default tile
    //    tile.color = color;
    //    return tile;
    //}

}

