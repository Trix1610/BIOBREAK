using System.Collections;
using UnityEngine;

namespace Core
{
    public class CoroutineRunner : MonoBehaviour
    {
        public static CoroutineRunner Instance { get; private set; }
        
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
        
        public static new Coroutine StartCoroutine(IEnumerator coroutine)
        {
            if (Instance == null)
            {
                Debug.LogError("CoroutineRunner: Instance is null");
                return null;
            }
            
            return ((MonoBehaviour)Instance).StartCoroutine(coroutine);
        }
        
        public static new void StopCoroutine(Coroutine coroutine)
        {
            if (Instance == null)
            {
                Debug.LogError("CoroutineRunner: Instance is null");
                return;
            }
            
            ((MonoBehaviour)Instance).StopCoroutine(coroutine);
        }
    }
}
