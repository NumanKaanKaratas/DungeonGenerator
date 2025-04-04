using UnityEngine;

namespace ooparts.dungen
{
    // Oda yerleþtirme stratejilerini oluþturan fabrika sýnýfý
    public static class RoomPlacementFactory
    {
        // Yerleþtirme tipine göre strateji oluþtur
        public static IRoomPlacementStrategy CreateStrategy(RoomPlacementType placementType)
        {
            switch (placementType)
            {
                case RoomPlacementType.GridBased:
                    return new GridBasedRoomPlacementStrategy();

                case RoomPlacementType.Clustered:
                    return new ClusteredRoomPlacementStrategy();

                case RoomPlacementType.Random:
                default:
                    return new RandomRoomPlacementStrategy();
            }
        }
    }
}