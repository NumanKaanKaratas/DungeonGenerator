using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ooparts.dungen
{
    /// <summary>
    /// Odalar�n ba�lant�lar�n� y�neten ana s�n�f
    /// </summary>
    public class ConnectionGenerator
    {
        // Harita y�netimi i�in gerekli �zel alanlar
        private MapGrid _mapGrid;
        private MapSettings _mapSettings;
        private List<Room> _rooms;
        private List<Corridor> _corridors;
        private MonoBehaviour _monoBehaviour;

        // Se�ilen ba�lant� stratejisi
        private IConnectionStrategy _connectionStrategy;

        /// <summary>
        /// ConnectionGenerator constructor
        /// </summary>
        /// <param name="mapGrid">Harita �zgaras�</param>
        /// <param name="mapSettings">Harita ayarlar�</param>
        /// <param name="rooms">Odalar listesi</param>
        /// <param name="corridors">Koridorlar listesi</param>
        /// <param name="monoBehaviour">Coroutine �al��t�rmak i�in gerekli MonoBehaviour</param>
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
        /// Odalar� ba�lant� stratejisine g�re ba�la
        /// </summary>
        /// <returns>Ba�lant� olu�turma i�lemini temsil eden coroutine</returns>
        public IEnumerator CreateConnections()
        {
            // Ba�lant� stratejisini fabrika �zerinden olu�tur
            _connectionStrategy = ConnectionStrategyFactory.CreateStrategy(
                _mapSettings.ConnectionMethod,
                _mapGrid,
                _mapSettings,
                _monoBehaviour
            );

            // Se�ilen stratejiye g�re ba�lant�lar� olu�tur
            yield return _monoBehaviour.StartCoroutine(
                _connectionStrategy.CreateConnections(_rooms, _corridors)
            );
        }
    }
}