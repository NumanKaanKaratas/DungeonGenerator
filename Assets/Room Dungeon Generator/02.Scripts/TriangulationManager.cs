using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ooparts.dungen
{
    public class TriangulationManager
    {
        private MapGrid _mapGrid;
        private List<Room> _rooms;
        private List<Corridor> _corridors = new List<Corridor>();
        private MonoBehaviour _monoBehaviour;

        public TriangulationManager(MapGrid mapGrid, List<Room> rooms, MonoBehaviour monoBehaviour)
        {
            _mapGrid = mapGrid;
            _rooms = rooms;
            _monoBehaviour = monoBehaviour;
        }

        public IEnumerator CreateConnections()
        {
            // Delaunay üçgenlemesi yap
            yield return _monoBehaviour.StartCoroutine(BowyerWatson());

            // Minimal spanning tree hesapla
            yield return _monoBehaviour.StartCoroutine(PrimMST());

            // Tüm odalarýn baðlý olduðundan emin ol
            EnsureAllRoomsConnected();
        }

        private IEnumerator BowyerWatson()
        {
            List<Triangle> triangulation = new List<Triangle>();

            // Algoritmanýn ihtiyaç duyduðu "loot" üçgenini oluþtur
            Triangle loot = CreateLootTriangle();
            triangulation.Add(loot);

            foreach (Room room in _rooms)
            {
                List<Triangle> badTriangles = new List<Triangle>();

                // Odayý içeren üçgenleri bul
                foreach (Triangle triangle in triangulation)
                {
                    if (triangle.IsContaining(room))
                    {
                        badTriangles.Add(triangle);
                    }
                }

                // Köþe baðlantýlarýný bulalým
                List<Corridor> polygon = new List<Corridor>();
                foreach (Triangle badTriangle in badTriangles)
                {
                    foreach (Corridor corridor in badTriangle.Corridors)
                    {
                        if (corridor.Triangles.Count == 1)
                        {
                            polygon.Add(corridor);
                            corridor.Triangles.Remove(badTriangle);
                            continue;
                        }

                        foreach (Triangle triangle in corridor.Triangles)
                        {
                            if (triangle == badTriangle)
                            {
                                continue;
                            }

                            // Delete Corridor which is between two bad triangles.
                            if (badTriangles.Contains(triangle))
                            {
                                corridor.Rooms[0].RoomCorridor.Remove(corridor.Rooms[1]);
                                corridor.Rooms[1].RoomCorridor.Remove(corridor.Rooms[0]);
                                Object.Destroy(corridor.gameObject);
                            }
                            else
                            {
                                polygon.Add(corridor);
                            }
                            break;
                        }
                    }
                }

                // Bad Triangles'larý kaldýr
                for (int index = badTriangles.Count - 1; index >= 0; --index)
                {
                    Triangle triangle = badTriangles[index];
                    badTriangles.RemoveAt(index);
                    triangulation.Remove(triangle);
                    foreach (Corridor corridor in triangle.Corridors)
                    {
                        corridor.Triangles.Remove(triangle);
                    }
                }

                // Yeni üçgenler oluþtur
                foreach (Corridor corridor in polygon)
                {
                    Triangle newTriangle = new Triangle(corridor.Rooms[0], corridor.Rooms[1], room);
                    triangulation.Add(newTriangle);
                }
            }

            yield return null;

            // Loot üçgeniyle ilgili üçgenleri kaldýr
            for (int index = triangulation.Count - 1; index >= 0; index--)
            {
                if (triangulation[index].Rooms.Contains(loot.Rooms[0]) ||
                    triangulation[index].Rooms.Contains(loot.Rooms[1]) ||
                    triangulation[index].Rooms.Contains(loot.Rooms[2]))
                {
                    triangulation.RemoveAt(index);
                }
            }

            // Loot üçgenini temizle
            foreach (Room room in loot.Rooms)
            {
                List<Corridor> deleteList = new List<Corridor>();
                foreach (KeyValuePair<Room, Corridor> pair in room.RoomCorridor)
                {
                    deleteList.Add(pair.Value);
                }
                for (int index = deleteList.Count - 1; index >= 0; index--)
                {
                    Corridor corridor = deleteList[index];
                    corridor.Rooms[0].RoomCorridor.Remove(corridor.Rooms[1]);
                    corridor.Rooms[1].RoomCorridor.Remove(corridor.Rooms[0]);
                    Object.Destroy(corridor.gameObject);
                }
                Object.Destroy(room.gameObject);
            }
        }

        private Triangle CreateLootTriangle()
        {
            // Haritayý kapsayan büyük bir üçgen oluþtur
            Vector3[] vertices = new Vector3[]
            {
                RoomMapManager.TileSize * new Vector3(_mapGrid.MapSize.x * 2, 0, _mapGrid.MapSize.z),
                RoomMapManager.TileSize * new Vector3(-_mapGrid.MapSize.x * 2, 0, _mapGrid.MapSize.z),
                RoomMapManager.TileSize * new Vector3(0, 0, -2 * _mapGrid.MapSize.z)
            };

            // Üçgenin köþeleri için odalar oluþtur
            Room[] tempRooms = new Room[3];
            for (int i = 0; i < 3; i++)
            {
                tempRooms[i] = Object.Instantiate(_rooms[0]); // Þablon olarak ilk odayý kullan
                tempRooms[i].transform.localPosition = vertices[i];
                tempRooms[i].name = "Loot Room " + i;
                tempRooms[i].Init(_rooms[0].transform.parent.GetComponent<MapManager>());
            }

            return new Triangle(tempRooms[0], tempRooms[1], tempRooms[2]);
        }

        private IEnumerator PrimMST()
        {
            List<Room> connectedRooms = new List<Room>();
            _corridors.Clear();

            // Ýlk odayla baþla
            connectedRooms.Add(_rooms[0]);

            while (connectedRooms.Count < _rooms.Count)
            {
                KeyValuePair<Room, Corridor> minLength = new KeyValuePair<Room, Corridor>();
                List<Corridor> deleteList = new List<Corridor>();

                // En kýsa baðlantýyý bul
                foreach (Room room in connectedRooms)
                {
                    foreach (KeyValuePair<Room, Corridor> pair in room.RoomCorridor)
                    {
                        if (connectedRooms.Contains(pair.Key))
                        {
                            continue;
                        }
                        if (minLength.Value == null || minLength.Value.Length > pair.Value.Length)
                        {
                            minLength = pair;
                        }
                    }
                }

                // Gereksiz koridorlarý kontrol et
                foreach (KeyValuePair<Room, Corridor> pair in minLength.Key.RoomCorridor)
                {
                    if (connectedRooms.Contains(pair.Key) && (minLength.Value != pair.Value))
                    {
                        deleteList.Add(pair.Value);
                    }
                }

                // Gereksiz koridorlarý sil
                for (int index = deleteList.Count - 1; index >= 0; index--)
                {
                    Corridor corridor = deleteList[index];
                    corridor.Rooms[0].RoomCorridor.Remove(corridor.Rooms[1]);
                    corridor.Rooms[1].RoomCorridor.Remove(corridor.Rooms[0]);
                    deleteList.RemoveAt(index);
                    Object.Destroy(corridor.gameObject);
                }

                // Odayý baðlý odalara ekle ve koridoru kaydet
                connectedRooms.Add(minLength.Key);
                _corridors.Add(minLength.Value);
            }

            yield return null;
        }

        private void EnsureAllRoomsConnected()
        {
            // Baðlý olmayan veya tek koridorlu odalarý bul
            List<Room> underConnectedRooms = _rooms.Where(room =>
                _corridors.Count(c => c.Rooms[0] == room || c.Rooms[1] == room) < 2).ToList();

            while (underConnectedRooms.Count > 0)
            {
                Room currentRoom = underConnectedRooms[0];
                Room closestRoom = null;
                float minDistance = float.MaxValue;

                foreach (Room room in _rooms)
                {
                    if (room == currentRoom)
                        continue;

                    // Zaten baðlý mý kontrol et
                    bool isAlreadyConnected = _corridors.Any(c =>
                        (c.Rooms[0] == currentRoom && c.Rooms[1] == room) ||
                        (c.Rooms[0] == room && c.Rooms[1] == currentRoom)
                    );

                    if (isAlreadyConnected)
                        continue;

                    float distance = Vector3.Distance(currentRoom.transform.position, room.transform.position);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestRoom = room;
                    }
                }

                if (closestRoom != null)
                {
                    Corridor newCorridor = currentRoom.CreateCorridor(closestRoom);
                    _corridors.Add(newCorridor);

                    // Tekrar kontrol et
                    underConnectedRooms = _rooms.Where(room =>
                        _corridors.Count(c => c.Rooms[0] == room || c.Rooms[1] == room) < 2).ToList();
                }
                else
                {
                    break;
                }
            }
        }



        public List<Corridor> GetCorridors()
        {
            return _corridors;
        }
    }
}