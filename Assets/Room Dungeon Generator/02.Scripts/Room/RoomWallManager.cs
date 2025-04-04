using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ooparts.dungen;

namespace ooparts.dungen
{
    public class RoomWallManager
    {
        private Room _room;
        private MapManager _map;
        private RoomDoorManager _doorManager;

        private GameObject _wallsObject;
        private GameObject _doorsObject;

        public RoomWallManager(Room room, MapManager map, RoomDoorManager doorManager)
        {
            _room = room;
            _map = map;
            _doorManager = doorManager;
        }

        // Duvarlar� ve kap�lar� yeniden olu�tur (kap� modu i�in)
        public IEnumerator RebuildWallsForDoorMode()
        {
            // �nce eski duvar ve kap� nesnelerini temizle
            CleanWalls();

            // Odan�n etraf�n� kontrol et ve duvar/kap� olu�tur
            for (int x = _room.Coordinates.x; x < _room.Coordinates.x + _room.Size.x; x++)
            {
                // Kuzey duvar�
                IntVector2 northPos = new IntVector2(x, _room.Coordinates.z + _room.Size.z);
                if (_map.IsValidCoordinate(northPos) && _map.GetTileType(northPos) == TileType.Empty)
                {
                    _map.SetTileType(northPos, TileType.Wall);
                }

                // G�ney duvar�
                IntVector2 southPos = new IntVector2(x, _room.Coordinates.z - 1);
                if (_map.IsValidCoordinate(southPos) && _map.GetTileType(southPos) == TileType.Empty)
                {
                    _map.SetTileType(southPos, TileType.Wall);
                }
            }

            for (int z = _room.Coordinates.z; z < _room.Coordinates.z + _room.Size.z; z++)
            {
                // Do�u duvar�
                IntVector2 eastPos = new IntVector2(_room.Coordinates.x + _room.Size.x, z);
                if (_map.IsValidCoordinate(eastPos) && _map.GetTileType(eastPos) == TileType.Empty)
                {
                    _map.SetTileType(eastPos, TileType.Wall);
                }

                // Bat� duvar�
                IntVector2 westPos = new IntVector2(_room.Coordinates.x - 1, z);
                if (_map.IsValidCoordinate(westPos) && _map.GetTileType(westPos) == TileType.Empty)
                {
                    _map.SetTileType(westPos, TileType.Wall);
                }
            }

            // Duvarlar�n g�rsel nesnelerini olu�tur
            yield return CreateWalls();
        }

        // Eski duvar ve kap� nesnelerini temizle
        public void CleanWalls()
        {
            if (_wallsObject != null)
            {
                Object.Destroy(_wallsObject);
                _wallsObject = null;
            }

            if (_doorsObject != null)
            {
                Object.Destroy(_doorsObject);
                _doorsObject = null;
            }
        }

