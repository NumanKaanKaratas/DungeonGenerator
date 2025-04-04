using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ooparts.dungen
{
    public class WallGenerator
    {
        private MapGrid _mapGrid;
        private List<Room> _rooms;
        private List<Corridor> _corridors;
        private ConnectionType _connectionType;
        private MonoBehaviour _monoBehaviour;

        public WallGenerator(MapGrid mapGrid, List<Room> rooms, List<Corridor> corridors,
                           ConnectionType connectionType, MonoBehaviour monoBehaviour)
        {
            _mapGrid = mapGrid;
            _rooms = rooms;
            _corridors = corridors;
            _connectionType = connectionType;
            _monoBehaviour = monoBehaviour;
        }

        public IEnumerator GenerateWalls()
        {
            // Önce haritadaki boþ alanlarý kontrol edip duvar olmasý gerekenleri belirle
            yield return _monoBehaviour.StartCoroutine(WallCheck());

            // Oda duvarlarýný oluþtur
            foreach (Room room in _rooms)
            {
                yield return _monoBehaviour.StartCoroutine(room.CreateWalls());
            }

            // Koridorlu modda koridor duvarlarýný oluþtur
            if (_connectionType == ConnectionType.Corridor)
            {
                foreach (Corridor corridor in _corridors)
                {
                    yield return _monoBehaviour.StartCoroutine(corridor.CreateWalls());
                }
            }

            Debug.Log("Every walls are generated");
        }

        private IEnumerator WallCheck()
        {
            IntVector2 mapSize = _mapGrid.MapSize;

            for (int x = 0; x < mapSize.x; x++)
            {
                for (int z = 0; z < mapSize.z; z++)
                {
                    if (_mapGrid.GetTileType(new IntVector2(x, z)) == TileType.Empty &&
                        _mapGrid.IsWall(x, z))
                    {
                        _mapGrid.SetTileType(new IntVector2(x, z), TileType.Wall);
                    }
                }
            }
            yield return null;
        }
    }
}