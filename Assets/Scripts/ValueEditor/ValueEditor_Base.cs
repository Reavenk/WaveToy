using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class ValueEditor_Base : MonoBehaviour
{
    private Main m;
    public Main Mgr {get=>this.m; }

    private RectTransform _rt;
    public RectTransform rectTransform {get=>this._rt; }

    EditValue ev;
    public EditValue EV { get=>this.ev; }

    public Coroutine escapeCheck = null;

    protected SceneActor actor;

    public void Awake()
    {
        this._rt = this.GetComponent<RectTransform>();
    }
    public virtual void Init(Main m, SceneActor actor, EditValue ev)
    { 
        this.m = m;
        this.ev = ev;
        this.actor = actor;
    }

    public virtual void OnUpdateValue()
    { 
    }
}
