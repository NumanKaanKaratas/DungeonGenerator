using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class DungeonTheme
{
    public string themeName;
    public List<GameObject> wallModels = new List<GameObject>();
    public List<GameObject> floorModels = new List<GameObject>();
    public List<GameObject> ceilingModels = new List<GameObject>();
}

[Serializable]
public class FurnitureSet
{
    public string setName;
    public List<GameObject> furnitureModels = new List<GameObject>();
}

public enum GenerationMode
{
    ConstrainedArea,
    CompactArrangement
}

public class XLevelGenerator : MonoBehaviour
{
    [Header("Generation Settings")]
    public int seed;
    public int minRooms = 5;
    public int maxRooms = 15;
    public Vector2Int minRoomSize = new Vector2Int(3, 3);
    public Vector2Int maxRoomSize = new Vector2Int(8, 8);
    public float cellSize = 1.0f;
    public GameObject roomPrefab;

    [Header("Generation Mode")]
    public GenerationMode generationMode;
    public Vector2 constrainedAreaSize = new Vector2(500f, 500f);

    [Header("Themes")]
    public List<DungeonTheme> themes = new List<DungeonTheme>();
    public List<FurnitureSet> furnitureSets = new List<FurnitureSet>();

    private List<XRoom> generatedRooms = new List<XRoom>();
    private System.Random randomGenerator;

    public void GenerateDungeon()
    {

        if (roomPrefab is null)
            return;

        ClearExistingDungeon();

        // Initialize random with seed
        randomGenerator = new System.Random(seed);

        // Determine number of rooms to generate
        int roomCount = randomGenerator.Next(minRooms, maxRooms + 1);

        if (generationMode == GenerationMode.ConstrainedArea)
        {
            GenerateRoomsInConstrainedArea(roomCount);
        }
        else
        {
            GenerateCompactRooms(roomCount);
        }

        // Connect rooms
        ConnectRooms();
    }

    private void ClearExistingDungeon()
    {
        // Destroy all existing rooms
        foreach (var room in generatedRooms)
        {
            if (room != null)
            {
                DestroyImmediate(room.gameObject);
            }
        }

        generatedRooms.Clear();
    }

    private void GenerateRoomsInConstrainedArea(int roomCount)
    {
        for (int i = 0; i < roomCount; i++)
        {
            // Generate random room size
            Vector2Int roomSize = new Vector2Int(
                randomGenerator.Next(minRoomSize.x, maxRoomSize.x + 1),
                randomGenerator.Next(minRoomSize.y, maxRoomSize.y + 1)
            );

            // Find a random position within the constrained area
            Vector3 position = new Vector3(
                (float)randomGenerator.NextDouble() * constrainedAreaSize.x - constrainedAreaSize.x / 2,
                0,
                (float)randomGenerator.NextDouble() * constrainedAreaSize.y - constrainedAreaSize.y / 2
            );

            // Create the room
            CreateRoom(position, roomSize);
        }
    }

