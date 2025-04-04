using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ooparts.dungen
{
    /// <summary>
    /// Baðlantý stratejileri için temel arayüz
    /// </summary>
    public interface IConnectionStrategy
    {
        /// <summary>
        /// Odalarý ve koridorlarý belirtilen baðlantý stratejisine göre baðlar
        /// </summary>
        /// <param name="rooms">Baðlanacak odalar listesi</param>
        /// <param name="corridors">Mevcut koridorlar listesi</param>
        /// <returns>Baðlantý oluþturma iþlemini temsil eden coroutine</returns>
        IEnumerator CreateConnections(List<Room> rooms, List<Corridor> corridors);
    }
}