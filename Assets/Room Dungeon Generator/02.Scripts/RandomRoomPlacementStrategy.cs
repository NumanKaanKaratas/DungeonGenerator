using System.Collections.Generic;
using UnityEngine;

namespace ooparts.dungen
{
    // Rastgele oda yerleþtirme stratejisi
    public class RandomRoomPlacementStrategy : BaseRoomPlacementStrategy
    {
        public override List<Room> PlaceRooms(Transform parent, MapGrid mapGrid, MapSettings mapSettings)
        {
            _rooms.Clear();
            int attemptedRoomCount = mapSettings.RoomCount;
            int placedRoomCount = 0;

            // Tüm odalarý yerleþtirmeye çalýþ
            for (int i = 0; i < attemptedRoomCount; i++)
            {
                // Rasgele oda boyutu
                IntVector2 size = CreateRandomRoomSize(mapSettings);

                // Rasgele pozisyon
                IntVector2 coordinates = new IntVector2(
                    Random.Range(1, mapGrid.MapSize.x - size.x - 1),
                    Random.Range(1, mapGrid.MapSize.z - size.z - 1)
                );

                // Çakýþma kontrolü ve oda oluþturma
                bool placed = false;
                for (int attempt = 0; attempt < 100; attempt++) // Maksimum 100 deneme
                {
                    if (!IsOverlapped(size, coordinates))
                    {
                        Room newRoom = CreateRoomAtPosition(parent, mapGrid, mapSettings, size, coordinates);
                        if (newRoom != null)
                        {
                            placedRoomCount++;
                            placed = true;
                            break;
                        }
                    }

                    // Yeni bir rasgele konumu dene
                    coordinates = new IntVector2(
                        Random.Range(1, mapGrid.MapSize.x - size.x - 1),
                        Random.Range(1, mapGrid.MapSize.z - size.z - 1)
                    );
                }

                if (!placed)
                {
                    Debug.Log("Cannot place more rooms!");
                    Debug.Log($"Created Rooms: {placedRoomCount}");
                    break;
                }
            }

            return _rooms;
        }
    }
}