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
/// Base class for application panes.
/// </summary>
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

    /// <summary>
    /// Notification when a SceneActor is added to the simulation.
    /// </summary>
    /// <param name="actor">The actor added to the sim.</param>
    public virtual void OnActorAdded(SceneActor actor)
    { }

    /// <summary>
    /// Notification when a SceneActor is removed from the simulation.
    /// </summary>
    /// <param name="actor">The actor removed from the sim.</param>
    public virtual void OnActorDeleted(SceneActor actor)
    {}

    /// <summary>
    /// Notification when an actor's parameter is modified.
    /// </summary>
    /// <param name="actor">The actor who's paramter is modified.</param>
    /// <param name="paramName">The name of the modified paramter.</param>
    public virtual void OnActorModified(SceneActor actor, string paramName)
    {}

    /// <summary>
    /// Notification when an actor has been selected.
    /// </summary>
    /// <param name="actor">The selected actor.</param>
    public virtual void OnActorSelected(SceneActor actor)
    { }

    /// <summary>
    /// Notification when the sim has been completly cleared.
    /// </summary>
    public virtual void OnCleared()
    { }

    /// <summary>
    /// Notification when a new sim has been loaded.
    /// </summary>
    public virtual void OnLoaded()
    { }
}
