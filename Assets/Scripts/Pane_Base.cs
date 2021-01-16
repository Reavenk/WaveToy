using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class Pane_Base : MonoBehaviour
{
    Main m;
    public Main mgr {get=>this.m; }

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

    public virtual void Init(Main m)
    { 
        this.m = m;
    }

    public virtual void OnActorAdded(SceneActor actor)
    { }

    public virtual void OnActorDeleted(SceneActor actor)
    {}

    public virtual void OnActorModified(SceneActor actor, string paramName)
    {}

    public virtual void OnActorSelected(SceneActor actor)
    { }

    public virtual void OnCleared()
    { }

    public virtual void OnLoaded()
    { }
}
