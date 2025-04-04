using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using ooparts.dungen;

namespace ooparts.dungen
{
    public class Corridor : MonoBehaviour
    {
        private GameObject _tilesObject;
        private GameObject _wallsObject;
        public Tile TilePrefab;
        public GameObject WallPrefab;

        public Room[] Rooms = new Room[2];
        public List<Triangle> Triangles = new List<Triangle>();

        public float Length;
        public IntVector2 Coordinates; // Rooms[1].x , Rooms[0].z

        // Koridor genişliği özelliği
        public int CorridorWidth = 1;

        private MapManager _map;
        private List<Tile> _tiles;

        public void Init(MapManager map)
        {
            _map = map;
        }

        public IEnumerator Generate()
        {
            transform.localPosition *= RoomMapManager.TileSize;
            _tilesObject = new GameObject("Tiles");
            _tilesObject.transform.parent = transform;
            _tilesObject.transform.localPosition = Vector3.zero;

            // Koridor konumlarını düzelt
            MoveStickedCorridor();

            _tiles = new List<Tile>();

            // X yönündeki koridoru oluştur
            CreateHorizontalCorridor();

            // Z yönündeki koridoru oluştur
            CreateVerticalCorridor();

            yield return null;
        }

        // X yönünde koridor oluştur
        private void CreateHorizontalCorridor()
        {
            // X yönündeki başlangıç ve bitiş noktalarını hesapla
            int startX = Rooms[0].Coordinates.x + Rooms[0].Size.x / 2;
            int endX = Coordinates.x;

            if (startX > endX)
            {
                int temp = startX;
                startX = endX;
                endX = temp;
            }

            // Koridor genişliği için orta nokta hesapla
            int centerZ = Coordinates.z;
            int halfWidth = CorridorWidth / 2;

            // Koridoru oluştur
            for (int x = startX; x <= endX; x++)
            {
                for (int offset = -halfWidth; offset <= halfWidth; offset++)
                {
                    int z = centerZ + offset;
                    Tile newTile = CreateTile(new IntVector2(x, z));
                    if (newTile)
                    {
                        _tiles.Add(newTile);
                    }
                }
            }
        }

        // Z yönünde koridor oluştur
        private void CreateVerticalCorridor()
        {
            // Z yönündeki başlangıç ve bitiş noktalarını hesapla
            int startZ = Rooms[1].Coordinates.z + Rooms[1].Size.z / 2;
            int endZ = Coordinates.z;

            if (startZ > endZ)
            {
                int temp = startZ;
                startZ = endZ;
                endZ = temp;
            }

            // Koridor genişliği için orta nokta hesapla
            int centerX = Coordinates.x;
            int halfWidth = CorridorWidth / 2;

            // Koridoru oluştur
            for (int z = startZ; z <= endZ; z++)
            {
                for (int offset = -halfWidth; offset <= halfWidth; offset++)
                {
                    int x = centerX + offset;
                    Tile newTile = CreateTile(new IntVector2(x, z));
                    if (newTile)
                    {
                        _tiles.Add(newTile);
                    }
                }
            }
        }

        private Tile CreateTile(IntVector2 coordinates)
        {
            // Koordinatların sınırlar içinde olup olmadığını kontrol et
            if (coordinates.x < 0 || coordinates.x >= _map.MapSettings.MapSize.x ||
                coordinates.z < 0 || coordinates.z >= _map.MapSettings.MapSize.z)
            {
                return null;
            }

            // Bu pozisyon boş mu kontrol et
            if (_map.GetTileType(coordinates) == TileType.Empty)
            {
                _map.SetTileType(coordinates, TileType.Corridor);
            }
            else
            {
                return null;
            }

            // Tile oluştur
            Tile newTile = Instantiate(TilePrefab);
            newTile.Coordinates = coordinates;
            newTile.name = "Tile " + coordinates.x + ", " + coordinates.z;
            newTile.transform.parent = _tilesObject.transform;

            // Pozisyonu düzelt - bunu oda zemin taşlarıyla aynı şekilde hesapla
            // Tam olarak tile merkezinde olacak şekilde
            newTile.transform.localPosition = RoomMapManager.TileSize * new Vector3(
                coordinates.x - Coordinates.x + 0.5f,
                0,
                coordinates.z - Coordinates.z + 0.5f);

            // Koridor tile'ına da zemin materyali ata
            if (newTile.transform.childCount > 0 && Rooms[0] != null && Rooms[0].Setting != null)
            {
                newTile.transform.GetChild(0).GetComponent<Renderer>().material = Rooms[0].Setting.floor;
            }

            return newTile;
        }

