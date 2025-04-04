using System.Collections.Generic;
using UnityEngine;

namespace ooparts.dungen
{
    // Grid tabanl� oda yerle�tirme stratejisi
    public class GridBasedRoomPlacementStrategy : BaseRoomPlacementStrategy
    {
        public override List<Room> PlaceRooms(Transform parent, MapGrid mapGrid, MapSettings mapSettings)
        {
            _rooms.Clear();
            int roomCount = mapSettings.RoomCount;

            // Grid boyutlar�n� hesapla
            int gridSizeX, gridSizeZ;
            CalculateGridSize(roomCount, mapGrid.MapSize, out gridSizeX, out gridSizeZ);

            Debug.Log($"Grid Dimensions: {gridSizeX}x{gridSizeZ} for {roomCount} rooms");

            // Grid h�cre boyutlar�
            float cellWidth = mapGrid.MapSize.x / (float)gridSizeX;
            float cellHeight = mapGrid.MapSize.z / (float)gridSizeZ;

            Debug.Log($"Cell Dimensions: {cellWidth}x{cellHeight}");

            int roomsPlaced = 0;

            // Her grid h�cresine bir oda yerle�tir
            for (int z = 0; z < gridSizeZ && roomsPlaced < roomCount; z++)
            {
                for (int x = 0; x < gridSizeX && roomsPlaced < roomCount; x++)
                {
                    // *** De�i�iklik 1: Her oda i�in min-max aral���ndan rasgele bir boyut se� ***
                    // Kullan�c�n�n tan�mlad��� min-max aral���na g�re rasgele bir oda boyutu olu�tur
                    IntVector2 randomSize = new IntVector2(
                        Random.Range(mapSettings.RoomSize.Min, mapSettings.RoomSize.Max + 1),
                        Random.Range(mapSettings.RoomSize.Min, mapSettings.RoomSize.Max + 1)
                    );

                    // *** De�i�iklik 2: Oda boyutu grid h�cresine s��mal� ***
                    // Odan�n maksimum boyutu, h�cre boyutunun %80'i olsun
                    int maxWidthForCell = Mathf.FloorToInt(cellWidth * 0.8f);
                    int maxHeightForCell = Mathf.FloorToInt(cellHeight * 0.8f);

                    // Oda boyutu hem kullan�c� taraf�ndan belirlenen aral�kta hem de h�cre boyutuna g�re s�n�rl� olsun
                    int roomWidth = Mathf.Min(randomSize.x, maxWidthForCell);
                    int roomHeight = Mathf.Min(randomSize.z, maxHeightForCell);

                    // Boyutun minimum de�erden k���k olmamas�n� sa�la
                    roomWidth = Mathf.Max(roomWidth, mapSettings.RoomSize.Min);
                    roomHeight = Mathf.Max(roomHeight, mapSettings.RoomSize.Min);

                    // Oda boyutlar�
                    IntVector2 size = new IntVector2(roomWidth, roomHeight);

                    // *** De�i�iklik 3: Grid i�inde biraz rasgelelik ekle ama grid yap�s�n� koru ***
                    // H�cre i�inde oday� rasgele bir konuma yerle�tir, ama yine de grid yap�s�n� koru
                    float randomOffsetX = Random.Range(-cellWidth * 0.15f, cellWidth * 0.15f);
                    float randomOffsetZ = Random.Range(-cellHeight * 0.15f, cellHeight * 0.15f);

                    // H�cre i�inde odan�n merkezi
                    float cellCenterX = (x + 0.5f) * cellWidth + randomOffsetX;
                    float cellCenterZ = (z + 0.5f) * cellHeight + randomOffsetZ;

                    // Oda koordinatlar� (h�crenin merkezine yerle�tir, oda boyutunun yar�s� kadar offset ile)
                    IntVector2 coordinates = new IntVector2(
                        Mathf.FloorToInt(cellCenterX - size.x / 2f),
                        Mathf.FloorToInt(cellCenterZ - size.z / 2f)
                    );

                    // S�n�rlar� kontrol et ve d�zelt
                    coordinates.x = Mathf.Clamp(coordinates.x, 1, mapGrid.MapSize.x - size.x - 1);
                    coordinates.z = Mathf.Clamp(coordinates.z, 1, mapGrid.MapSize.z - size.z - 1);

                    // Oday� olu�tur
                    bool placed = false;
                    for (int attempt = 0; attempt < 10; attempt++) // Birka� deneme yap
                    {
                        if (!IsOverlapped(size, coordinates))
                        {
                            Room newRoom = CreateRoomAtPosition(parent, mapGrid, mapSettings, size, coordinates);
                            if (newRoom != null)
                            {
                                roomsPlaced++;
                                placed = true;
                                Debug.Log($"Placed room in grid cell ({x},{z}) at coordinates ({coordinates.x},{coordinates.z}) with size {size.x}x{size.z}");
                                break;
                            }
                        }

                        // �ak��ma varsa, h�cre i�inde biraz rastgele kayd�r ama grid yap�s�n� koru
                        randomOffsetX = Random.Range(-cellWidth * 0.15f, cellWidth * 0.15f);
                        randomOffsetZ = Random.Range(-cellHeight * 0.15f, cellHeight * 0.15f);
                        cellCenterX = (x + 0.5f) * cellWidth + randomOffsetX;
                        cellCenterZ = (z + 0.5f) * cellHeight + randomOffsetZ;

                        coordinates = new IntVector2(
                            Mathf.Clamp(Mathf.FloorToInt(cellCenterX - size.x / 2f), 1, mapGrid.MapSize.x - size.x - 1),
                            Mathf.Clamp(Mathf.FloorToInt(cellCenterZ - size.z / 2f), 1, mapGrid.MapSize.z - size.z - 1)
                        );
                    }

                    if (!placed)
                    {
                        Debug.Log($"Could not place room in grid cell ({x},{z})");
                    }
                }
            }

            Debug.Log($"Grid-based placement created {roomsPlaced} rooms out of {roomCount}");

            // E�er hedeflenen oda say�s�na ula��lamad�ysa, kalan odalar� rastgele yerle�tirmeyi dene
            if (roomsPlaced < roomCount)
            {
                Debug.Log($"Trying to place remaining {roomCount - roomsPlaced} rooms randomly");
                AttemptRandomPlacement(parent, mapGrid, mapSettings, roomCount - roomsPlaced);
            }

            return _rooms;
        }

