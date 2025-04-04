using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ooparts.dungen
{
    /// <summary>
    /// Odalarýn baðlantýlarýný yöneten ana sýnýf
    /// </summary>
    public class ConnectionGenerator
    {
        // Harita yönetimi için gerekli özel alanlar
        private MapGrid _mapGrid;
        private MapSettings _mapSettings;
        private List<Room> _rooms;
        private List<Corridor> _corridors;
        private MonoBehaviour _monoBehaviour;

        // Seçilen baðlantý stratejisi
        private IConnectionStrategy _connectionStrategy;

        /// <summary>
        /// ConnectionGenerator constructor
        /// </summary>
        /// <param name="mapGrid">Harita ýzgarasý</param>
        /// <param name="mapSettings">Harita ayarlarý</param>
        /// <param name="rooms">Odalar listesi</param>
        /// <param name="corridors">Koridorlar listesi</param>
        /// <param name="monoBehaviour">Coroutine çalýþtýrmak için gerekli MonoBehaviour</param>
        public ConnectionGenerator(
            MapGrid mapGrid,
            MapSettings mapSettings,
            List<Room> rooms,
            List<Corridor> corridors,
            MonoBehaviour monoBehaviour)
        {
            _mapGrid = mapGrid;
            _mapSettings = mapSettings;
            _rooms = rooms;
            _corridors = corridors;
            _monoBehaviour = monoBehaviour;
        }

        /// <summary>
        /// Odalarý baðlantý stratejisine göre baðla
        /// </summary>
        /// <returns>Baðlantý oluþturma iþlemini temsil eden coroutine</returns>
        public IEnumerator CreateConnections()
        {
            // Baðlantý stratejisini fabrika üzerinden oluþtur
            _connectionStrategy = ConnectionStrategyFactory.CreateStrategy(
                _mapSettings.ConnectionMethod,
                _mapGrid,
                _mapSettings,
                _monoBehaviour
            );

            // Seçilen stratejiye göre baðlantýlarý oluþtur
            yield return _monoBehaviour.StartCoroutine(
                _connectionStrategy.CreateConnections(_rooms, _corridors)
            );
        }
    }
}