using System.Collections.Generic;
using UnityEngine;

namespace ooparts.dungen
{
    // Oda yerleştirme stratejilerinin arayüzü
    public interface IRoomPlacementStrategy
    {
        // Oda yerleştirme işlemini gerçekleştir
        List<Room> PlaceRooms(Transform parent, MapGrid mapGrid, MapSettings mapSettings);
    }
}