using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ooparts.dungen;

namespace ooparts.dungen
{
    public class RoomEntityManager
    {
        private Room _room;
        private MapManager _map;
        private GameObject _monstersObject;
        private GameObject[] _monsters;

        public RoomEntityManager(Room room, MapManager map)
        {
            _room = room;
            _map = map;
        }

        // Canavarlarý oluþtur
        public IEnumerator CreateMonsters()
        {
            // Eski canavarlarý temizle
            CleanMonsters();

            // Yeni canavarlar için parent oluþtur
            _monstersObject = new GameObject("Monsters");
            _monstersObject.transform.parent = _room.transform;
            _monstersObject.transform.localPosition = Vector3.zero;

            // Canavarlarý oluþtur
            _monsters = new GameObject[_room.MonsterCount];

            for (int i = 0; i < _room.MonsterCount; i++)
            {
                GameObject newMonster = Object.Instantiate(_room.MonsterPrefab);
                newMonster.name = "Monster " + (i + 1);
                newMonster.transform.parent = _monstersObject.transform;

                // Canavarlarý odanýn içinde daðýt
                float xPos = Random.Range(0.2f, 0.8f) * _room.Size.x;
                float zPos = Random.Range(0.2f, 0.8f) * _room.Size.z;
                newMonster.transform.localPosition = new Vector3(xPos - _room.Size.x / 2f, 0f, zPos - _room.Size.z / 2f);

                _monsters[i] = newMonster;
            }
            yield return null;
        }

        // Oyuncuyu oluþtur
        public IEnumerator CreatePlayer()
        {
            GameObject player = Object.Instantiate(_room.PlayerPrefab);
            player.name = "Player";
            player.transform.parent = _room.transform.parent;

            // Oyuncuyu odanýn merkezine yerleþtir
            player.transform.localPosition = _room.transform.localPosition;

            yield return null;
        }

        // Canavarlarý temizle
        private void CleanMonsters()
        {
            if (_monstersObject != null)
            {
                Object.Destroy(_monstersObject);
                _monstersObject = null;
                _monsters = null;
            }
        }
    }
}