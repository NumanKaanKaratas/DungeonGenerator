using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ooparts.dungen
{
    /// <summary>
    /// Koridor tabanlý oda baðlantý stratejisi
    /// </summary>
    public class CorridorConnectionStrategy : IConnectionStrategy
    {
        private MapSettings _mapSettings;
        private MonoBehaviour _monoBehaviour;

        /// <summary>
        /// Koridor baðlantý stratejisi oluþturucusu
        /// </summary>
        /// <param name="mapSettings">Harita ayarlarý</param>
        /// <param name="monoBehaviour">Coroutine çalýþtýrmak için gerekli MonoBehaviour</param>
        public CorridorConnectionStrategy(MapSettings mapSettings, MonoBehaviour monoBehaviour)
        {
            _mapSettings = mapSettings;
            _monoBehaviour = monoBehaviour;
        }

        /// <summary>
        /// Koridorlarý oluþturarak odalarý baðla
        /// </summary>
        public IEnumerator CreateConnections(List<Room> rooms, List<Corridor> corridors)
        {
            // Koridorlarýn toplam sayýsýný log olarak yazdýr
            Debug.Log($"Processing {corridors.Count} corridors for corridor connections");

            foreach (Corridor corridor in corridors)
            {
                // Koridor geniþliðini Min-Max arasýnda rastgele belirle
                corridor.CorridorWidth = Random.Range(
                    _mapSettings.CorridorWidth.Min,
                    _mapSettings.CorridorWidth.Max + 1
                );

                // Koridoru oluþtur
                yield return _monoBehaviour.StartCoroutine(corridor.Generate());
            }

            Debug.Log("Every corridors are generated");
        }
    }
}