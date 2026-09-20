using System.Collections.Generic;
using UnityEngine;

public class ProjectilePool
{
    private readonly GameObject prefab;
    private readonly Queue<GameObject> pool = new Queue<GameObject>();

    public ProjectilePool(GameObject prefab, int initialSize = 10)
    {
        this.prefab = prefab;
        
        for (int i = 0; i < initialSize; i++)
        {
            GameObject obj = Object.Instantiate(prefab);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public GameObject Get()
    {
        GameObject obj;
        while (pool.Count > 0)
        {
            obj = pool.Dequeue();
            // Пропускаем уничтоженные объекты
            if (obj != null)
            {
                obj.SetActive(true);
                return obj;
            }
        }
        // Если в пуле нет валидных объектов, создаем новый
        obj = Object.Instantiate(prefab);
        obj.SetActive(true);
        return obj;
    }

    public void ReturnToPool(GameObject obj)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}