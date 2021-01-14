using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class CanvasSingleton : MonoBehaviour
{
    static Canvas instance;
    public static Canvas Instance {get => instance; }

    private void Awake()
    {
        instance = this.GetComponent<Canvas>();
    }
}
