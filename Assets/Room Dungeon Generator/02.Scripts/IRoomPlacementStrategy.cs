using System.Collections.Generic;
using UnityEngine;

namespace ooparts.dungen
{
    // Oda yerle�tirme stratejilerinin aray�z�
    public interface IRoomPlacementStrategy
    {
        // Oda yerle�tirme i�lemini ger�ekle�tir
        List<Room> PlaceRooms(Transform parent, MapGrid mapGrid, MapSettings mapSettings);
    }
}