        // Duvar ve kap�lar� olu�tur (g�rsel objeler)
        public IEnumerator CreateWalls()
        {
            // Duvarlar i�in bir parent objesi olu�tur
            _wallsObject = new GameObject("Walls");
            _wallsObject.transform.parent = _room.transform;
            _wallsObject.transform.localPosition = Vector3.zero;

            // Kap�lar i�in bir parent objesi olu�tur
            _doorsObject = new GameObject("Doors");
            _doorsObject.transform.parent = _room.transform;
            _doorsObject.transform.localPosition = Vector3.zero;

            // Odan�n etraf�ndaki duvar ve kap� karelerini tara
            IntVector2 leftBottom = new IntVector2(_room.Coordinates.x - 1, _room.Coordinates.z - 1);
            IntVector2 rightTop = new IntVector2(_room.Coordinates.x + _room.Size.x, _room.Coordinates.z + _room.Size.z);

            for (int x = leftBottom.x; x <= rightTop.x; x++)
            {
                for (int z = leftBottom.z; z <= rightTop.z; z++)
                {
                    // Koordinat kontrol�
                    if (x < 0 || x >= _map.MapSettings.MapSize.x || z < 0 || z >= _map.MapSettings.MapSize.z)
                        continue;

                    // �� k�s�m veya k��eleri atla
                    if ((x != leftBottom.x && x != rightTop.x && z != leftBottom.z && z != rightTop.z) ||
                        ((x == leftBottom.x || x == rightTop.x) && (z == leftBottom.z || z == rightTop.z)))
                    {
                        continue;
                    }

                    IntVector2 currentPos = new IntVector2(x, z);
                    TileType tileType = _map.GetTileType(currentPos);

                    // Duvar veya kap� de�ilse atla
                    if (tileType != TileType.Wall && tileType != TileType.Door)
                    {
                        continue;
                    }

                    // Rotasyonu belirle
                    Quaternion rotation = Quaternion.identity;
                    if (x == leftBottom.x)
                    {
                        rotation = MapDirection.West.ToRotation();
                    }
                    else if (x == rightTop.x)
                    {
                        rotation = MapDirection.East.ToRotation();
                    }
                    else if (z == leftBottom.z)
                    {
                        rotation = MapDirection.South.ToRotation();
                    }
                    else if (z == rightTop.z)
                    {
                        rotation = MapDirection.North.ToRotation();
                    }
                    else
                    {
                        Debug.LogError("Wall/Door is not on appropriate location!!");
                        continue;
                    }

                    // Kap� m� duvar m� kontrol et ve ilgili nesneyi olu�tur
                    if (tileType == TileType.Door)
                    {
                        CreateDoorObject(currentPos, rotation);
                    }
                    else
                    {
                        CreateWallObject(currentPos, rotation);
                    }
                }
            }

            yield return null;
        }

        // Kap� objesi olu�tur
        private void CreateDoorObject(IntVector2 position, Quaternion rotation)
        {
            // Kap� prefab� yoksa duvar prefab�n� kullan
            GameObject newDoor = Object.Instantiate(_room.DoorPrefab != null ? _room.DoorPrefab : _room.WallPrefab);
            newDoor.name = "Door (" + position.x + ", " + position.z + ")";
            newDoor.transform.parent = _doorsObject.transform;
            newDoor.transform.localPosition = RoomMapManager.TileSize * new Vector3(
                position.x - _room.Coordinates.x - _room.Size.x * 0.5f + 0.5f,
                0f,
                position.z - _room.Coordinates.z - _room.Size.z * 0.5f + 0.5f);
            newDoor.transform.localRotation = rotation;
            newDoor.transform.localScale *= RoomMapManager.TileSize;

            // Kap� materyali atama
            if (_room.Setting != null && newDoor.transform.childCount > 0)
            {
                if (_room.Setting.door != null)
                {
                    newDoor.transform.GetChild(0).GetComponent<Renderer>().material = _room.Setting.door;
                }
                else if (_room.Setting.wall != null)
                {
                    newDoor.transform.GetChild(0).GetComponent<Renderer>().material = _room.Setting.wall;
                }
            }
        }

        // Duvar objesi olu�tur
        private void CreateWallObject(IntVector2 position, Quaternion rotation)
        {
            GameObject newWall = Object.Instantiate(_room.WallPrefab);
            newWall.name = "Wall (" + position.x + ", " + position.z + ")";
            newWall.transform.parent = _wallsObject.transform;
            newWall.transform.localPosition = RoomMapManager.TileSize * new Vector3(
                position.x - _room.Coordinates.x - _room.Size.x * 0.5f + 0.5f,
                0f,
                position.z - _room.Coordinates.z - _room.Size.z * 0.5f + 0.5f);
            newWall.transform.localRotation = rotation;
            newWall.transform.localScale *= RoomMapManager.TileSize;

            // Duvar materyali atama
            if (_room.Setting != null && _room.Setting.wall != null && newWall.transform.childCount > 0)
            {
                newWall.transform.GetChild(0).GetComponent<Renderer>().material = _room.Setting.wall;
            }
        }
    }
}