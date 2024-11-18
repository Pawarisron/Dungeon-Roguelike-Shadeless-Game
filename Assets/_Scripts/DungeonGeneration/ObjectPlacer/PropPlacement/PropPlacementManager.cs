using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class PropPlacementManager : MonoBehaviour
{
    //TODO: Refactor PropPlacementManager

    private DungeonData dungeonData;

    [SerializeField]
    private List<PropData> propsToPlace;

    [SerializeField, Range(0, 1)]
    private float cornerPropPlacementChance = 0.7f;

    [SerializeField]
    private GameObject parentProps;

    public GameEvent OnFinishedPropPlacing;

    public bool showGizmo = false;

    private HashSet<Vector2Int> debugAvailablePositions = new HashSet<Vector2Int>();

    //private HashSet<Vector2Int> test = new HashSet<Vector2Int>();

    private void Awake()
    {
        dungeonData = FindAnyObjectByType<DungeonData>();
    }

    public void ProcessPropPlacement()
    {
        if (dungeonData == null)
        {
            Debug.LogError("DungeonData is null," + this.name);
            return;
        }
        //Debug variable
        debugAvailablePositions.Clear();

        //Avoid Type
        List<DungeonRoomType> avoidedSpawnInnerRoomType = new List<DungeonRoomType>(DungeonData.DungeonMainRoomType);

        foreach (DungeonRoomData room in dungeonData.Rooms)
        {
            //Place props in the corners
            List<PropData> cornerProps = propsToPlace.Where(p => p.Corner).ToList();
            
            PlaceCornerProps(room, cornerProps);

            //Place props near LEFT wall
            List<PropData> leftWallProps = propsToPlace
            .Where(x => x.NearWallLeft)
            .OrderByDescending(x => x.PropSize.x * x.PropSize.y)
            .ToList();

            PlaceProps(room, leftWallProps, room.NearWallTilesLeft, PlacementOriginCorner.BottomLeft);

            //Place props near RIGHT wall
            List<PropData> rightWallProps = propsToPlace
            .Where(x => x.NearWallRight)
            .OrderByDescending(x => x.PropSize.x * x.PropSize.y)
            .ToList();

            PlaceProps(room, rightWallProps, room.NearWallTilesRight, PlacementOriginCorner.TopRight);

            //Place props near UP wall
            List<PropData> topWallProps = propsToPlace
            .Where(x => x.NearWallUP)
            .OrderByDescending(x => x.PropSize.x * x.PropSize.y)
            .ToList();

            PlaceProps(room, topWallProps, room.NearWallTilesUp, PlacementOriginCorner.TopLeft);

            //Place props near DOWN wall
            List<PropData> downWallProps = propsToPlace
            .Where(x => x.NearWallDown)
            .OrderByDescending(x => x.PropSize.x * x.PropSize.y)
            .ToList();

            PlaceProps(room, downWallProps, room.NearWallTilesDown, PlacementOriginCorner.BottomLeft);

            //Avoid spawn in avoided room
            if (!avoidedSpawnInnerRoomType.Contains(room.RoomType))
            {
                //Place inner props
                List<PropData> innerProps = propsToPlace
                    .Where(x => x.Inner)
                    .OrderByDescending(x => x.PropSize.x * x.PropSize.y)
                    .ToList();

                //Avoid unAvaliable positions
                HashSet<Vector2Int> innerTiles = new HashSet<Vector2Int>(room.InnerTiles.Except(room.ObjectPositions));
                PlaceProps(room, innerProps, innerTiles, PlacementOriginCorner.BottomLeft);
            }
        }
        //Wait 0.5 Sec
        //Invoke("RunEvent", 0.5f);
        RunEvent();

    }
    public void RunEvent()
    {
        OnFinishedPropPlacing.Raise(this, null);
    }
    private void OnDrawGizmosSelected()
    {
        if (showGizmo == false || dungeonData == null)
            return;
        //Deubg
        foreach (var position in debugAvailablePositions)
        {
            Color color = Color.green;
            color.a = 0.3f;
            Gizmos.color = color;
            Gizmos.DrawCube(position + Vector2.one * 0.5f, Vector2.one);
        }


        foreach (var position in dungeonData.PropPositions)
        {
            Color color = Color.red;
            color.a = 0.4f;
            Gizmos.color = color;
            Gizmos.DrawCube(position + Vector2.one * 0.5f, Vector2.one);
        }

    }

    /// <summary>
    /// Places props near walls. We need to specify the props anw the placement start point
    /// </summary>
    /// <param name="room"></param>
    /// <param name="wallProps">Props that we should try to place</param>
    /// <param name="availableTiles">Tiles that are near the specific wall</param>
    /// <param name="placement">How to place bigger props. Ex near top wall we want to start placemt from the Top corner and find if there are free spaces below</param>
    private void PlaceProps(
        DungeonRoomData room, List<PropData> wallProps, HashSet<Vector2Int> availableTiles, PlacementOriginCorner placement)
    {
        if (wallProps.Count <= 0)
            return;

        //Remove path positions from the initial nearWallTiles to ensure the clear path to traverse dungeon
        HashSet<Vector2Int> tempPositons = new HashSet<Vector2Int>(availableTiles);
        tempPositons.ExceptWith(dungeonData.PathBase);

        //Debug
        debugAvailablePositions.UnionWith(tempPositons);

        //We will try to place all the props
        foreach (PropData propToPlace in wallProps)
        {
            //We want to place only certain quantity of each prop
            int quantity
                = UnityEngine.Random.Range(propToPlace.MinPlacementQuantity, propToPlace.MaxPlacementQuantity + 1);

            for (int i = 0; i < quantity; i++)
            {
                //remove taken positions
                tempPositons.ExceptWith(room.PropPositions);
                //shuffel the positions
                List<Vector2Int> availablePositions = tempPositons.OrderBy(x => Guid.NewGuid()).ToList();
                //If placement has failed there is no point in trying to place the same prop again
                if (TryPlacingPropBruteForce(room, propToPlace, availablePositions, placement) == false)
                    break;
            }

        }
    }

    /// <summary>
    /// Tries to place the Prop using brute force (trying each available tile position)
    /// </summary>
    /// <param name="room"></param>
    /// <param name="propToPlace"></param>
    /// <param name="availablePositions"></param>
    /// <param name="placement"></param>
    /// <returns>False if there is no space. True if placement was successful</returns>
    private bool TryPlacingPropBruteForce(
        DungeonRoomData room, PropData propToPlace, List<Vector2Int> availablePositions, PlacementOriginCorner placement)
    {
        //try placing the objects starting from the corner specified by the placement parameter
        for (int i = 0; i < availablePositions.Count; i++)
        {
            //select the specified position (but it can be already taken after placing the corner props as a group)
            Vector2Int position = availablePositions[i];
            if (room.PropPositions.Contains(position))
                continue;

            //check if there is enough space around to fit the prop
            List<Vector2Int> freePositionsAround
                = TryToFitProp(propToPlace, availablePositions, position, placement);

            //If we have enough spaces place the prop
            if (freePositionsAround.Count == propToPlace.PropSize.x * propToPlace.PropSize.y)
            {
                //Place the gameobject
                PlacePropGameObjectAt(room, position, propToPlace);
                //Lock all the positions recquired by the prop (based on its size)
                foreach (Vector2Int pos in freePositionsAround)
                {
                    //Hashest will ignore duplicate 
                    room.PropPositions.Add(pos);
                }

                //Deal with groups
                if (propToPlace.PlaceAsGroup)
                {
                    PlaceGroupObject(room, position, propToPlace, 1);
                }
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Checks if the prop will fit (accordig to it size)
    /// </summary>
    /// <param name="prop"></param>
    /// <param name="availablePositions"></param>
    /// <param name="originPosition"></param>
    /// <param name="placement"></param>
    /// <returns></returns>
    private List<Vector2Int> TryToFitProp(
        PropData prop,
        List<Vector2Int> availablePositions,
        Vector2Int originPosition,
        PlacementOriginCorner placement)
    {
        List<Vector2Int> freePositions = new();

        //Perform the specific loop depending on the PlacementOriginCorner
        if (placement == PlacementOriginCorner.BottomLeft)
        {
            for (int xOffset = 0; xOffset < prop.PropSize.x; xOffset++)
            {
                for (int yOffset = 0; yOffset < prop.PropSize.y; yOffset++)
                {
                    Vector2Int tempPos = originPosition + new Vector2Int(xOffset, yOffset);
                    if (availablePositions.Contains(tempPos))
                        freePositions.Add(tempPos);
                }
            }
        }
        else if (placement == PlacementOriginCorner.BottomRight)
        {
            for (int xOffset = -prop.PropSize.x + 1; xOffset <= 0; xOffset++)
            {
                for (int yOffset = 0; yOffset < prop.PropSize.y; yOffset++)
                {
                    Vector2Int tempPos = originPosition + new Vector2Int(xOffset, yOffset);
                    if (availablePositions.Contains(tempPos))
                        freePositions.Add(tempPos);
                }
            }
        }
        else if (placement == PlacementOriginCorner.TopLeft)
        {
            for (int xOffset = 0; xOffset < prop.PropSize.x; xOffset++)
            {
                for (int yOffset = -prop.PropSize.y + 1; yOffset <= 0; yOffset++)
                {
                    Vector2Int tempPos = originPosition + new Vector2Int(xOffset, yOffset);
                    if (availablePositions.Contains(tempPos))
                        freePositions.Add(tempPos);
                }
            }
        }
        else
        {
            for (int xOffset = -prop.PropSize.x + 1; xOffset <= 0; xOffset++)
            {
                for (int yOffset = -prop.PropSize.y + 1; yOffset <= 0; yOffset++)
                {
                    Vector2Int tempPos = originPosition + new Vector2Int(xOffset, yOffset);
                    if (availablePositions.Contains(tempPos))
                        freePositions.Add(tempPos);
                }
            }
        }

        return freePositions;
    }

    /// <summary>
    /// Places props in the corners of the room
    /// </summary>
    /// <param name="room"></param>
    /// <param name="cornerProps"></param>
    private void PlaceCornerProps(DungeonRoomData room, List<PropData> cornerProps)
    {
        if (cornerProps.Count <= 0)
            return;
        
        float tempChance = cornerPropPlacementChance;
        HashSet<Vector2Int> tmpRoomCornerTiles = new HashSet<Vector2Int>(room.CornerTiles);
        tmpRoomCornerTiles.ExceptWith(dungeonData.PathBase);
        
        //Debug
        debugAvailablePositions.UnionWith(tmpRoomCornerTiles);

        foreach (Vector2Int cornerTile in tmpRoomCornerTiles)
        {
            if (UnityEngine.Random.value < tempChance)
            {
                PropData propToPlace = cornerProps[UnityEngine.Random.Range(0, cornerProps.Count)];

                PlacePropGameObjectAt(room, cornerTile, propToPlace);
                if (propToPlace.PlaceAsGroup)
                {
                    PlaceGroupObject(room, cornerTile, propToPlace, 2);
                }
            }
            //else
            //{
            //    tempChance = Mathf.Clamp01(tempChance + 0.1f);
            //}
        }
    }

    /// <summary>
    /// Helps to find free spaces around the groupOriginPosition to place a prop as a group
    /// </summary>
    /// <param name="room"></param>
    /// <param name="groupOriginPosition"></param>
    /// <param name="propToPlace"></param>
    /// <param name="searchOffset">The search offset ex 1 = we will check all tiles withing the distance of 1 unity away from origin position</param>
    private void PlaceGroupObject(
        DungeonRoomData room, Vector2Int groupOriginPosition, PropData propToPlace, int searchOffset)
    {
        //TODO: Make place group canplace more than 8 and can placeing bigger props
        //***Can work poorely when placing bigger props as groups

        //calculate how many elements are in the group -1 that we have placed in the center
        int count = UnityEngine.Random.Range(propToPlace.GroupMinCount, propToPlace.GroupMaxCount) - 1;
        count = Mathf.Clamp(count, 0, 8);

        //find the available spaces around the center point.
        //we use searchOffset to limit the distance between those points and the center point
        List<Vector2Int> availableSpaces = new List<Vector2Int>();
        for (int xOffset = -searchOffset; xOffset <= searchOffset; xOffset++)
        {
            for (int yOffset = -searchOffset; yOffset <= searchOffset; yOffset++)
            {
                Vector2Int tempPos = groupOriginPosition + new Vector2Int(xOffset, yOffset);
                if (room.FloorTiles.Contains(tempPos) &&
                    !dungeonData.PathBase.Contains(tempPos) &&
                    !room.PropPositions.Contains(tempPos))
                {
                    availableSpaces.Add(tempPos);
                }
            }
        }

        //shuffle the list
        availableSpaces.OrderBy(x => Guid.NewGuid());

        //place the props (as many as we want or if there is less space fill all the available spaces)
        int tempCount = count < availableSpaces.Count ? count : availableSpaces.Count;
        for (int i = 0; i < tempCount; i++)
        {
            PlacePropGameObjectAt(room, availableSpaces[i], propToPlace);
        }

    }

    /// <summary>
    /// Place a prop as a new GameObject at a specified position
    /// </summary>
    /// <param name="room"></param>
    /// <param name="placementPostion"></param>
    /// <param name="propToPlace"></param>
    /// <returns></returns>
    private GameObject PlacePropGameObjectAt(DungeonRoomData room, Vector2Int placementPostion, PropData propToPlace)
    {
        GameObject prop = Instantiate(propToPlace.PropPrefab);
        prop.transform.localPosition = (Vector2)placementPostion;
        //set parent
        prop.transform.parent = parentProps.transform;
        prop.name = propToPlace.PropPrefab.name + placementPostion;
        //inherited from parent
        prop.layer = parentProps.layer;
        //Loop Through every child in prefab
        foreach (Transform child in prop.transform)
        {
            child.gameObject.layer = parentProps.layer;
        }

        //Save the prop in the room data (in the dunbgeon data)
        room.PropPositions.Add(placementPostion);
        room.PropReferences.Add(prop);
        return prop;
    }
}

/// <summary>
/// Where to start placing the prop ex. start at BottomLeft corner and search 
/// if there are free space to the Right and Up in case of placing a biggex prop
/// </summary>
public enum PlacementOriginCorner
{
    BottomLeft,
    BottomRight,
    TopLeft,
    TopRight
}