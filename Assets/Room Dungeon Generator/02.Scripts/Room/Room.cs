using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ooparts.dungen;

namespace ooparts.dungen
{
    public class Room : MonoBehaviour
    {
        // Bağlantılar
        public Corridor CorridorPrefab;
        public Dictionary<Room, Corridor> RoomCorridor = new Dictionary<Room, Corridor>();

        // Oda özellikleri
        public IntVector2 Size;
        public IntVector2 Coordinates;
        public int Num;
        public RoomSetting Setting;

        // Tile ve duvar prefabları
        public Tile TilePrefab;
        public GameObject WallPrefab;
        public GameObject DoorPrefab;

        // Oyuncu ve canavar prefabları
        public GameObject PlayerPrefab;
        public GameObject MonsterPrefab;
        public int MonsterCount;

        // Alt yönetici sınıflar
        private RoomTileManager _tileManager;
        private RoomDoorManager _doorManager;
        private RoomWallManager _wallManager;
        private RoomEntityManager _entityManager;

        // Harita yöneticisi
        private MapManager _map;

        public void Init(MapManager map)
        {
            _map = map;

            // Alt yöneticileri oluştur
            _tileManager = new RoomTileManager(this, map);
            _doorManager = new RoomDoorManager(this, map);
            _wallManager = new RoomWallManager(this, map, _doorManager);
            _entityManager = new RoomEntityManager(this, map);
        }

        public IEnumerator Generate()
        {
            // Tile'ları oluştur
            yield return _tileManager.Generate();
        }

        public List<RoomDoorManager.DoorInfo> GetDoorInfos()
        {
            return _doorManager.GetDoorInfos(); // RoomDoorManager'daki mevcut metodu kullan
        }

        public RoomDoorManager GetDoorManager()
        {
            return _doorManager; // RoomDoorManager referansı
        }

        // Koridor oluştur
        public Corridor CreateCorridor(Room otherRoom)
        {
            // Zaten bağlıysa, var olan koridoru döndür
            if (RoomCorridor.ContainsKey(otherRoom))
            {
                return RoomCorridor[otherRoom];
            }

            // Yeni koridor oluştur
            Corridor newCorridor = Instantiate(CorridorPrefab);
            newCorridor.name = "Corridor (" + otherRoom.Num + ", " + Num + ")";
            newCorridor.transform.parent = transform.parent;
            newCorridor.Coordinates = new IntVector2(Coordinates.x + Size.x / 2, otherRoom.Coordinates.z + otherRoom.Size.z / 2);
            newCorridor.transform.localPosition = new Vector3(newCorridor.Coordinates.x - _map.MapSettings.MapSize.x / 2, 0, newCorridor.Coordinates.z - _map.MapSettings.MapSize.z / 2);
            newCorridor.Rooms[0] = otherRoom;
            newCorridor.Rooms[1] = this;
            newCorridor.Length = Vector3.Distance(otherRoom.transform.localPosition, transform.localPosition);
            newCorridor.Init(_map);

            // Oda-koridor kayıtları
            otherRoom.RoomCorridor.Add(this, newCorridor);
            RoomCorridor.Add(otherRoom, newCorridor);

            return newCorridor;
        }

        // Kolay erişim için alt yöneticilere yönlendirme metotları

        // Tile yönetimi
        public void RecreateTiles()
        {
            _tileManager.Recreate();
        }

        // Kapı yönetimi
        public IntVector2 CreateDoorPosition(MapDirection direction)
        {
            return _doorManager.CreateDoorPosition(direction);
        }

        public void CreateDoorConnection(MapDirection direction, Room connectedRoom, int doorWidth)
        {
            _doorManager.CreateDoorConnection(direction, connectedRoom, doorWidth);
        }

        public void CreateDoor(MapDirection direction, Room connectedRoom)
        {
            _doorManager.CreateDoor(direction, connectedRoom);
        }

        // Duvar yönetimi
        public IEnumerator CreateWalls()
        {
            yield return _wallManager.CreateWalls();
        }

        public IEnumerator RebuildWallsForDoorMode()
        {
            yield return _wallManager.RebuildWallsForDoorMode();
        }

        // Varlık yönetimi (Oyuncu ve Canavarlar)
        public IEnumerator CreatePlayer()
        {
            yield return _entityManager.CreatePlayer();
        }

        public IEnumerator CreateMonsters()
        {
            yield return _entityManager.CreateMonsters();
        }

        // Oda pozisyon yönetimi
        public void MoveToDoorPosition(IntVector2 doorPos, IntVector2 targetDoorPos, MapDirection direction)
        {
            // Temizle
            _tileManager.Clean();

            IntVector2 moveDelta = new IntVector2(0, 0);

            switch (direction)
            {
                case MapDirection.North:
                    moveDelta.z = targetDoorPos.z - doorPos.z - 1;
                    break;
                case MapDirection.East:
                    moveDelta.x = targetDoorPos.x - doorPos.x - 1;
                    break;
                case MapDirection.South:
                    moveDelta.z = targetDoorPos.z - doorPos.z + 1;
                    break;
                case MapDirection.West:
                    moveDelta.x = targetDoorPos.x - doorPos.x + 1;
                    break;
            }

            // Koordinatları güncelle
            Coordinates.x += moveDelta.x;
            Coordinates.z += moveDelta.z;

            // Transform pozisyonunu güncelle
            Vector3 position = _map.CoordinatesToPosition(Coordinates);
            position.x += Size.x * 0.5f - 0.5f;
            position.z += Size.z * 0.5f - 0.5f;
            position *= RoomMapManager.TileSize;
            transform.localPosition = position;

            // Yeniden tile'ları oluştur
            RecreateTiles();
        }
    }
}