    private void GenerateCompactRooms(int roomCount)
    {
        List<Rect> roomRects = new List<Rect>();

        for (int i = 0; i < roomCount; i++)
        {
            // Generate random room size
            Vector2Int roomSize = new Vector2Int(
                randomGenerator.Next(minRoomSize.x, maxRoomSize.x + 1),
                randomGenerator.Next(minRoomSize.y, maxRoomSize.y + 1)
            );

            // Initial position (will be adjusted)
            Vector3 position = Vector3.zero;

            if (i > 0)
            {
                // Try to place the room close to an existing room
                int attempts = 0;
                bool validPlacement = false;

                while (!validPlacement && attempts < 50)
                {
                    // Choose a random existing room
                    int referenceRoomIndex = randomGenerator.Next(0, roomRects.Count);
                    Rect referenceRect = roomRects[referenceRoomIndex];

                    // Choose a random side of the reference room
                    int side = randomGenerator.Next(0, 4);

                    // Calculate position based on the side
                    float roomWidth = roomSize.x * cellSize;
                    float roomHeight = roomSize.y * cellSize;

                    switch (side)
                    {
                        case 0: // Top
                            position = new Vector3(
                                referenceRect.center.x - roomWidth / 2 + (float)randomGenerator.NextDouble() * (referenceRect.width + roomWidth) - roomWidth / 2,
                                0,
                                referenceRect.yMax
                            );
                            break;
                        case 1: // Right
                            position = new Vector3(
                                referenceRect.xMax,
                                0,
                                referenceRect.center.y - roomHeight / 2 + (float)randomGenerator.NextDouble() * (referenceRect.height + roomHeight) - roomHeight / 2
                            );
                            break;
                        case 2: // Bottom
                            position = new Vector3(
                                referenceRect.center.x - roomWidth / 2 + (float)randomGenerator.NextDouble() * (referenceRect.width + roomWidth) - roomWidth / 2,
                                0,
                                referenceRect.yMin - roomHeight
                            );
                            break;
                        case 3: // Left
                            position = new Vector3(
                                referenceRect.xMin - roomWidth,
                                0,
                                referenceRect.center.y - roomHeight / 2 + (float)randomGenerator.NextDouble() * (referenceRect.height + roomHeight) - roomHeight / 2
                            );
                            break;
                    }

                    // Create a rect for the new room
                    Rect newRect = new Rect(
                        position.x,
                        position.z,
                        roomSize.x * cellSize,
                        roomSize.y * cellSize
                    );

                    // Check if new room overlaps with existing rooms
                    validPlacement = true;
                    foreach (var existingRect in roomRects)
                    {
                        if (newRect.Overlaps(existingRect))
                        {
                            validPlacement = false;
                            break;
                        }
                    }

                    if (validPlacement)
                    {
                        roomRects.Add(newRect);
                    }

                    attempts++;
                }

                if (!validPlacement)
                {
                    // If couldn't find a valid position after max attempts, just place it randomly
                    position = new Vector3(
                        (float)randomGenerator.NextDouble() * 50 - 25,
                        0,
                        (float)randomGenerator.NextDouble() * 50 - 25
                    );

                    Rect newRect = new Rect(
                        position.x,
                        position.z,
                        roomSize.x * cellSize,
                        roomSize.y * cellSize
                    );

                    roomRects.Add(newRect);
                }
            }
            else
            {
                // First room is placed at origin
                Rect newRect = new Rect(
                    -roomSize.x * cellSize / 2,
                    -roomSize.y * cellSize / 2,
                    roomSize.x * cellSize,
                    roomSize.y * cellSize
                );

                roomRects.Add(newRect);
            }

            // Create the room using the final position
            CreateRoom(new Vector3(roomRects[i].x, 0, roomRects[i].y), roomSize);
        }

        // Adjust room positions to remove overlapping walls
        OptimizeRoomPositions();
    }

    private void CreateRoom(Vector3 position, Vector2Int size)
    {
        GameObject roomObject = Instantiate(roomPrefab, position, Quaternion.identity);
        roomObject.transform.parent = transform;

        XRoom room = roomObject.GetComponent<XRoom>();
        if (room != null)
        {
            room.Initialize(size, cellSize);

            // Assign a random theme from available themes
            if (themes.Count > 0)
            {
                int themeIndex = randomGenerator.Next(0, themes.Count);
                room.AssignTheme(themes[themeIndex]);
            }

            // Assign a random furniture set
            if (furnitureSets.Count > 0)
            {
                int setIndex = randomGenerator.Next(0, furnitureSets.Count);
                room.AssignFurnitureSet(furnitureSets[setIndex]);
            }

            room.BuildRoom();
            generatedRooms.Add(room);
        }
    }

    private void OptimizeRoomPositions()
    {
        bool changes;
        int iterations = 0;
        int maxIterations = 10;

        do
        {
            changes = false;
            iterations++;

            for (int i = 0; i < generatedRooms.Count; i++)
            {
                for (int j = i + 1; j < generatedRooms.Count; j++)
                {
                    XRoom roomA = generatedRooms[i];
                    XRoom roomB = generatedRooms[j];

                    // Calculate room bounds
                    Bounds boundsA = roomA.GetRoomBounds();
                    Bounds boundsB = roomB.GetRoomBounds();

                    // Check for potential wall sharing
                    Vector3 direction = boundsB.center - boundsA.center;

                    // If rooms are close enough in x or z direction
                    if (Mathf.Abs(direction.x) < (boundsA.size.x / 2 + boundsB.size.x / 2 + cellSize) &&
                        Mathf.Abs(direction.z) < (boundsA.size.z / 2 + boundsB.size.z / 2 + cellSize))
                    {
                        // Determine the dominant axis
                        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
                        {
                            // X-axis is dominant, adjust on x
                            float targetDistance = (boundsA.size.x / 2 + boundsB.size.x / 2);
                            float moveAmount = targetDistance - Mathf.Abs(direction.x);

                            if (moveAmount > 0 && moveAmount < cellSize * 2)
                            {
                                Vector3 moveDir = new Vector3(Mathf.Sign(direction.x), 0, 0);
                                roomA.transform.position -= moveDir * moveAmount / 2;
                                roomB.transform.position += moveDir * moveAmount / 2;
                                roomA.UpdateRoomPosition();
                                roomB.UpdateRoomPosition();
                                changes = true;
                            }
                        }
                        else
                        {
                            // Z-axis is dominant, adjust on z
                            float targetDistance = (boundsA.size.z / 2 + boundsB.size.z / 2);
                            float moveAmount = targetDistance - Mathf.Abs(direction.z);

                            if (moveAmount > 0 && moveAmount < cellSize * 2)
                            {
                                Vector3 moveDir = new Vector3(0, 0, Mathf.Sign(direction.z));
                                roomA.transform.position -= moveDir * moveAmount / 2;
                                roomB.transform.position += moveDir * moveAmount / 2;
                                roomA.UpdateRoomPosition();
                                roomB.UpdateRoomPosition();
                                changes = true;
                            }
                        }
                    }
                }
            }
        } while (changes && iterations < maxIterations);
    }

