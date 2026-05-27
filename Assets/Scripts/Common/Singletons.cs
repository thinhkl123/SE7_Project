
namespace CustomUtils
{
    using UnityEngine;
    
    public class SingletonMono<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T Instance { get; private set; }
        protected virtual void Awake()
        {
            if (Instance == null)
            {
                Instance = this as T;
            }
            else
            {
                Debug.LogError("[SingletonMono<" + typeof(T).Name + ">] Creating new Instance but " + Instance.gameObject.name + " existed, destroying: " + gameObject.name);
                Destroy(gameObject);
            }
        }
    }
    public class Singleton<T> where T : Singleton<T>, new()
    {
        public static T Instance { get; private set; }
        public Singleton()
        {
            if (Instance == null)
            {
                Instance = this as T;
            }
            else
            {
                Debug.LogError("[Singleton] Instance (" + typeof(T).Name + ") existed!");
            }
        }
    }
}