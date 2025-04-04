using UnityEngine;
using System;

namespace ooparts.dungen
{

    [System.Serializable]
    public struct MinMax
    {
        public int Min;
        public int Max;
    }

    [System.Serializable]
    public class MapSettings
    {
        // Harita boyutları
        public IntVector2 MapSize;

        // Oda ayarları
        public int RoomCount;
        public MinMax RoomSize;
        public RoomSetting[] RoomSettings;

        // Yerleştirme stratejisi
        public RoomPlacementType PlacementType = RoomPlacementType.Random;

        // Bağlantı ayarları
        public ConnectionType ConnectionMethod = ConnectionType.Corridor;
        public MinMax CorridorWidth;

        // Çoklu oda bağlantısı için yeni ayarlar
        public bool AllowMultiRoomConnections = false;
        public int MaxRoomConnectionCount = 3;

        // Genel ayarlar
        public float GenerationStepDelay;

        // Prefab referansları
        public Room RoomPrefab;
    }
}