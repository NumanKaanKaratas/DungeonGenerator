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

    // Ba�lant� tipleri
    public enum ConnectionType
    {
        Corridor,
        DirectDoor
    }

    // Oda yerle�tirme stratejileri
    public enum RoomPlacementType
    {
        Random,      // Rasgele yerle�tirme
        GridBased,   // Grid tabanl� yerle�tirme
        Clustered    // Merkeze yak�n k�melenmi� yerle�tirme
    }
}