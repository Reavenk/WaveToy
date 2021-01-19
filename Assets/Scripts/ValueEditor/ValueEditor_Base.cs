// MIT License
// 
// Copyright (c) 2021 Pixel Precision, LLC
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The base class for a widget - 
/// These are editors for values that can be interacted with in the properties tree.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class ValueEditor_Base : MonoBehaviour
{
    private Main m;
    public Main Mgr {get=>this.m; }

    private RectTransform _rt;
    public RectTransform rectTransform {get=>this._rt; }

    /// <summary>
    /// The value being edited by the widget.
    /// </summary>
    EditValue ev;

    /// <summary>
    /// Public accessor of ev.
    /// </summary>
    public EditValue EV { get=>this.ev; }

    /// <summary>
    /// Coroutine to check if the Escape key is pressed during a drag operation.
    /// </summary>
    public Coroutine escapeCheck = null;

    /// <summary>
    /// The actor the parameter the widget is editing belongs to.
    /// </summary>
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

    /// <summary>
    /// Virtual function. Called when the widget should be refresh to reflect modified values.
    /// </summary>
    public virtual void OnUpdateValue()
    { 
    }
}
