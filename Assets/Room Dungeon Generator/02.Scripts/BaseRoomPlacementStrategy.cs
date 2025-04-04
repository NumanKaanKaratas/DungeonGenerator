using System.Collections.Generic;
using UnityEngine;

namespace ooparts.dungen
{
    // T�m yerle�tirme stratejileri i�in temel s�n�f
    public abstract class BaseRoomPlacementStrategy : IRoomPlacementStrategy
    {
        // Yerle�tirilen odalar�n listesi
        protected List<Room> _rooms = new List<Room>();

        // Oda yerle�tirme i�lemini ger�ekle�tir
        public abstract List<Room> PlaceRooms(Transform parent, MapGrid mapGrid, MapSettings mapSettings);

        // Belirtilen pozisyonda oda olu�tur
        protected Room CreateRoomAtPosition(Transform parent, MapGrid mapGrid, MapSettings mapSettings, IntVector2 size, IntVector2 coordinates)
        {
            if (!IsOverlapped(size, coordinates))
            {
                Room newRoom = Object.Instantiate(mapSettings.RoomPrefab);
                _rooms.Add(newRoom);

                newRoom.Num = _rooms.Count;
                newRoom.name = "Room " + newRoom.Num + " (" + coordinates.x + ", " + coordinates.z + ")";
                newRoom.Size = size;
                newRoom.Coordinates = coordinates;
                newRoom.transform.parent = parent;

                Vector3 position = mapGrid.CoordinatesToPosition(coordinates);
                position.x += size.x * 0.5f - 0.5f;
                position.z += size.z * 0.5f - 0.5f;
                position *= RoomMapManager.TileSize;
                newRoom.transform.localPosition = position;

                return newRoom;
            }

            return null;
        }

        // Oda �ak��ma kontrol�
        protected bool IsOverlapped(IntVector2 size, IntVector2 coordinates)
        {
            foreach (Room room in _rooms)
            {
                // Odalar aras�nda biraz bo�luk b�rak
                if (Mathf.Abs(room.Coordinates.x - coordinates.x + (room.Size.x - size.x) * 0.5f) < (room.Size.x + size.x) * 0.7f &&
                    Mathf.Abs(room.Coordinates.z - coordinates.z + (room.Size.z - size.z) * 0.5f) < (room.Size.z + size.z) * 0.7f)
                {
                    return true;
                }
            }
            return false;
        }

        // Rasgele oda boyutu olu�tur
        protected IntVector2 CreateRandomRoomSize(MapSettings mapSettings)
        {
            return new IntVector2(
                Random.Range(mapSettings.RoomSize.Min, mapSettings.RoomSize.Max + 1),
                Random.Range(mapSettings.RoomSize.Min, mapSettings.RoomSize.Max + 1)
            );
        }
    }
}