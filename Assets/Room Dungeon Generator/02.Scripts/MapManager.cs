using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace ooparts.dungen
{
    public class MapManager : MonoBehaviour
    {
        // Ana bileþenler
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
        /// Harita oluþturma metodunu baþlat
        /// </summary>
        public IEnumerator Generate()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            // MapGrid oluþtur
            _mapGrid = new MapGrid(MapSettings.MapSize);

            // Odalarý yerleþtir
            _roomPlacer = new RoomPlacer(_mapGrid, MapSettings);
            _rooms = _roomPlacer.PlaceRooms(transform);

            // Odalarý baþlat ve gerekli nesneleri oluþtur
            foreach (Room room in _rooms)
            {
                room.Setting = MapSettings.RoomSettings[Random.Range(0, MapSettings.RoomSettings.Length)];
                room.Init(this);
                StartCoroutine(room.Generate());

                // Oyuncu veya canavar oluþtur
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

            // Odalar arasý baðlantýlarý belirle
            _triangulationManager = new TriangulationManager(_mapGrid, _rooms, this);
            yield return StartCoroutine(_triangulationManager.CreateConnections());
            _corridors = _triangulationManager.GetCorridors();

            UnityEngine.Debug.Log("Every rooms are minimally connected");

            // Baðlantýlarý oluþtur (koridor veya kapý)
            _connectionGenerator = new ConnectionGenerator(_mapGrid, MapSettings, _rooms, _corridors, this);
            yield return StartCoroutine(_connectionGenerator.CreateConnections());

            // Duvarlarý oluþtur
            _wallGenerator = new WallGenerator(_mapGrid, _rooms, _corridors, MapSettings.ConnectionMethod, this);
            yield return StartCoroutine(_wallGenerator.GenerateWalls());

            stopwatch.Stop();
            UnityEngine.Debug.Log("Done in :" + stopwatch.ElapsedMilliseconds / 1000f + "s");
        }

        // Map sýnýfý yapýsýna göre eski uyumluluðu saðla
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