using UnityEngine;

namespace ooparts.dungen
{
    // Oda yerle�tirme stratejilerini olu�turan fabrika s�n�f�
    public static class RoomPlacementFactory
    {
        // Yerle�tirme tipine g�re strateji olu�tur
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