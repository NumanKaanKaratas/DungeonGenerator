using System.Collections.Generic;
using UnityEngine;

namespace ooparts.dungen
{
    // Oda yerleþtirme iþlemlerini yöneten ana sýnýf
    public class RoomPlacer
    {
        private MapGrid _mapGrid;
        private MapSettings _mapSettings;
        private IRoomPlacementStrategy _strategy;

        public RoomPlacer(MapGrid mapGrid, MapSettings mapSettings)
        {
            _mapGrid = mapGrid;
            _mapSettings = mapSettings;
            // Stratejiyi oluþtur
            _strategy = RoomPlacementFactory.CreateStrategy(mapSettings.PlacementType);
        }

        // Odalarý yerleþtir
        public List<Room> PlaceRooms(Transform parent)
        {
            return _strategy.PlaceRooms(parent, _mapGrid, _mapSettings);
        }
    }
}