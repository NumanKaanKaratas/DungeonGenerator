using UnityEngine;
using System.Collections;

namespace ooparts.dungen
{
    public class RoomMapManager : MonoBehaviour
    {
        public MapSettings MapSettings = new MapSettings();

        // Statik tile boyutu
        public static int TileSize { get; private set; } = 1;

        // MapManager referansı
        private MapManager _mapManager;

        private void Start()
        {
            GenerateMap();
        }

        private void Update()
        {
            // Boşluk tuşuna basınca haritayı yeniden oluştur
            if (Input.GetKeyDown(KeyCode.Space))
            {
                RegenerateMap();
            }
        }

        /// <summary>
        /// Yeni harita oluştur
        /// </summary>
        public void GenerateMap()
        {
            // MapManager bileşenini ekle
            _mapManager = gameObject.AddComponent<MapManager>();

            // Ayarları MapManager'a doğrudan aktar
            _mapManager.MapSettings = this.MapSettings;

            // Harita oluşturma coroutine'ini başlat
            StartCoroutine(_mapManager.Generate());
        }

        /// <summary>
        /// Mevcut haritayı sil ve yeniden oluştur
        /// </summary>
        public void RegenerateMap()
        {
            // Mevcut MapManager varsa sil
            if (_mapManager != null)
            {
                Destroy(_mapManager);
            }

            // Yeni harita oluştur
            GenerateMap();
        }
    }
}