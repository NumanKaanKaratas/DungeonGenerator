using UnityEngine;

namespace ooparts.dungen
{
    public class MapGrid
    {
        private TileType[,] _tileTypes;
        private IntVector2 _mapSize;

        public MapGrid(IntVector2 mapSize)
        {
            _mapSize = mapSize;
            _tileTypes = new TileType[mapSize.x, mapSize.z];
        }

        public void SetTileType(IntVector2 coordinates, TileType tileType)
        {
            if (IsValidCoordinate(coordinates))
            {
                _tileTypes[coordinates.x, coordinates.z] = tileType;
            }
        }

        public TileType GetTileType(IntVector2 coordinates)
        {
            if (IsValidCoordinate(coordinates))
            {
                return _tileTypes[coordinates.x, coordinates.z];
            }
            return TileType.Empty;
        }

        public bool IsValidCoordinate(IntVector2 coordinates)
        {
            return coordinates.x >= 0 && coordinates.x < _mapSize.x &&
                   coordinates.z >= 0 && coordinates.z < _mapSize.z;
        }

        public IntVector2 MapSize => _mapSize;

        public Vector3 CoordinatesToPosition(IntVector2 coordinates)
        {
            return new Vector3(
                coordinates.x - _mapSize.x * 0.5f + 0.5f,
                0f,
                coordinates.z - _mapSize.z * 0.5f + 0.5f);
        }

        public bool IsWall(int x, int z)
        {
            for (int i = x - 1; i <= x + 1; i++)
            {
                if (i < 0 || i >= _mapSize.x)
                {
                    continue;
                }
                for (int j = z - 1; j <= z + 1; j++)
                {
                    if (j < 0 || j >= _mapSize.z || (i == x && j == z))
                    {
                        continue;
                    }
                    if (_tileTypes[i, j] == TileType.Room ||
                        _tileTypes[i, j] == TileType.Corridor ||
                        _tileTypes[i, j] == TileType.Door)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}