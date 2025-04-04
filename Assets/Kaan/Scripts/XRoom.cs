using System;
using System.Collections.Generic;
using UnityEngine;

public enum Direction
{
    North,
    East,
    South,
    West
}

[Serializable]
public class WallData
{
    public Direction direction;
    public List<Vector2Int> wallCells = new List<Vector2Int>();
    public List<DoorwayData> doorways = new List<DoorwayData>();
}

[Serializable]
public class DoorwayData
{
    public Vector2Int position;
    public bool isConnected;
    public XRoom connectedRoom;
}

public class XRoom : MonoBehaviour
{
    [Header("Room Properties")]
    public Vector2Int size;
    public float cellSize = 1.0f;

    [Header("Generated Data")]
    public List<WallData> walls = new List<WallData>();
    public List<Vector2Int> floorCells = new List<Vector2Int>();
    public List<Vector2Int> ceilingCells = new List<Vector2Int>();

    [Header("Room Contents")]
    public List<GameObject> placedFurniture = new List<GameObject>();

    // Theme-related properties
    private DungeonTheme currentTheme;
    private FurnitureSet currentFurnitureSet;

    // Dictionary to store instantiated game objects for easy access
    private Dictionary<Vector2Int, GameObject> wallObjects = new Dictionary<Vector2Int, GameObject>();
    private Dictionary<Vector2Int, GameObject> floorObjects = new Dictionary<Vector2Int, GameObject>();
    private Dictionary<Vector2Int, GameObject> ceilingObjects = new Dictionary<Vector2Int, GameObject>();

    // Room height tracking
    private float roomHeight = 2.0f; // Default room height

    public void Initialize(Vector2Int roomSize, float cellSizeValue)
    {
        size = roomSize;
        cellSize = cellSizeValue;

        // Initialize room data
        InitializeRoom();
    }

    public void AssignTheme(DungeonTheme theme)
    {
        currentTheme = theme;
    }

    public void AssignFurnitureSet(FurnitureSet furnitureSet)
    {
        currentFurnitureSet = furnitureSet;
    }

    private void InitializeRoom()
    {
        // Clear existing data
        walls.Clear();
        floorCells.Clear();
        ceilingCells.Clear();

        // Initialize walls for each direction
        walls.Add(new WallData { direction = Direction.North });
        walls.Add(new WallData { direction = Direction.East });
        walls.Add(new WallData { direction = Direction.South });
        walls.Add(new WallData { direction = Direction.West });

        // Calculate the total room world dimensions
        float roomWorldWidth = size.x * cellSize;
        float roomWorldDepth = size.y * cellSize;

        // Calculate starting points - the room center is at transform.position
        float startX = -roomWorldWidth / 2;
        float startZ = -roomWorldDepth / 2;

        // Populate floor and ceiling cells with proper spacing
        for (int x = 0; x < size.x; x++)
        {
            for (int z = 0; z < size.y; z++)
            {
                Vector2Int position = new Vector2Int(x, z);
                floorCells.Add(position);
                ceilingCells.Add(position);

                // Add wall cells
                if (x == 0)
                {
                    GetWallData(Direction.West).wallCells.Add(position);
                }
                if (x == size.x - 1)
                {
                    GetWallData(Direction.East).wallCells.Add(position);
                }
                if (z == 0)
                {
                    GetWallData(Direction.South).wallCells.Add(position);
                }
                if (z == size.y - 1)
                {
                    GetWallData(Direction.North).wallCells.Add(position);
                }
            }
        }

        Debug.Log($"Room initialized with dimensions: {size.x}x{size.y} cells, " +
                 $"world size: {roomWorldWidth}x{roomWorldDepth}, " +
                 $"cell size: {cellSize}");
    }

    public void BuildRoom()
    {
        // Clear any existing objects
        ClearRoom();

        // Check if a theme is assigned
        if (currentTheme == null || currentTheme.wallModels.Count == 0 ||
            currentTheme.floorModels.Count == 0 || currentTheme.ceilingModels.Count == 0)
        {
            Debug.LogError("Cannot build room: no theme assigned or theme is incomplete");
            return;
        }

        // First, determine the wall height by measuring a wall prefab
        DetermineRoomHeight();

        // Create floor
        CreateFloor();

        // Create walls
        CreateWalls();

        // Create ceiling (after walls are created so we know the height)
        CreateCeiling();
    }

