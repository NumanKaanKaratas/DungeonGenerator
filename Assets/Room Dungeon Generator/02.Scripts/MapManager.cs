using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace ooparts.dungen
{
    public class MapManager : MonoBehaviour
    {
        // Ana bile�enler
        private MapGrid _mapGrid;
        private RoomPlacer _roomPlacer;
        private TriangulationManager _triangulationManager;
        private ConnectionGenerator _connectionGenerator;
        private WallGenerator _wallGenerator;

        // Veriler
        public MapSettings MapSettings = new MapSettings();
        private List<Room> _rooms = new List<Room>();
        private List<Corridor> _corridors = new List<Corridor>();

        // Player and monster control
        private bool _hasPlayer = false;

        /// <summary>
        /// Harita olu�turma metodunu ba�lat
        /// </summary>
        public IEnumerator Generate()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            // MapGrid olu�tur
            _mapGrid = new MapGrid(MapSettings.MapSize);

            // Odalar� yerle�tir
            _roomPlacer = new RoomPlacer(_mapGrid, MapSettings);
            _rooms = _roomPlacer.PlaceRooms(transform);

            // Odalar� ba�lat ve gerekli nesneleri olu�tur
            foreach (Room room in _rooms)
            {
                room.Setting = MapSettings.RoomSettings[Random.Range(0, MapSettings.RoomSettings.Length)];
                room.Init(this);
                StartCoroutine(room.Generate());

                // Oyuncu veya canavar olu�tur
                if (_hasPlayer)
                {
                    yield return room.CreateMonsters();
                }
                else
                {
                    yield return room.CreatePlayer();
                    _hasPlayer = true;
                }
                yield return null;
            }

            UnityEngine.Debug.Log("Every rooms are generated");

            // Odalar aras� ba�lant�lar� belirle
            _triangulationManager = new TriangulationManager(_mapGrid, _rooms, this);
            yield return StartCoroutine(_triangulationManager.CreateConnections());
            _corridors = _triangulationManager.GetCorridors();

            UnityEngine.Debug.Log("Every rooms are minimally connected");

            // Ba�lant�lar� olu�tur (koridor veya kap�)
            _connectionGenerator = new ConnectionGenerator(_mapGrid, MapSettings, _rooms, _corridors, this);
            yield return StartCoroutine(_connectionGenerator.CreateConnections());

            // Duvarlar� olu�tur
            _wallGenerator = new WallGenerator(_mapGrid, _rooms, _corridors, MapSettings.ConnectionMethod, this);
            yield return StartCoroutine(_wallGenerator.GenerateWalls());

            stopwatch.Stop();
            UnityEngine.Debug.Log("Done in :" + stopwatch.ElapsedMilliseconds / 1000f + "s");
        }

        // Map s�n�f� yap�s�na g�re eski uyumlulu�u sa�la
        public void SetTileType(IntVector2 coordinates, TileType tileType)
        {
            _mapGrid.SetTileType(coordinates, tileType);
        }

        public TileType GetTileType(IntVector2 coordinates)
        {
            return _mapGrid.GetTileType(coordinates);
        }

        public Vector3 CoordinatesToPosition(IntVector2 coordinates)
        {
            return _mapGrid.CoordinatesToPosition(coordinates);
        }

        public bool IsValidCoordinate(IntVector2 coordinates)
        {
            return _mapGrid.IsValidCoordinate(coordinates);
        }
    }
}