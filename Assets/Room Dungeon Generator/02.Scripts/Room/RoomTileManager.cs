using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ooparts.dungen;

namespace ooparts.dungen
{
    public class RoomTileManager
    {
        private Room _room;
        private MapManager _map;
        private GameObject _tilesObject;
        private Tile[,] _tiles;

        public RoomTileManager(Room room, MapManager map)
        {
            _room = room;
            _map = map;
        }

        public IEnumerator Generate()
        {
            // Tile'lar için parent nesne oluþtur
            _tilesObject = new GameObject("Tiles");
            _tilesObject.transform.parent = _room.transform;
            _tilesObject.transform.localPosition = Vector3.zero;

            // Tüm tile'larý oluþtur
            _tiles = new Tile[_room.Size.x, _room.Size.z];
            for (int x = 0; x < _room.Size.x; x++)
            {
                for (int z = 0; z < _room.Size.z; z++)
                {
                    _tiles[x, z] = CreateTile(new IntVector2((_room.Coordinates.x + x), _room.Coordinates.z + z));
                }
            }
            yield return null;
        }

        public void Clean()
        {
            // Mevcut tile'larý temizle
            for (int x = 0; x < _room.Size.x; x++)
            {
                for (int z = 0; z < _room.Size.z; z++)
                {
                    IntVector2 tileCoord = new IntVector2(_room.Coordinates.x + x, _room.Coordinates.z + z);
                    if (_map.IsValidCoordinate(tileCoord) && _map.GetTileType(tileCoord) == TileType.Room)
                    {
                        _map.SetTileType(tileCoord, TileType.Empty);
                    }
                }
            }

            // Mevcut görsel nesneleri yok et
            if (_tilesObject != null)
            {
                Object.Destroy(_tilesObject.gameObject);
                _tilesObject = null;
            }
        }

        public void Recreate()
        {
            // Temizleme iþlemi
            Clean();

            // Yeni parent oluþtur
            _tilesObject = new GameObject("Tiles");
            _tilesObject.transform.parent = _room.transform;
            _tilesObject.transform.localPosition = Vector3.zero;

            // Tile'larý yeniden oluþtur
            _tiles = new Tile[_room.Size.x, _room.Size.z];
            for (int x = 0; x < _room.Size.x; x++)
            {
                for (int z = 0; z < _room.Size.z; z++)
                {
                    IntVector2 coordinates = new IntVector2(_room.Coordinates.x + x, _room.Coordinates.z + z);
                    if (coordinates.x >= 0 && coordinates.x < _map.MapSettings.MapSize.x &&
                        coordinates.z >= 0 && coordinates.z < _map.MapSettings.MapSize.z)
                    {
                        _tiles[x, z] = CreateTile(coordinates);
                    }
                }
            }
        }

        private Tile CreateTile(IntVector2 coordinates)
        {
            if (_map.GetTileType(coordinates) == TileType.Empty)
            {
                _map.SetTileType(coordinates, TileType.Room);
            }
            else
            {
                Debug.LogError("Tile Conflict at " + coordinates.x + ", " + coordinates.z + "!");
            }

            Tile newTile = Object.Instantiate(_room.TilePrefab);
            newTile.Coordinates = coordinates;
            newTile.name = "Tile " + coordinates.x + ", " + coordinates.z;
            newTile.transform.parent = _tilesObject.transform;
            newTile.transform.localPosition = RoomMapManager.TileSize * new Vector3(
                coordinates.x - _room.Coordinates.x - _room.Size.x * 0.5f + 0.5f,
                0f,
                coordinates.z - _room.Coordinates.z - _room.Size.z * 0.5f + 0.5f);

            // Zemin materyalini ata
            if (newTile.transform.childCount > 0 && _room.Setting != null)
            {
                newTile.transform.GetChild(0).GetComponent<Renderer>().material = _room.Setting.floor;
            }

            return newTile;
        }
    }
}