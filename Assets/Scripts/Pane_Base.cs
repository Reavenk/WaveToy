using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class Pane_Base : MonoBehaviour
{
    RectTransform rt;
    public RectTransform rectTransform 
    { 
        get
        {
            if(this.rt == null)
                this.rt = this.gameObject.GetComponent<RectTransform>();

            return this.rt;
        }
    }
}
