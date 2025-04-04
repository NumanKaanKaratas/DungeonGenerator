using System.Collections.Generic;
using UnityEngine;

namespace ooparts.dungen
{
    // Oda yerle�tirme i�lemlerini y�neten ana s�n�f
    public class RoomPlacer
    {
        private MapGrid _mapGrid;
        private MapSettings _mapSettings;
        private IRoomPlacementStrategy _strategy;

        public RoomPlacer(MapGrid mapGrid, MapSettings mapSettings)
        {
            _mapGrid = mapGrid;
            _mapSettings = mapSettings;
            // Stratejiyi olu�tur
            _strategy = RoomPlacementFactory.CreateStrategy(mapSettings.PlacementType);
        }

        // Odalar� yerle�tir
        public List<Room> PlaceRooms(Transform parent)
        {
            return _strategy.PlaceRooms(parent, _mapGrid, _mapSettings);
        }
    }
}