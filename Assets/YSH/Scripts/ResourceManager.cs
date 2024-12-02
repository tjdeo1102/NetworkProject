using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : Singleton<ResourceManager>
{
    Dictionary<string, UnityEngine.Object> _resources;

    protected override void Init()
    {
        _resources = new Dictionary<string, UnityEngine.Object>();
    }

    public T Load<T>(in string path) where T : Object
    {
        string key = path.Substring(path.LastIndexOf('/') + 1).Trim();

        _resources.TryGetValue(key, out Object resource);
        if (resource != null)
        {
            Debug.Log($"Load : Found Cache data : {key}");
            return resource as T;
        }

        Debug.Log($"Load : Can't found Cache data : {key}");
        Debug.Log($"Load : Load New Resource : {key}");
        resource = Resources.Load<T>(path.Trim());
        if (resource == null)
        {
            Debug.LogWarning("Resource Load Fail...");
            Debug.LogWarning($"Path : {path}");
            return null;
        }

        _resources.TryAdd(key, resource);

        return resource as T;
    }
}