    private void DetermineRoomHeight()
    {
        // Measure all wall prefabs to get accurate room height
        roomHeight = 0;

        if (currentTheme.wallModels.Count > 0)
        {
            foreach (GameObject wallPrefab in currentTheme.wallModels)
            {
                if (wallPrefab != null)
                {
                    // Create a temporary instance to measure its bounds
                    GameObject tempWall = Instantiate(wallPrefab);
                    Bounds wallBounds = GetObjectBounds(tempWall);
                    DestroyImmediate(tempWall);

                    // Use the maximum height found
                    roomHeight = Mathf.Max(roomHeight, wallBounds.size.y);
                }
            }
        }

        // If no valid measurements were found, use a default height
        if (roomHeight <= 0)
        {
            roomHeight = 2.0f;
            Debug.LogWarning("Could not determine room height from wall models. Using default height of 2.0 units.");
        }

        Debug.Log($"Room height determined: {roomHeight} units");
    }

    private void ClearRoom()
    {
        // Destroy all child objects
        foreach (Transform child in transform)
        {
            DestroyImmediate(child.gameObject);
        }

        // Clear dictionaries
        wallObjects.Clear();
        floorObjects.Clear();
        ceilingObjects.Clear();
        placedFurniture.Clear();
    }

    private void CreateFloor()
    {
        GameObject floorParent = new GameObject("Floor");
        floorParent.transform.parent = transform;

        // First, measure the dimensions of all floor prefabs
        Dictionary<GameObject, Bounds> floorPrefabBounds = new Dictionary<GameObject, Bounds>();
        foreach (GameObject prefab in currentTheme.floorModels)
        {
            if (!floorPrefabBounds.ContainsKey(prefab))
            {
                GameObject tempObj = Instantiate(prefab);
                floorPrefabBounds[prefab] = GetObjectBounds(tempObj);
                DestroyImmediate(tempObj);
            }
        }

        // Calculate offset from center of room
        Vector3 roomCenter = transform.position;
        float roomWorldWidth = size.x * cellSize;
        float roomWorldDepth = size.y * cellSize;
        Vector3 startPosition = new Vector3(
            roomCenter.x - roomWorldWidth / 2 + cellSize / 2,
            roomCenter.y,
            roomCenter.z - roomWorldDepth / 2 + cellSize / 2
        );

        foreach (Vector2Int cell in floorCells)
        {
            // Choose a random floor model
            GameObject floorPrefab = currentTheme.floorModels[UnityEngine.Random.Range(0, currentTheme.floorModels.Count)];
            Bounds prefabBounds = floorPrefabBounds[floorPrefab];

            // Calculate position based on cell coordinates and the calculated start position
            Vector3 position = new Vector3(
                startPosition.x + cell.x * cellSize,
                roomCenter.y - prefabBounds.size.y, // Position floor so top is at ground level
                startPosition.z + cell.y * cellSize
            );

            // Instantiate floor at correct position
            GameObject floorObject = Instantiate(floorPrefab, position, Quaternion.identity, floorParent.transform);
            floorObject.name = $"Floor_{cell.x}_{cell.y}";

            // Store reference
            floorObjects[cell] = floorObject;
        }
    }

