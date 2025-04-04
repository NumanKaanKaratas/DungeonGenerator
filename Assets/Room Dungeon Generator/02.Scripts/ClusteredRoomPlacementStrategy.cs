using System.Collections.Generic;
using UnityEngine;

namespace ooparts.dungen
{
    // Merkez yo�unluklu oda yerle�tirme stratejisi
    public class ClusteredRoomPlacementStrategy : BaseRoomPlacementStrategy
    {
        public override List<Room> PlaceRooms(Transform parent, MapGrid mapGrid, MapSettings mapSettings)
        {
            _rooms.Clear();
            int roomCount = mapSettings.RoomCount;

            // Haritan�n merkezi
            Vector2 mapCenter = new Vector2(mapGrid.MapSize.x / 2f, mapGrid.MapSize.z / 2f);

            // Merkez oday� olu�tur
            IntVector2 centerSize = CreateRandomRoomSize(mapSettings);

            IntVector2 centerCoordinates = new IntVector2(
                Mathf.FloorToInt(mapCenter.x - centerSize.x / 2f),
                Mathf.FloorToInt(mapCenter.y - centerSize.z / 2f)
            );

            

            Room centerRoom = CreateRoomAtPosition(parent, mapGrid, mapSettings, centerSize, centerCoordinates);
            if (centerRoom == null)
            {
                // Merkez oda olu�turulamazsa rastgele yerle�tirmeye ge�
                Debug.LogWarning("Could not place center room. Falling back to random placement.");
                RandomRoomPlacementStrategy fallbackStrategy = new RandomRoomPlacementStrategy();
                return fallbackStrategy.PlaceRooms(parent, mapGrid, mapSettings);
            }

            // Di�er odalar� merkezden d��a do�ru olu�tur
            for (int i = 1; i < roomCount; i++)
            {
                // Merkeze olan mesafeyi belirle (d��a do�ru artan bir de�er)
                float distanceProgress = i / (float)roomCount; // 0 ile 1 aras�nda bir de�er
                float distanceFromCenter = distanceProgress * (mapGrid.MapSize.x / 3f); // Harita boyutunun 1/3'� kadar max uzakl�k

                // Rastgele bir a�� belirle
                float angle = Random.Range(0f, 360f);

                // Polar koordinatlardan Kartezyen koordinatlara d�n��t�r
                float x = mapCenter.x + distanceFromCenter * Mathf.Cos(angle * Mathf.Deg2Rad);
                float z = mapCenter.y + distanceFromCenter * Mathf.Sin(angle * Mathf.Deg2Rad);

                // Oda boyutu belirle
                IntVector2 size = CreateRandomRoomSize(mapSettings);

                // Koordinatlar� hesapla ve s�n�rlar i�inde tut
                IntVector2 coordinates = new IntVector2(
                    Mathf.Clamp(Mathf.FloorToInt(x - size.x / 2f), 1, mapGrid.MapSize.x - size.x - 1),
                    Mathf.Clamp(Mathf.FloorToInt(z - size.z / 2f), 1, mapGrid.MapSize.z - size.z - 1)
                );

                // Maksimum deneme say�s�
                int maxAttempts = 50;
                bool placed = false;

                // Oday� yerle�tirmeyi dene
                for (int attempt = 0; attempt < maxAttempts; attempt++)
                {
                    if (!IsOverlapped(size, coordinates))
                    {
                        CreateRoomAtPosition(parent, mapGrid, mapSettings, size, coordinates);
                        placed = true;
                        break;
                    }

                    // Yerle�tirilemezse yeni pozisyon dene
                    angle = Random.Range(0f, 360f);
                    x = mapCenter.x + distanceFromCenter * Mathf.Cos(angle * Mathf.Deg2Rad);
                    z = mapCenter.y + distanceFromCenter * Mathf.Sin(angle * Mathf.Deg2Rad);

                    coordinates = new IntVector2(
                        Mathf.Clamp(Mathf.FloorToInt(x - size.x / 2f), 1, mapGrid.MapSize.x - size.x - 1),
                        Mathf.Clamp(Mathf.FloorToInt(z - size.z / 2f), 1, mapGrid.MapSize.z - size.z - 1)
                    );
                }

                // E�er oda yerle�tirilemezse daha fazla deneme yapma
                if (!placed)
                {
                    Debug.Log($"Could not place all rooms in clustered mode. Placed {_rooms.Count} rooms.");
                    break;
                }
            }

            return _rooms;
        }
    }
}