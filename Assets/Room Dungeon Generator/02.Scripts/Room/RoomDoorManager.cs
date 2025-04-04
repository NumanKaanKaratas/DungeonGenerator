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

        // Kap� bilgilerini tutan yap�
        public class DoorInfo
        {
            public MapDirection Direction;
            public Room ConnectedRoom;
            public List<IntVector2> DoorTiles = new List<IntVector2>();
        }

        // Kap� bilgilerini saklamak i�in liste ve s�zl�k
        private List<DoorInfo> _doorInfos = new List<DoorInfo>();
        private Dictionary<MapDirection, Room> _doors = new Dictionary<MapDirection, Room>();

        public RoomDoorManager(Room room, MapManager map)
        {
            _room = room;
            _map = map;
        }

        // Oda i�in kap� konumu olu�tur
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

        // Yeni kap� ba�lant�s� olu�tur
        public void CreateDoorConnection(MapDirection direction, Room connectedRoom, int doorWidth)
        {
            // Bu y�nde zaten kap� var m� kontrol et
            for (int i = 0; i < _doorInfos.Count; i++)
            {
                if (_doorInfos[i].Direction == direction)
                {
                    // Varsa g�ncelle
                    _doorInfos[i].ConnectedRoom = connectedRoom;
                    _doorInfos[i].DoorTiles.Clear();

                    // Yeni kap� karelerini ekle
                    AddDoorTiles(_doorInfos[i], doorWidth);
                    return;
                }
            }

            // Yeni kap� bilgisi olu�tur
            DoorInfo doorInfo = new DoorInfo
            {
                Direction = direction,
                ConnectedRoom = connectedRoom
            };

            // Kap� karelerini ekle
            AddDoorTiles(doorInfo, doorWidth);

            // Listeye ekle
            _doorInfos.Add(doorInfo);

            // Kap�y� _doors s�zl���ne de ekle (eski sistem ile uyumluluk i�in)
            if (!_doors.ContainsKey(direction))
            {
                _doors[direction] = connectedRoom;
            }
        }

        // Klasik tek kap� olu�turma (eski y�ntem - uyumluluk i�in)
        public void CreateDoor(MapDirection direction, Room connectedRoom)
        {
            // Eski kap�y� kald�r, e�er varsa
            if (_doors.ContainsKey(direction))
            {
                _doors.Remove(direction);
            }

            // Yeni kap� ekle
            _doors[direction] = connectedRoom;

            // Kap� pozisyonunu al ve kap� olarak i�aretle
            IntVector2 doorPos = CreateDoorPosition(direction);

            // Koordinatlar�n harita s�n�rlar� i�inde oldu�undan emin ol
            if (_map.IsValidCoordinate(doorPos))
            {
                _map.SetTileType(doorPos, TileType.Door);
            }
        }

        // Kap� karelerini ekle
        private void AddDoorTiles(DoorInfo doorInfo, int doorWidth)
        {
            // Duvar�n ortas�nda kap�n�n ba�lang�� pozisyonunu hesapla
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

            // Kap� karelerini ekle - y�n�ne g�re
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

                // Bu pozisyonu kap� olarak i�aretle
                if (_map.IsValidCoordinate(doorPos))
                {
                    // E�er bu pozisyon bo�sa veya duvara d�n��t�r�lm��se, kap� olarak ayarla
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

        // Bir pozisyonun kap� karesi olup olmad���n� kontrol et
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