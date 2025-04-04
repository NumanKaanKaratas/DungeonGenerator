using UnityEngine;
using System;

namespace ooparts.dungen
{
    /// <summary>
    /// Ba�lant� stratejisi olu�turma fabrikas�
    /// </summary>
    public static class ConnectionStrategyFactory
    {
        /// <summary>
        /// Ba�lant� t�r�ne g�re uygun stratejisi olu�tur
        /// </summary>
        /// <param name="connectionType">Ba�lant� t�r�</param>
        /// <param name="mapGrid">Harita�zgaras�</param>
        /// <param name="mapSettings">Harita ayarlar�</param>
        /// <param name="monoBehaviour">Coroutine �al��t�rmak i�in gerekli MonoBehaviour</param>
        /// <returns>Olu�turulan ba�lant� stratejisi</returns>
        public static IConnectionStrategy CreateStrategy(
            ConnectionType connectionType,
            MapGrid mapGrid,
            MapSettings mapSettings,
            MonoBehaviour monoBehaviour)
        {
            // Ba�lant� t�r�ne g�re uygun stratejiyi se�
            switch (connectionType)
            {
                case ConnectionType.Corridor:
                    return new CorridorConnectionStrategy(mapSettings, monoBehaviour);

                case ConnectionType.DirectDoor:
                    return new DirectDoorConnectionStrategy(mapGrid, mapSettings, monoBehaviour);

                default:
                    // Tan�mlanmam�� bir ba�lant� t�r� varsa hata f�rlat
                    throw new ArgumentException($"Desteklenmeyen ba�lant� t�r�: {connectionType}");
            }
        }
    }
}