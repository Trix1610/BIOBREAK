using UnityEngine;

namespace Core
{
    public class PlayerReference : MonoBehaviour
    {
        public static PlayerReference Instance { get; private set; }
        public GameObject Player { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void SetPlayer(GameObject player)
        {
            Player = player;
        }

        public void ClearPlayer()
        {
            Player = null;
        }
    }
}
