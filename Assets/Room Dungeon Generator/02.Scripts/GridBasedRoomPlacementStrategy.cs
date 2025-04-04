using System.Collections.Generic;
using UnityEngine;

namespace ooparts.dungen
{
    // Grid tabanlý oda yerleþtirme stratejisi
    public class GridBasedRoomPlacementStrategy : BaseRoomPlacementStrategy
    {
        public override List<Room> PlaceRooms(Transform parent, MapGrid mapGrid, MapSettings mapSettings)
        {
            _rooms.Clear();
            int roomCount = mapSettings.RoomCount;

            // Grid boyutlarýný hesapla
            int gridSizeX, gridSizeZ;
            CalculateGridSize(roomCount, mapGrid.MapSize, out gridSizeX, out gridSizeZ);

            Debug.Log($"Grid Dimensions: {gridSizeX}x{gridSizeZ} for {roomCount} rooms");

            // Grid hücre boyutlarý
            float cellWidth = mapGrid.MapSize.x / (float)gridSizeX;
            float cellHeight = mapGrid.MapSize.z / (float)gridSizeZ;

            Debug.Log($"Cell Dimensions: {cellWidth}x{cellHeight}");

            int roomsPlaced = 0;

            // Her grid hücresine bir oda yerleþtir
            for (int z = 0; z < gridSizeZ && roomsPlaced < roomCount; z++)
            {
                for (int x = 0; x < gridSizeX && roomsPlaced < roomCount; x++)
                {
                    // *** Deðiþiklik 1: Her oda için min-max aralýðýndan rasgele bir boyut seç ***
                    // Kullanýcýnýn tanýmladýðý min-max aralýðýna göre rasgele bir oda boyutu oluþtur
                    IntVector2 randomSize = new IntVector2(
                        Random.Range(mapSettings.RoomSize.Min, mapSettings.RoomSize.Max + 1),
                        Random.Range(mapSettings.RoomSize.Min, mapSettings.RoomSize.Max + 1)
                    );

                    // *** Deðiþiklik 2: Oda boyutu grid hücresine sýðmalý ***
                    // Odanýn maksimum boyutu, hücre boyutunun %80'i olsun
                    int maxWidthForCell = Mathf.FloorToInt(cellWidth * 0.8f);
                    int maxHeightForCell = Mathf.FloorToInt(cellHeight * 0.8f);

                    // Oda boyutu hem kullanýcý tarafýndan belirlenen aralýkta hem de hücre boyutuna göre sýnýrlý olsun
                    int roomWidth = Mathf.Min(randomSize.x, maxWidthForCell);
                    int roomHeight = Mathf.Min(randomSize.z, maxHeightForCell);

                    // Boyutun minimum deðerden küçük olmamasýný saðla
                    roomWidth = Mathf.Max(roomWidth, mapSettings.RoomSize.Min);
                    roomHeight = Mathf.Max(roomHeight, mapSettings.RoomSize.Min);

                    // Oda boyutlarý
                    IntVector2 size = new IntVector2(roomWidth, roomHeight);

                    // *** Deðiþiklik 3: Grid içinde biraz rasgelelik ekle ama grid yapýsýný koru ***
                    // Hücre içinde odayý rasgele bir konuma yerleþtir, ama yine de grid yapýsýný koru
                    float randomOffsetX = Random.Range(-cellWidth * 0.15f, cellWidth * 0.15f);
                    float randomOffsetZ = Random.Range(-cellHeight * 0.15f, cellHeight * 0.15f);

                    // Hücre içinde odanýn merkezi
                    float cellCenterX = (x + 0.5f) * cellWidth + randomOffsetX;
                    float cellCenterZ = (z + 0.5f) * cellHeight + randomOffsetZ;

                    // Oda koordinatlarý (hücrenin merkezine yerleþtir, oda boyutunun yarýsý kadar offset ile)
                    IntVector2 coordinates = new IntVector2(
                        Mathf.FloorToInt(cellCenterX - size.x / 2f),
                        Mathf.FloorToInt(cellCenterZ - size.z / 2f)
                    );

                    // Sýnýrlarý kontrol et ve düzelt
                    coordinates.x = Mathf.Clamp(coordinates.x, 1, mapGrid.MapSize.x - size.x - 1);
                    coordinates.z = Mathf.Clamp(coordinates.z, 1, mapGrid.MapSize.z - size.z - 1);

                    // Odayý oluþtur
                    bool placed = false;
                    for (int attempt = 0; attempt < 10; attempt++) // Birkaç deneme yap
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

                        // Çakýþma varsa, hücre içinde biraz rastgele kaydýr ama grid yapýsýný koru
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

            // Eðer hedeflenen oda sayýsýna ulaþýlamadýysa, kalan odalarý rastgele yerleþtirmeyi dene
            if (roomsPlaced < roomCount)
            {
                Debug.Log($"Trying to place remaining {roomCount - roomsPlaced} rooms randomly");
                AttemptRandomPlacement(parent, mapGrid, mapSettings, roomCount - roomsPlaced);
            }

            return _rooms;
        }

        // Rastgele oda yerleþtirmeyi dene (GridBased'in yedek fonksiyonu)
        private void AttemptRandomPlacement(Transform parent, MapGrid mapGrid, MapSettings mapSettings, int remainingRooms)
        {
            for (int i = 0; i < remainingRooms * 5; i++) // Ekstra deneme þansý
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

        // Grid boyutlarýný hesapla
        private void CalculateGridSize(int roomCount, IntVector2 mapSize, out int gridX, out int gridZ)
        {
            // Harita oranýný hesapla
            float mapRatio = mapSize.x / (float)mapSize.z;

            // Grid boyutlarýný hesapla
            gridZ = Mathf.FloorToInt(Mathf.Sqrt(roomCount / mapRatio));
            if (gridZ <= 0) gridZ = 1;

            gridX = Mathf.CeilToInt(roomCount / (float)gridZ);
            if (gridX <= 0) gridX = 1;

            // Toplam hücre sayýsýný kontrol et
            while (gridX * gridZ < roomCount)
            {
                gridZ++;
            }
        }
    }
}