        // Rastgele oda yerle�tirmeyi dene (GridBased'in yedek fonksiyonu)
        private void AttemptRandomPlacement(Transform parent, MapGrid mapGrid, MapSettings mapSettings, int remainingRooms)
        {
            for (int i = 0; i < remainingRooms * 5; i++) // Ekstra deneme �ans�
            {
                // Rasgele oda boyutu
                IntVector2 size = CreateRandomRoomSize(mapSettings);

                // Rasgele koordinat
                IntVector2 coordinates = new IntVector2(
                    Random.Range(1, mapGrid.MapSize.x - size.x - 1),
                    Random.Range(1, mapGrid.MapSize.z - size.z - 1)
                );

                if (!IsOverlapped(size, coordinates))
                {
                    Room newRoom = CreateRoomAtPosition(parent, mapGrid, mapSettings, size, coordinates);
                    if (newRoom != null)
                    {
                        remainingRooms--;
                        if (remainingRooms <= 0)
                            break;
                    }
                }
            }

            Debug.Log($"Final room count: {_rooms.Count}");
        }

        // Grid boyutlar�n� hesapla
        private void CalculateGridSize(int roomCount, IntVector2 mapSize, out int gridX, out int gridZ)
        {
            // Harita oran�n� hesapla
            float mapRatio = mapSize.x / (float)mapSize.z;

            // Grid boyutlar�n� hesapla
            gridZ = Mathf.FloorToInt(Mathf.Sqrt(roomCount / mapRatio));
            if (gridZ <= 0) gridZ = 1;

            gridX = Mathf.CeilToInt(roomCount / (float)gridZ);
            if (gridX <= 0) gridX = 1;

            // Toplam h�cre say�s�n� kontrol et
            while (gridX * gridZ < roomCount)
            {
                gridZ++;
            }
        }
    }
}