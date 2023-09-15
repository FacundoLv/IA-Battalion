using UnityEngine;

namespace Core
{
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance = null;

        public static bool IsAwake => _instance != null;

        public static T Instance
        {
            get
            {
                if (_instance != null) return _instance;
            
                _instance = (T) FindObjectOfType(typeof(T));
                if (_instance != null) return _instance;
            
                var goName = typeof(T).ToString();
                var go = GameObject.Find(goName);

                if (go == null) go = new GameObject(goName);

                _instance = go.AddComponent<T>();
                return _instance;
            }
        }

        public virtual void OnApplicationQuit()
        {
            _instance = null;
        }
    }
}