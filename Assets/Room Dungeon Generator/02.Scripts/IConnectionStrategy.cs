using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ooparts.dungen
{
    /// <summary>
    /// Ba�lant� stratejileri i�in temel aray�z
    /// </summary>
    public interface IConnectionStrategy
    {
        /// <summary>
        /// Odalar� ve koridorlar� belirtilen ba�lant� stratejisine g�re ba�lar
        /// </summary>
        /// <param name="rooms">Ba�lanacak odalar listesi</param>
        /// <param name="corridors">Mevcut koridorlar listesi</param>
        /// <returns>Ba�lant� olu�turma i�lemini temsil eden coroutine</returns>
        IEnumerator CreateConnections(List<Room> rooms, List<Corridor> corridors);
    }
}