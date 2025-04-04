using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ooparts.dungen
{
    /// <summary>
    /// Doðrudan kapý baðlantý stratejisi
    /// </summary>
    public class DirectDoorConnectionStrategy : IConnectionStrategy
    {
        private MapGrid _mapGrid;
        private MapSettings _mapSettings;
        private MonoBehaviour _monoBehaviour;

        public DirectDoorConnectionStrategy(
            MapGrid mapGrid,
            MapSettings mapSettings,
            MonoBehaviour monoBehaviour)
        {
            _mapGrid = mapGrid;
            _mapSettings = mapSettings;
            _monoBehaviour = monoBehaviour;
        }

        /// <summary>
        /// Odalarý doðrudan kapýlarla baðla
        /// </summary>
        public IEnumerator CreateConnections(List<Room> rooms, List<Corridor> corridors)
        {
            Debug.Log($"Processing connections for {rooms.Count} rooms");

            List<Room> connectableRooms = new List<Room>(rooms);

            while (connectableRooms.Count > 1)
            {
                Room primaryRoom = connectableRooms[0];
                connectableRooms.RemoveAt(0);

                var potentialConnections = FindPotentialConnections(primaryRoom, connectableRooms);

                var connectionsToMake = _mapSettings.AllowMultiRoomConnections
                    ? potentialConnections.Take(_mapSettings.MaxRoomConnectionCount)
                    : potentialConnections.Take(1);

                foreach (var targetRoom in connectionsToMake)
                {
                    yield return _monoBehaviour.StartCoroutine(
                        ConnectRoomsDirectly(primaryRoom, targetRoom)
                    );

                    connectableRooms.Remove(targetRoom);
                }
            }

            Debug.Log("Direct room connections completed");
        }

        /// <summary>
        /// Potansiyel baðlantý odalarýný bul
        /// </summary>
        private List<Room> FindPotentialConnections(Room primaryRoom, List<Room> rooms)
        {
            return rooms
                .OrderBy(r => Vector3.Distance(primaryRoom.transform.position, r.transform.position))
                .ToList();
        }

        /// <summary>
        /// Ýki odayý doðrudan baðla
        /// </summary>
        private IEnumerator ConnectRoomsDirectly(Room room1, Room room2)
        {
            // Orijinal pozisyonlarý kaydet
            IntVector2 originalPos1 = new IntVector2(room1.Coordinates.x, room1.Coordinates.z);
            IntVector2 originalPos2 = new IntVector2(room2.Coordinates.x, room2.Coordinates.z);

            // Baðlantý yönünü belirle
            MapDirection connectionDirection = DetermineConnectionDirection(room1, room2);

            // Kapý geniþliðini belirle
            int doorWidth = Random.Range(
                _mapSettings.CorridorWidth.Min,
                _mapSettings.CorridorWidth.Max + 1
            );

            // Odalarý yakýnlaþtýr ve hizala
            bool connectionSuccessful = AlignRoomsForConnection(room1, room2, connectionDirection, doorWidth);

            if (connectionSuccessful)
            {
                // Kapýlarý oluþtur
                room1.CreateDoorConnection(connectionDirection, room2, doorWidth);
                room2.CreateDoorConnection(connectionDirection.GetOpposite(), room1, doorWidth);

                // Tile'larý yeniden oluþtur
                room1.RecreateTiles();
                room2.RecreateTiles();

                // Fazladan duvarlarý kaldýr
                RemoveRedundantWalls(room1, room2, connectionDirection);
            }
            else
            {
                // Baðlantý baþarýsýz olursa eski konumlara geri dön
                room1.Coordinates = originalPos1;
                room2.Coordinates = originalPos2;
                UpdateRoomTransform(room1);
                UpdateRoomTransform(room2);
            }

            yield return null;
        }

        /// <summary>
        /// Baðlantý yönünü belirle
        /// </summary>
        private MapDirection DetermineConnectionDirection(Room room1, Room room2)
        {
            Vector3 direction = room2.transform.position - room1.transform.position;

            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
            {
                return direction.x > 0 ? MapDirection.East : MapDirection.West;
            }
            else
            {
                return direction.z > 0 ? MapDirection.North : MapDirection.South;
            }
        }

        /// <summary>
        /// Odalarý baðlantý için hizala
        /// </summary>
        private bool AlignRoomsForConnection(Room room1, Room room2, MapDirection connectionDirection, int doorWidth)
        {
            ClearRoomTiles(room1);
            ClearRoomTiles(room2);

            IntVector2 offset1 = new IntVector2(0, 0);
            IntVector2 offset2 = new IntVector2(0, 0);

            switch (connectionDirection)
            {
                case MapDirection.North:
                    offset2.z = room1.Coordinates.z + room1.Size.z - room2.Coordinates.z + 1;
                    offset2.x = CalculateHorizontalAlignment(room1, room2);
                    break;

                case MapDirection.East:
                    offset2.x = room1.Coordinates.x + room1.Size.x - room2.Coordinates.x + 1;
                    offset2.z = CalculateVerticalAlignment(room1, room2);
                    break;

                case MapDirection.South:
                    offset1.z = room2.Coordinates.z + room2.Size.z - room1.Coordinates.z + 1;
                    offset1.x = CalculateHorizontalAlignment(room2, room1);
                    break;

                case MapDirection.West:
                    offset1.x = room2.Coordinates.x + room2.Size.x - room1.Coordinates.x + 1;
                    offset1.z = CalculateVerticalAlignment(room2, room1);
                    break;
            }

            // Odalarýn pozisyonlarýný güncelle
            room1.Coordinates.x += offset1.x;
            room1.Coordinates.z += offset1.z;
            UpdateRoomTransform(room1);

            room2.Coordinates.x += offset2.x;
            room2.Coordinates.z += offset2.z;
            UpdateRoomTransform(room2);

            MarkRoomTiles(room1);
            MarkRoomTiles(room2);

            return true;
        }

        /// <summary>
        /// Yatay hizalamayý hesapla
        /// </summary>
        private int CalculateHorizontalAlignment(Room room1, Room room2)
        {
            int center1X = room1.Coordinates.x + room1.Size.x / 2;
            int center2X = room2.Coordinates.x + room2.Size.x / 2;
            return center1X - center2X;
        }

        /// <summary>
        /// Dikey hizalamayý hesapla
        /// </summary>
        private int CalculateVerticalAlignment(Room room1, Room room2)
        {
            int center1Z = room1.Coordinates.z + room1.Size.z / 2;
            int center2Z = room2.Coordinates.z + room2.Size.z / 2;
            return center1Z - center2Z;
        }

        /// <summary>
        /// Fazladan duvarlarý kaldýr
        /// </summary>
        private void RemoveRedundantWalls(Room room1, Room room2, MapDirection connectionDirection)
        {
            // Baðlantý yönüne göre gereksiz duvarlarý tespit et ve kaldýr
            switch (connectionDirection)
            {
                case MapDirection.North:
                    RemoveWall(room1, MapDirection.North);
                    RemoveWall(room2, MapDirection.South);
                    break;
                case MapDirection.East:
                    RemoveWall(room1, MapDirection.East);
                    RemoveWall(room2, MapDirection.West);
                    break;
                case MapDirection.South:
                    RemoveWall(room1, MapDirection.South);
                    RemoveWall(room2, MapDirection.North);
                    break;
                case MapDirection.West:
                    RemoveWall(room1, MapDirection.West);
                    RemoveWall(room2, MapDirection.East);
                    break;
            }
        }

        // Önceki metodlar ayný kalacak...

        /// <summary>
        /// Belirli bir yöndeki duvarý kaldýr
        /// </summary>
        private void RemoveWall(Room room, MapDirection direction)
        {
            // Duvar kaldýrma için koordinat hesaplamalarý
            IntVector2 wallStart = GetWallStartCoordinate(room, direction);
            int wallLength = GetWallLength(room, direction);

            // Duvar karelerini temizle
            for (int i = 0; i < wallLength; i++)
            {
                IntVector2 currentWallTile = GetAdjacentWallTile(wallStart, direction, i);

                // Boþ veya duvar olan tile'larý temizle
                if (_mapGrid.IsValidCoordinate(currentWallTile))
                {
                    TileType currentType = _mapGrid.GetTileType(currentWallTile);
                    if (currentType == TileType.Wall || currentType == TileType.Empty)
                    {
                        _mapGrid.SetTileType(currentWallTile, TileType.Empty);
                    }
                }
            }
        }

        /// <summary>
        /// Odanýn belirli bir yöndeki duvar baþlangýç koordinatýný hesapla
        /// </summary>
        private IntVector2 GetWallStartCoordinate(Room room, MapDirection direction)
        {
            switch (direction)
            {
                case MapDirection.North:
                    return new IntVector2(room.Coordinates.x, room.Coordinates.z + room.Size.z);
                case MapDirection.East:
                    return new IntVector2(room.Coordinates.x + room.Size.x, room.Coordinates.z);
                case MapDirection.South:
                    return new IntVector2(room.Coordinates.x, room.Coordinates.z - 1);
                case MapDirection.West:
                    return new IntVector2(room.Coordinates.x - 1, room.Coordinates.z);
                default:
                    return room.Coordinates;
            }
        }

        /// <summary>
        /// Odanýn belirli bir yöndeki duvar uzunluðunu hesapla
        /// </summary>
        private int GetWallLength(Room room, MapDirection direction)
        {
            switch (direction)
            {
                case MapDirection.North:
                case MapDirection.South:
                    return room.Size.x;
                case MapDirection.East:
                case MapDirection.West:
                    return room.Size.z;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Duvar tile'ýnýn komþu koordinatýný hesapla
        /// </summary>
        private IntVector2 GetAdjacentWallTile(IntVector2 wallStart, MapDirection direction, int offset)
        {
            switch (direction)
            {
                case MapDirection.North:
                    return new IntVector2(wallStart.x + offset, wallStart.z);
                case MapDirection.East:
                    return new IntVector2(wallStart.x, wallStart.z + offset);
                case MapDirection.South:
                    return new IntVector2(wallStart.x + offset, wallStart.z);
                case MapDirection.West:
                    return new IntVector2(wallStart.x, wallStart.z + offset);
                default:
                    return wallStart;
            }
        }

        /// <summary>
        /// Odanýn karelerini temizle
        /// </summary>
        private void ClearRoomTiles(Room room)
        {
            for (int x = 0; x < room.Size.x; x++)
            {
                for (int z = 0; z < room.Size.z; z++)
                {
                    IntVector2 tileCoord = new IntVector2(room.Coordinates.x + x, room.Coordinates.z + z);
                    if (_mapGrid.IsValidCoordinate(tileCoord) &&
                        _mapGrid.GetTileType(tileCoord) == TileType.Room)
                    {
                        _mapGrid.SetTileType(tileCoord, TileType.Empty);
                    }
                }
            }
        }

        /// <summary>
        /// Odayý harita karelerinde iþaretle
        /// </summary>
        private void MarkRoomTiles(Room room)
        {
            for (int x = 0; x < room.Size.x; x++)
            {
                for (int z = 0; z < room.Size.z; z++)
                {
                    IntVector2 tileCoord = new IntVector2(room.Coordinates.x + x, room.Coordinates.z + z);
                    if (_mapGrid.IsValidCoordinate(tileCoord))
                    {
                        _mapGrid.SetTileType(tileCoord, TileType.Room);
                    }
                }
            }
        }

        /// <summary>
        /// Oda transform pozisyonunu güncelle
        /// </summary>
        private void UpdateRoomTransform(Room room)
        {
            Vector3 position = _mapGrid.CoordinatesToPosition(room.Coordinates);
            position.x += room.Size.x * 0.5f - 0.5f;
            position.z += room.Size.z * 0.5f - 0.5f;
            position *= RoomMapManager.TileSize;
            room.transform.localPosition = position;
        }
    }
}