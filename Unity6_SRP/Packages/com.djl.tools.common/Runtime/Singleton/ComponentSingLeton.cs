using UnityEngine;

public class ComponentSingLeton<T> : MonoBehaviour where T : Component
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                CreateSingleton();
            }
            //instance.transform.parent = GameRootManager.Instance.SingletonRoot;

            return instance;
        }
    }
    private static void CreateSingleton()
    {
        string name = typeof(T).ToString();
        instance = new GameObject(name).AddComponent<T>();
        if (instance != null)
        {
            DontDestroyOnLoad(instance.gameObject);
        }
    }
}