    private void CreateCeiling()
    {
        GameObject ceilingParent = new GameObject("Ceiling");
        ceilingParent.transform.parent = transform;

        // First, measure the dimensions of all ceiling prefabs
        Dictionary<GameObject, Bounds> ceilingPrefabBounds = new Dictionary<GameObject, Bounds>();
        foreach (GameObject prefab in currentTheme.ceilingModels)
        {
            if (!ceilingPrefabBounds.ContainsKey(prefab))
            {
                GameObject tempObj = Instantiate(prefab);
                ceilingPrefabBounds[prefab] = GetObjectBounds(tempObj);
                DestroyImmediate(tempObj);
            }
        }

        // Calculate offset from center of room
        Vector3 roomCenter = transform.position;
        float roomWorldWidth = size.x * cellSize;
        float roomWorldDepth = size.y * cellSize;
        Vector3 startPosition = new Vector3(
            roomCenter.x - roomWorldWidth / 2 + cellSize / 2,
            roomCenter.y,
            roomCenter.z - roomWorldDepth / 2 + cellSize / 2
        );

        foreach (Vector2Int cell in ceilingCells)
        {
            // Choose a random ceiling model
            GameObject ceilingPrefab = currentTheme.ceilingModels[UnityEngine.Random.Range(0, currentTheme.ceilingModels.Count)];
            Bounds prefabBounds = ceilingPrefabBounds[ceilingPrefab];

            // Calculate position based on cell coordinates and the calculated start position
            Vector3 position = new Vector3(
                startPosition.x + cell.x * cellSize,
                roomCenter.y + roomHeight, // Position ceiling so bottom is at wall top
                startPosition.z + cell.y * cellSize
            );

            // Instantiate ceiling at correct position
            GameObject ceilingObject = Instantiate(ceilingPrefab, position, Quaternion.identity, ceilingParent.transform);
            ceilingObject.name = $"Ceiling_{cell.x}_{cell.y}";

            // Store reference
            ceilingObjects[cell] = ceilingObject;
        }
    }

