using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour
{
    public static T Instance { get; protected set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = gameObject.GetComponent<T>();
            DontDestroyOnLoad(gameObject);

            Init();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    protected virtual void Init() { }
}