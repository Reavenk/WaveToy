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

using PxPre.Datum;

/// <summary>
/// Spinner widget for floats and ints.
/// </summary>
public class ValueEditor_Spinner : ValueEditor_Base
{
    /// <summary>
    /// The input view for the value.
    /// </summary>
    public UnityEngine.UI.InputField input;

    /// <summary>
    /// The spinner to drag the number value.
    /// </summary>
    public UnityEngine.UI.Button btnSpinner;

    /// <summary>
    /// The cached value of what a spinner value had when a spinner drag operation was initiated.
    /// </summary>
    static Val startingDragValue = null;

    /// <summary>
    /// The global UI pixel position of the mouse when a spinner drag operation was initiated.
    /// </summary>
    static Vector2 startDrag = Vector2.zero;

    public override void Init(Main m, SceneActor actor, EditValue ev)
    { 
        base.Init(m, actor, ev);

        this.input.onEndEdit.AddListener(
            (x) =>
            {
                this.OnTextChange();
            });

        SetupWidgetDrag(this.Mgr, this.actor, this.btnSpinner, this.EV, this);

        this.OnUpdateValue();
    }

    public override void OnUpdateValue()
    { 
        this.input.text = this.EV.val.GetString();
    }

    /// <summary>
    /// Callback for when the input is changed.
    /// </summary>
    public void OnTextChange()
    {
        Val vb = null;
        if(this.EV.val.ty == Val.Type.Float)
        { 
            if(float.TryParse(this.input.text, out float f) == true)
                vb = new ValFloat(f);
        }
        else
        {
            if (int.TryParse(this.input.text, out int i) == true)
                vb = new ValInt(i);
        }

        if(vb != null)
        {
            this.EV.val.SetValue(this.EV.Clamp(vb));
            this.Mgr.NotifyActorModified(this.actor, this.EV.name);
        }
        this.OnUpdateValue();
    }

    /// <summary>
    /// A function to implement the spinner behaviour.
    /// </summary>
    /// <param name="mgr">The application manager.</param>
    /// <param name="actor">The actor that owns the parameter the widget is being setup for.</param>
    /// <param name="btn">The spinner button.</param>
    /// <param name="ev">The parameter value.</param>
    /// <param name="ve">The parameter's widget.</param>
    public static void SetupWidgetDrag(Main mgr, SceneActor actor, UnityEngine.UI.Button btn, EditValue ev, ValueEditor_Base ve)
    { 
        UnityEngine.EventSystems.EventTrigger et = 
            btn.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();

        et.triggers = new List<UnityEngine.EventSystems.EventTrigger.Entry>();

        UnityEngine.EventSystems.EventTrigger.Entry aBeginDrag = new UnityEngine.EventSystems.EventTrigger.Entry();
        aBeginDrag.eventID = UnityEngine.EventSystems.EventTriggerType.BeginDrag;
        aBeginDrag.callback.AddListener(
            (x)=>
            {
                UnityEngine.EventSystems.PointerEventData ped = x as UnityEngine.EventSystems.PointerEventData;

                if(ve.escapeCheck != null)
                    ve.StopCoroutine(ve.escapeCheck);

                ve.escapeCheck = ve.StartCoroutine(EndDragOnEscape(ped, mgr, actor, ve));

                startingDragValue = ev.val.Clone();
                startDrag = ped.position;
            });

        UnityEngine.EventSystems.EventTrigger.Entry aEndDrag = new UnityEngine.EventSystems.EventTrigger.Entry();
        aEndDrag.eventID = UnityEngine.EventSystems.EventTriggerType.EndDrag;
        aEndDrag.callback.AddListener(
            (x)=>
            {
                UnityEngine.EventSystems.PointerEventData ped = x as UnityEngine.EventSystems.PointerEventData;

                ValFloat diff = new ValFloat(-(startDrag.y - ped.position.y));
                Val vb = ev.Offset(startingDragValue, diff);
                ev.val.SetValue(vb);

                mgr.NotifyActorModified(actor, ev.name);
                if(ve != null && ve.escapeCheck != null)
                { 
                    ve.StopCoroutine(ve.escapeCheck);
                    ve.escapeCheck = null;
                }
            });

        UnityEngine.EventSystems.EventTrigger.Entry aDrag = new UnityEngine.EventSystems.EventTrigger.Entry();
        aDrag.eventID = UnityEngine.EventSystems.EventTriggerType.Drag;
        aDrag.callback.AddListener(
            (x)=>
            {
                UnityEngine.EventSystems.PointerEventData ped = x as UnityEngine.EventSystems.PointerEventData;

                ValFloat diff = new ValFloat(-(startDrag.y - ped.position.y));
                Val vb = ev.Offset(startingDragValue, diff);
                ev.val.SetValue(vb);

                mgr.NotifyActorModified(actor, ev.name);
            });

        et.triggers.Add(aBeginDrag);
        et.triggers.Add(aEndDrag);
        et.triggers.Add(aDrag);
    }

    /// <summary>
    /// Utility function to implement checking if the Escape key is pressed during a drag operation,
    /// and canceling the drag operation and restoring the original value.
    /// </summary>
    /// <param name="draggedPtr">
    /// The PointerEventData involved with the drag. This is required to cancel the drag operation
    /// through Unity's input system.</param>
    /// <param name="mgr">The application manager.</param>
    /// <param name="actor">The actor who's parameter was being modified.</param>
    /// <param name="ve">The widget who's drag action is being cancelled.</param>
    /// <returns></returns>
    public static IEnumerator EndDragOnEscape(UnityEngine.EventSystems.PointerEventData draggedPtr, Main mgr, SceneActor actor, ValueEditor_Base ve)
    {
        while(true)
        { 
            if(Input.GetKeyDown( KeyCode.Escape) == true)
            { 
                draggedPtr.pointerDrag = null;
                draggedPtr.dragging = false;

                ve.EV.val.SetValue(startingDragValue);
                mgr.NotifyActorModified(actor, ve.EV);

                ve.escapeCheck = null;
                yield break;
            }

            yield return null;
        }
    }
}