    private void CreateWalls()
    {
        GameObject wallsParent = new GameObject("Walls");
        wallsParent.transform.parent = transform;

        // Calculate offset from center of room
        Vector3 roomCenter = transform.position;
        float roomWorldWidth = size.x * cellSize;
        float roomWorldDepth = size.y * cellSize;
        Vector3 startPosition = new Vector3(
            roomCenter.x - roomWorldWidth / 2 + cellSize / 2,
            roomCenter.y,
            roomCenter.z - roomWorldDepth / 2 + cellSize / 2
        );

        // First create a dictionary to track which cells are corner cells
        Dictionary<Vector2Int, bool> cornerCells = new Dictionary<Vector2Int, bool>();

        // Identify corner cells (cells that belong to two different walls)
        foreach (WallData wall in walls)
        {
            foreach (Vector2Int cell in wall.wallCells)
            {
                // Check if this cell is already marked by another wall
                if (cornerCells.ContainsKey(cell))
                {
                    cornerCells[cell] = true; // Mark as corner
                }
                else
                {
                    cornerCells[cell] = false; // Not yet known to be a corner
                }
            }
        }

        // Structure to store corner information
        Dictionary<Vector2Int, (Direction primary, Direction secondary)> cornerDirections =
            new Dictionary<Vector2Int, (Direction primary, Direction secondary)>();

        // Determine the primary and secondary directions for corners
        foreach (WallData wall in walls)
        {
            foreach (Vector2Int cell in wall.wallCells)
            {
                if (cornerCells.ContainsKey(cell) && cornerCells[cell])
                {
                    // This is a corner cell
                    if (!cornerDirections.ContainsKey(cell))
                    {
                        // First wall to claim this corner
                        cornerDirections[cell] = (wall.direction, Direction.North); // Placeholder secondary
                    }
                    else
                    {
                        // Second wall to claim this corner, complete the information
                        var existing = cornerDirections[cell];
                        cornerDirections[cell] = (existing.primary, wall.direction);
                    }
                }
            }
        }

        // Process each wall direction
        foreach (WallData wall in walls)
        {
            GameObject directionParent = new GameObject(wall.direction.ToString());
            directionParent.transform.parent = wallsParent.transform;

            foreach (Vector2Int cell in wall.wallCells)
            {
                // Skip if this cell has a doorway
                bool isDoorway = false;
                foreach (var doorway in wall.doorways)
                {
                    if (doorway.position == cell)
                    {
                        isDoorway = true;
                        break;
                    }
                }

                // Skip corner cells that don't have this direction as primary
                if (cornerCells.ContainsKey(cell) && cornerCells[cell] &&
                    cornerDirections.ContainsKey(cell) &&
                    cornerDirections[cell].primary != wall.direction)
                {
                    continue; // Skip this cell, it will be handled by the primary direction
                }

                if (!isDoorway)
                {
                    // Choose a random wall model
                    GameObject wallPrefab = currentTheme.wallModels[UnityEngine.Random.Range(0, currentTheme.wallModels.Count)];

                    // Calculate position based on cell coordinates and the calculated start position
                    Vector3 position = new Vector3(
                        startPosition.x + cell.x * cellSize,
                        roomCenter.y, // Base at ground level
                        startPosition.z + cell.y * cellSize
                    );

                    // Calculate rotation based on wall direction
                    Quaternion rotation = Quaternion.identity;

                    // Special handling for corners
                    if (cornerCells.ContainsKey(cell) && cornerCells[cell] && cornerDirections.ContainsKey(cell))
                    {
                        // This is a corner with this direction as primary
                        var directions = cornerDirections[cell];

                        // Set rotation based on the corner type
                        switch (directions.primary)
                        {
                            case Direction.North when directions.secondary == Direction.East:
                                rotation = Quaternion.Euler(0, 0, 0);
                                break;
                            case Direction.East when directions.secondary == Direction.South:
                                rotation = Quaternion.Euler(0, 90, 0);
                                break;
                            case Direction.South when directions.secondary == Direction.West:
                                rotation = Quaternion.Euler(0, 180, 0);
                                break;
                            case Direction.West when directions.secondary == Direction.North:
                                rotation = Quaternion.Euler(0, 270, 0);
                                break;
                            // Handle other corner combinations
                            case Direction.North when directions.secondary == Direction.West:
                                rotation = Quaternion.Euler(0, 270, 0);
                                break;
                            case Direction.East when directions.secondary == Direction.North:
                                rotation = Quaternion.Euler(0, 0, 0);
                                break;
                            case Direction.South when directions.secondary == Direction.East:
                                rotation = Quaternion.Euler(0, 90, 0);
                                break;
                            case Direction.West when directions.secondary == Direction.South:
                                rotation = Quaternion.Euler(0, 180, 0);
                                break;
                            default:
                                // Default rotation for non-specific corners
                                switch (directions.primary)
                                {
                                    case Direction.North:
                                        rotation = Quaternion.Euler(0, 0, 0);
                                        break;
                                    case Direction.East:
                                        rotation = Quaternion.Euler(0, 90, 0);
                                        break;
                                    case Direction.South:
                                        rotation = Quaternion.Euler(0, 180, 0);
                                        break;
                                    case Direction.West:
                                        rotation = Quaternion.Euler(0, 270, 0);
                                        break;
                                }
                                break;
                        }
                    }
                    else
                    {
                        // Normal wall (not a corner)
                        switch (wall.direction)
                        {
                            case Direction.North:
                                rotation = Quaternion.Euler(0, 0, 0);
                                break;
                            case Direction.East:
                                rotation = Quaternion.Euler(0, 90, 0);
                                break;
                            case Direction.South:
                                rotation = Quaternion.Euler(0, 180, 0);
                                break;
                            case Direction.West:
                                rotation = Quaternion.Euler(0, 270, 0);
                                break;
                        }
                    }

                    // Create a temporary object to measure its bounds
                    GameObject tempWall = Instantiate(wallPrefab);
                    Bounds wallBounds = GetObjectBounds(tempWall);
                    DestroyImmediate(tempWall);

                    // Instantiate wall
                    GameObject wallObject = Instantiate(wallPrefab, position, rotation, directionParent.transform);

                    // Name the object to reflect if it's a corner
                    if (cornerCells.ContainsKey(cell) && cornerCells[cell] && cornerDirections.ContainsKey(cell))
                    {
                        var directions = cornerDirections[cell];
                        wallObject.name = $"Corner_{directions.primary}_{directions.secondary}_{cell.x}_{cell.y}";
                    }
                    else
                    {
                        wallObject.name = $"Wall_{wall.direction}_{cell.x}_{cell.y}";
                    }

                    // Store reference
                    wallObjects[cell] = wallObject;
                }
                else
                {
                    // Create doorway (placeholder for now)
                    GameObject doorwayParent = new GameObject($"Doorway_{wall.direction}_{cell.x}_{cell.y}");
                    doorwayParent.transform.parent = directionParent.transform;

                    // Calculate position based on cell coordinates and the calculated start position
                    Vector3 doorPosition = new Vector3(
                        startPosition.x + cell.x * cellSize,
                        roomCenter.y, // Base at ground level
                        startPosition.z + cell.y * cellSize
                    );

                    doorwayParent.transform.position = doorPosition;

                    // TODO: Implement actual doorway models
                }
            }
        }
    }

