using UnityEngine;

namespace ooparts.dungen
{
    // Karo tipleri
    public enum TileType
    {
        Empty,
        Room,
        Corridor,
        Wall,
        Door
    }

    // Baðlantý tipleri
    public enum ConnectionType
    {
        Corridor,
        DirectDoor
    }

    // Oda yerleþtirme stratejileri
    public enum RoomPlacementType
    {
        Random,      // Rasgele yerleþtirme
        GridBased,   // Grid tabanlý yerleþtirme
        Clustered    // Merkeze yakýn kümelenmiþ yerleþtirme
    }
}