        public void Show()
        {
            Debug.DrawLine(Rooms[0].transform.localPosition, transform.localPosition, Color.white, 3.5f);
            Debug.DrawLine(transform.localPosition, Rooms[1].transform.localPosition, Color.white, 3.5f);
        }

        private void MoveStickedCorridor()
        {
            IntVector2 correction = new IntVector2(0, 0);

            if (Rooms[0].Coordinates.x == Coordinates.x + 1)
            {
                // left 2
                correction.x = 2;
            }
            else if (Rooms[0].Coordinates.x + Rooms[0].Size.x == Coordinates.x)
            {
                // right 2
                correction.x = -2;
            }
            else if (Rooms[0].Coordinates.x == Coordinates.x)
            {
                // left
                correction.x = 1;
            }
            else if (Rooms[0].Coordinates.x + Rooms[0].Size.x == Coordinates.x + 1)
            {
                // right
                correction.x = -1;
            }


            if (Rooms[1].Coordinates.z == Coordinates.z + 1)
            {
                // Bottom 2
                correction.z = 2;
            }
            else if (Rooms[1].Coordinates.z + Rooms[1].Size.z == Coordinates.z)
            {
                // Top 2
                correction.z = -2;
            }
            else if (Rooms[1].Coordinates.z == Coordinates.z)
            {
                // Bottom
                correction.z = 1;
            }
            else if (Rooms[1].Coordinates.z + Rooms[1].Size.z == Coordinates.z + 1)
            {
                // Top
                correction.z = -1;
            }

            Coordinates += correction;
            transform.localPosition += RoomMapManager.TileSize * new Vector3(correction.x, 0f, correction.z);
        }

        public IEnumerator CreateWalls()
        {
            _wallsObject = new GameObject("Walls");
            _wallsObject.transform.parent = transform;
            _wallsObject.transform.localPosition = Vector3.zero;

            // Tüm koridor karelerini kontrol et
            foreach (Tile tile in _tiles)
            {
                foreach (MapDirection direction in MapDirections.Directions)
                {
                    IntVector2 coordinates = tile.Coordinates + direction.ToIntVector2();

                    // Koordinatların sınırlar içinde olup olmadığını kontrol et
                    if (coordinates.x < 0 || coordinates.x >= _map.MapSettings.MapSize.x ||
                        coordinates.z < 0 || coordinates.z >= _map.MapSettings.MapSize.z)
                    {
                        continue;
                    }

                    // Komşu kare boşsa veya duvarsa
                    TileType neighborType = _map.GetTileType(coordinates);
                    if (neighborType == TileType.Empty || neighborType == TileType.Wall)
                    {
                        // Duvar olarak işaretle
                        if (neighborType == TileType.Empty)
                        {
                            _map.SetTileType(coordinates, TileType.Wall);
                        }

                        // Duvar objesi oluştur
                        GameObject newWall = Instantiate(WallPrefab);
                        newWall.name = "Wall (" + coordinates.x + ", " + coordinates.z + ")";
                        newWall.transform.parent = _wallsObject.transform;

                        // Duvar pozisyonunu düzelt - zemin sınırına tam oturacak şekilde
                        Vector3 wallPosition = new Vector3(
                            coordinates.x - Coordinates.x + 0.5f,
                            0f,
                            coordinates.z - Coordinates.z + 0.5f);

                        // Duvar tam olarak koridor zemini ile aynı hizada olmalı
                        newWall.transform.localPosition = RoomMapManager.TileSize * wallPosition;
                        newWall.transform.localRotation = direction.ToRotation();
                        newWall.transform.localScale *= RoomMapManager.TileSize;

                        // Duvar materyalini ata
                        if (Rooms[0] != null && Rooms[0].Setting != null)
                        {
                            newWall.transform.GetChild(0).GetComponent<Renderer>().material = Rooms[0].Setting.wall;
                        }
                    }
                }
            }
            yield return null;
        }
    }
}