    // Helper method to get the bounds of an object including all children
    private Bounds GetObjectBounds(GameObject obj)
    {
        Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();

        if (renderers.Length > 0)
        {
            // Initialize with the first renderer's bounds
            bounds = renderers[0].bounds;

            // Expand to include all other renderers
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }
        }
        else
        {
            // Fallback if no renderers are found
            MeshFilter[] meshFilters = obj.GetComponentsInChildren<MeshFilter>();
            if (meshFilters.Length > 0 && meshFilters[0].sharedMesh != null)
            {
                bounds = meshFilters[0].sharedMesh.bounds;
                for (int i = 1; i < meshFilters.Length; i++)
                {
                    if (meshFilters[i].sharedMesh != null)
                    {
                        bounds.Encapsulate(meshFilters[i].sharedMesh.bounds);
                    }
                }
            }
        }

        return bounds;
    }

    public Vector3 CellToWorldPosition(Vector2Int cell)
    {
        // Convert cell coordinates to world position (using only bounds, not pivot points)
        // Ensure proper spacing based on cellSize between all objects
        return new Vector3(
            transform.position.x + cell.x * cellSize,  // Use direct multiplication without adjustment
            transform.position.y,
            transform.position.z + cell.y * cellSize   // Use direct multiplication without adjustment
        );
    }

    public Vector2Int WorldToCellPosition(Vector3 worldPos)
    {
        // Convert world position to cell coordinates based on the updated grid system
        float roomWorldWidth = size.x * cellSize;
        float roomWorldDepth = size.y * cellSize;

        // Calculate starting points - the room center is at transform.position
        float startX = transform.position.x - roomWorldWidth / 2 + cellSize / 2;
        float startZ = transform.position.z - roomWorldDepth / 2 + cellSize / 2;

        // Calculate the cell coordinates
        return new Vector2Int(
            Mathf.FloorToInt((worldPos.x - startX) / cellSize),
            Mathf.FloorToInt((worldPos.z - startZ) / cellSize)
        );
    }

    private WallData GetWallData(Direction direction)
    {
        foreach (WallData wall in walls)
        {
            if (wall.direction == direction)
            {
                return wall;
            }
        }

        // Should never happen if walls are initialized properly
        Debug.LogError($"Wall data not found for direction {direction}");
        return null;
    }

    public void CreateDoorway(Direction direction)
    {
        WallData wall = GetWallData(direction);

        if (wall != null && wall.wallCells.Count > 0)
        {
            // Choose a random position along the wall for the doorway
            int index = UnityEngine.Random.Range(0, wall.wallCells.Count);
            Vector2Int doorPosition = wall.wallCells[index];

            // Create a new doorway
            DoorwayData doorway = new DoorwayData
            {
                position = doorPosition,
                isConnected = false,
                connectedRoom = null
            };

            wall.doorways.Add(doorway);

            // Update the room visually
            UpdateRoomVisuals();
        }
    }

    private void UpdateRoomVisuals()
    {
        // Rebuild the room to reflect the changes
        BuildRoom();
    }

    public void UpdateRoomPosition()
    {
        // Update the visual representation after position changes
        UpdateRoomVisuals();
    }

    public Bounds GetRoomBounds()
    {
        // Calculate the bounds of the room in world space
        // The room's center is exactly at its transform position, with dimensions calculated from cellSize
        Vector3 center = transform.position + new Vector3(0, roomHeight / 2, 0);
        Vector3 size = new Vector3(this.size.x * cellSize, roomHeight, this.size.y * cellSize);

        // Debug information to verify bounds calculation
        Debug.Log($"Room bounds: Center={center}, Size={size}, " +
                 $"Min={center - size / 2}, Max={center + size / 2}, " +
                 $"CellSize={cellSize}, RoomDimensions={this.size}");

        return new Bounds(center, size);
    }
}