    private void ConnectRooms()
    {
        // Get minimum spanning tree to ensure all rooms are connected
        List<RoomConnection> connections = GetMinimumSpanningTree();

        // Create doorways between connected rooms
        foreach (var connection in connections)
        {
            CreateDoorBetweenRooms(connection.roomA, connection.roomB);
        }
    }

    private List<RoomConnection> GetMinimumSpanningTree()
    {
        // Implement Kruskal's algorithm for minimum spanning tree
        List<RoomConnection> allPossibleConnections = new List<RoomConnection>();

        // Create all possible connections between rooms
        for (int i = 0; i < generatedRooms.Count; i++)
        {
            for (int j = i + 1; j < generatedRooms.Count; j++)
            {
                XRoom roomA = generatedRooms[i];
                XRoom roomB = generatedRooms[j];

                float distance = Vector3.Distance(roomA.transform.position, roomB.transform.position);
                allPossibleConnections.Add(new RoomConnection(roomA, roomB, distance));
            }
        }

        // Sort connections by distance
        allPossibleConnections.Sort((a, b) => a.distance.CompareTo(b.distance));

        // Apply Kruskal's algorithm
        DisjointSet<XRoom> disjointSet = new DisjointSet<XRoom>(generatedRooms);
        List<RoomConnection> minimumSpanningTree = new List<RoomConnection>();

        foreach (var connection in allPossibleConnections)
        {
            if (disjointSet.Find(connection.roomA) != disjointSet.Find(connection.roomB))
            {
                minimumSpanningTree.Add(connection);
                disjointSet.Union(connection.roomA, connection.roomB);
            }
        }

        return minimumSpanningTree;
    }

    private void CreateDoorBetweenRooms(XRoom roomA, XRoom roomB)
    {
        // Determine the closest walls between the two rooms
        Vector3 direction = roomB.transform.position - roomA.transform.position;

        Direction wallDirectionA;
        Direction wallDirectionB;

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
        {
            // X-axis is dominant
            if (direction.x > 0)
            {
                wallDirectionA = Direction.East;
                wallDirectionB = Direction.West;
            }
            else
            {
                wallDirectionA = Direction.West;
                wallDirectionB = Direction.East;
            }
        }
        else
        {
            // Z-axis is dominant
            if (direction.z > 0)
            {
                wallDirectionA = Direction.North;
                wallDirectionB = Direction.South;
            }
            else
            {
                wallDirectionA = Direction.South;
                wallDirectionB = Direction.North;
            }
        }

        // Create doorways
        roomA.CreateDoorway(wallDirectionA);
        roomB.CreateDoorway(wallDirectionB);
    }
}

// Helper class for connections between rooms
public class RoomConnection
{
    public XRoom roomA;
    public XRoom roomB;
    public float distance;

    public RoomConnection(XRoom roomA, XRoom roomB, float distance)
    {
        this.roomA = roomA;
        this.roomB = roomB;
        this.distance = distance;
    }
}

// Helper class for Kruskal's algorithm
public class DisjointSet<T>
{
    private Dictionary<T, T> parent = new Dictionary<T, T>();
    private Dictionary<T, int> rank = new Dictionary<T, int>();

    public DisjointSet(List<T> elements)
    {
        foreach (var element in elements)
        {
            parent[element] = element;
            rank[element] = 0;
        }
    }

    public T Find(T x)
    {
        if (!parent.ContainsKey(x))
        {
            parent[x] = x;
            rank[x] = 0;
            return x;
        }

        if (!parent[x].Equals(x))
        {
            parent[x] = Find(parent[x]);
        }

        return parent[x];
    }

    public void Union(T x, T y)
    {
        T xRoot = Find(x);
        T yRoot = Find(y);

        if (xRoot.Equals(yRoot))
        {
            return;
        }

        if (rank[xRoot] < rank[yRoot])
        {
            parent[xRoot] = yRoot;
        }
        else if (rank[xRoot] > rank[yRoot])
        {
            parent[yRoot] = xRoot;
        }
        else
        {
            parent[yRoot] = xRoot;
            rank[xRoot]++;
        }
    }
}