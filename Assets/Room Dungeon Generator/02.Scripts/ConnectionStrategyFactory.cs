using UnityEngine;
using System;

namespace ooparts.dungen
{
    /// <summary>
    /// Baðlantý stratejisi oluþturma fabrikasý
    /// </summary>
    public static class ConnectionStrategyFactory
    {
        /// <summary>
        /// Baðlantý türüne göre uygun stratejisi oluþtur
        /// </summary>
        /// <param name="connectionType">Baðlantý türü</param>
        /// <param name="mapGrid">Haritaýzgarasý</param>
        /// <param name="mapSettings">Harita ayarlarý</param>
        /// <param name="monoBehaviour">Coroutine çalýþtýrmak için gerekli MonoBehaviour</param>
        /// <returns>Oluþturulan baðlantý stratejisi</returns>
        public static IConnectionStrategy CreateStrategy(
            ConnectionType connectionType,
            MapGrid mapGrid,
            MapSettings mapSettings,
            MonoBehaviour monoBehaviour)
        {
            // Baðlantý türüne göre uygun stratejiyi seç
            switch (connectionType)
            {
                case ConnectionType.Corridor:
                    return new CorridorConnectionStrategy(mapSettings, monoBehaviour);

                case ConnectionType.DirectDoor:
                    return new DirectDoorConnectionStrategy(mapGrid, mapSettings, monoBehaviour);

                default:
                    // Tanýmlanmamýþ bir baðlantý türü varsa hata fýrlat
                    throw new ArgumentException($"Desteklenmeyen baðlantý türü: {connectionType}");
            }
        }
    }
}