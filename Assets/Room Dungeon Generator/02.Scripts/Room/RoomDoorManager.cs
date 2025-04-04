using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ooparts.dungen;

namespace ooparts.dungen
{
    public class RoomDoorManager
    {
        private Room _room;
        private MapManager _map;

        // Kapý bilgilerini tutan yapý
        public class DoorInfo
        {
            public MapDirection Direction;
            public Room ConnectedRoom;
            public List<IntVector2> DoorTiles = new List<IntVector2>();
        }

        // Kapý bilgilerini saklamak için liste ve sözlük
        private List<DoorInfo> _doorInfos = new List<DoorInfo>();
        private Dictionary<MapDirection, Room> _doors = new Dictionary<MapDirection, Room>();

        public RoomDoorManager(Room room, MapManager map)
        {
            _room = room;
            _map = map;
        }

        // Oda için kapý konumu oluþtur
        public IntVector2 CreateDoorPosition(MapDirection direction)
        {
            IntVector2 doorPos = new IntVector2();

            switch (direction)
            {
                case MapDirection.North:
                    doorPos.x = _room.Coordinates.x + _room.Size.x / 2;
                    doorPos.z = _room.Coordinates.z + _room.Size.z - 1;
                    break;
                case MapDirection.East:
                    doorPos.x = _room.Coordinates.x + _room.Size.x - 1;
                    doorPos.z = _room.Coordinates.z + _room.Size.z / 2;
                    break;
                case MapDirection.South:
                    doorPos.x = _room.Coordinates.x + _room.Size.x / 2;
                    doorPos.z = _room.Coordinates.z;
                    break;
                case MapDirection.West:
                    doorPos.x = _room.Coordinates.x;
                    doorPos.z = _room.Coordinates.z + _room.Size.z / 2;
                    break;
            }

            return doorPos;
        }

        // Yeni kapý baðlantýsý oluþtur
        public void CreateDoorConnection(MapDirection direction, Room connectedRoom, int doorWidth)
        {
            // Bu yönde zaten kapý var mý kontrol et
            for (int i = 0; i < _doorInfos.Count; i++)
            {
                if (_doorInfos[i].Direction == direction)
                {
                    // Varsa güncelle
                    _doorInfos[i].ConnectedRoom = connectedRoom;
                    _doorInfos[i].DoorTiles.Clear();

                    // Yeni kapý karelerini ekle
                    AddDoorTiles(_doorInfos[i], doorWidth);
                    return;
                }
            }

            // Yeni kapý bilgisi oluþtur
            DoorInfo doorInfo = new DoorInfo
            {
                Direction = direction,
                ConnectedRoom = connectedRoom
            };

            // Kapý karelerini ekle
            AddDoorTiles(doorInfo, doorWidth);

            // Listeye ekle
            _doorInfos.Add(doorInfo);

            // Kapýyý _doors sözlüðüne de ekle (eski sistem ile uyumluluk için)
            if (!_doors.ContainsKey(direction))
            {
                _doors[direction] = connectedRoom;
            }
        }

        // Klasik tek kapý oluþturma (eski yöntem - uyumluluk için)
        public void CreateDoor(MapDirection direction, Room connectedRoom)
        {
            // Eski kapýyý kaldýr, eðer varsa
            if (_doors.ContainsKey(direction))
            {
                _doors.Remove(direction);
            }

            // Yeni kapý ekle
            _doors[direction] = connectedRoom;

            // Kapý pozisyonunu al ve kapý olarak iþaretle
            IntVector2 doorPos = CreateDoorPosition(direction);

            // Koordinatlarýn harita sýnýrlarý içinde olduðundan emin ol
            if (_map.IsValidCoordinate(doorPos))
            {
                _map.SetTileType(doorPos, TileType.Door);
            }
        }

        // Kapý karelerini ekle
        private void AddDoorTiles(DoorInfo doorInfo, int doorWidth)
        {
            // Duvarýn ortasýnda kapýnýn baþlangýç pozisyonunu hesapla
            IntVector2 doorStartPos = new IntVector2();
            int halfWidth = doorWidth / 2;

            switch (doorInfo.Direction)
            {
                case MapDirection.North:
                    doorStartPos.x = _room.Coordinates.x + _room.Size.x / 2 - halfWidth;
                    doorStartPos.z = _room.Coordinates.z + _room.Size.z;
                    break;

                case MapDirection.East:
                    doorStartPos.x = _room.Coordinates.x + _room.Size.x;
                    doorStartPos.z = _room.Coordinates.z + _room.Size.z / 2 - halfWidth;
                    break;

                case MapDirection.South:
                    doorStartPos.x = _room.Coordinates.x + _room.Size.x / 2 - halfWidth;
                    doorStartPos.z = _room.Coordinates.z - 1;
                    break;

                case MapDirection.West:
                    doorStartPos.x = _room.Coordinates.x - 1;
                    doorStartPos.z = _room.Coordinates.z + _room.Size.z / 2 - halfWidth;
                    break;
            }

            // Kapý karelerini ekle - yönüne göre
            for (int i = 0; i < doorWidth; i++)
            {
                IntVector2 doorPos;

                switch (doorInfo.Direction)
                {
                    case MapDirection.North:
                    case MapDirection.South:
                        doorPos = new IntVector2(doorStartPos.x + i, doorStartPos.z);
                        break;
                    default: // East or West
                        doorPos = new IntVector2(doorStartPos.x, doorStartPos.z + i);
                        break;
                }

                doorInfo.DoorTiles.Add(doorPos);

                // Bu pozisyonu kapý olarak iþaretle
                if (_map.IsValidCoordinate(doorPos))
                {
                    // Eðer bu pozisyon boþsa veya duvara dönüþtürülmüþse, kapý olarak ayarla
                    TileType currentType = _map.GetTileType(doorPos);
                    if (currentType == TileType.Empty || currentType == TileType.Wall)
                    {
                        _map.SetTileType(doorPos, TileType.Door);
                    }
                }
            }
        }

        public List<DoorInfo> GetDoorInfos()
        {
            return _doorInfos;
        }

        // Bir pozisyonun kapý karesi olup olmadýðýný kontrol et
        public bool IsDoorTile(IntVector2 pos)
        {
            foreach (DoorInfo doorInfo in _doorInfos)
            {
                foreach (IntVector2 doorTile in doorInfo.DoorTiles)
                {
                    if (doorTile.x == pos.x && doorTile.z == pos.z)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

    }
}