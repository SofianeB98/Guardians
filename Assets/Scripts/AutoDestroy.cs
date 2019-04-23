using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    float remaining = 0;

    [SerializeField]
    float time = 0.1f;

    void Awake()
    {
        remaining = time;
    }

    void Update()
    {
        if ((remaining -= Time.deltaTime) <= 0)
        {
            Destroy(gameObject);
        }
    }
    
}
