using System.Collections.Generic;
using UnityEngine;

namespace ooparts.dungen
{
    // Oda yerleþtirme stratejilerinin arayüzü
    public interface IRoomPlacementStrategy
    {
        // Oda yerleþtirme iþlemini gerçekleþtir
        List<Room> PlaceRooms(Transform parent, MapGrid mapGrid, MapSettings mapSettings);
    }
}