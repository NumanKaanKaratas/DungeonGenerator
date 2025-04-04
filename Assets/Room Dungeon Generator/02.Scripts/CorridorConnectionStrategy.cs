using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ooparts.dungen
{
    /// <summary>
    /// Koridor tabanl� oda ba�lant� stratejisi
    /// </summary>
    public class CorridorConnectionStrategy : IConnectionStrategy
    {
        private MapSettings _mapSettings;
        private MonoBehaviour _monoBehaviour;

        /// <summary>
        /// Koridor ba�lant� stratejisi olu�turucusu
        /// </summary>
        /// <param name="mapSettings">Harita ayarlar�</param>
        /// <param name="monoBehaviour">Coroutine �al��t�rmak i�in gerekli MonoBehaviour</param>
        public CorridorConnectionStrategy(MapSettings mapSettings, MonoBehaviour monoBehaviour)
        {
            _mapSettings = mapSettings;
            _monoBehaviour = monoBehaviour;
        }

        /// <summary>
        /// Koridorlar� olu�turarak odalar� ba�la
        /// </summary>
        public IEnumerator CreateConnections(List<Room> rooms, List<Corridor> corridors)
        {
            // Koridorlar�n toplam say�s�n� log olarak yazd�r
            Debug.Log($"Processing {corridors.Count} corridors for corridor connections");

            foreach (Corridor corridor in corridors)
            {
                // Koridor geni�li�ini Min-Max aras�nda rastgele belirle
                corridor.CorridorWidth = Random.Range(
                    _mapSettings.CorridorWidth.Min,
                    _mapSettings.CorridorWidth.Max + 1
                );

                // Koridoru olu�tur
                yield return _monoBehaviour.StartCoroutine(corridor.Generate());
            }

            Debug.Log("Every corridors are generated");
        }
    }
}