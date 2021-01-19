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
/// Spinner widgets for rotation parameters.
/// </summary>
public class ValueEditor_Rot : ValueEditor_Base
{
    /// <summary>
    /// Input field for teh angle.
    /// </summary>
    public UnityEngine.UI.InputField input;

    /// <summary>
    /// Spinner to drag the angle value.
    /// </summary>
    public UnityEngine.UI.Button btnSpinner;

    /// <summary>
    /// Button to rotate the value counter-clockwise (15 degrees)
    /// </summary>
    public UnityEngine.UI.Button btnCCW;

    /// <summary>
    /// Button to rotate the value clockwise (15 degrees)
    /// </summary>
    public UnityEngine.UI.Button btnCW;

    /// <summary>
    /// Slider to modify the rotation between the range of [-180,180]
    /// </summary>
    public UnityEngine.UI.Slider slider;

    /// <summary>
    /// Since many widgets are reflecting the same value and are tied to their own
    /// callbacks, a semaphore is used to prevent callbacks modifying other widgets
    /// to sync them and triggering even more callbacks.
    /// </summary>
    int recurseGuard = 0;

    public override void Init(Main m, SceneActor actor, EditValue ev)
    {
        base.Init(m, actor, ev);

        this.btnCCW.onClick.AddListener(this.OnButtonCCW);
        this.btnCW.onClick.AddListener(this.OnButtonCW);

        this.slider.onValueChanged.AddListener((x)=>{ this.OnSlider(); });
        this.input.onEndEdit.AddListener((x)=>{this.OnTextChange(); });


        ValueEditor_Spinner.SetupWidgetDrag(
            this.Mgr, 
            this.actor, 
            this.btnSpinner, 
            this.EV,
            this);

        this.OnUpdateValue();
    }

    /// <summary>
    /// Update all the UI elements to reflect the current value.
    /// </summary>
    public override void OnUpdateValue()
    {
        if(this.recurseGuard > 0)
            return;

        ++this.recurseGuard;

        this.input.text = this.EV.val.GetString();
        this.slider.value = Mathf.InverseLerp(-180.0f, 180.0f, this.EV.val.GetFloat());

        --this.recurseGuard;
    }

    /// <summary>
    /// The callback for the counter-clockwise button.
    /// </summary>
    public void OnButtonCCW()
    { 
        this.SetAngle(this.EV.val.GetFloat() + 15.0f);
    }

    /// <summary>
    /// The callback for the clockwise button.
    /// </summary>
    public void OnButtonCW()
    {
        this.SetAngle(this.EV.val.GetFloat() - 15.0f);
    }

    /// <summary>
    /// Callback for when the slider is modified.
    /// </summary>
    public void OnSlider()
    {
        if(this.recurseGuard > 0)
            return;

        this.SetAngle(Mathf.Lerp( -180.0f, 180.0f, this.slider.value));
    }

    /// <summary>
    /// Given any angle value, wrap it in the range [-180, 180] and update the widgets.
    /// </summary>
    /// <param name="f">The new rotation value.</param>
    public void SetAngle(float f)
    { 
        if(this.recurseGuard > 0)
            return;

        f = ((f + 180.0f) % 360.0f + 360.0f) % 360.0f - 180.0f;
        this.EV.val.SetFloat(f);
        this.Mgr.NotifyActorModified(this.actor, this.EV.name);
        this.OnUpdateValue();
    }

    /// <summary>
    /// Callback for when the input value changes.
    /// </summary>
    public void OnTextChange()
    {
        if(this.recurseGuard > 0)
            return;

        Val vb = null;

        if (float.TryParse(this.input.text, out float f) == true)
            vb = new ValFloat(f);

        if (vb != null)
        {
            this.EV.val.SetValue(this.EV.Clamp(vb));
            this.Mgr.NotifyActorModified(this.actor, this.EV.name);
        }

        this.OnUpdateValue();
    }
}
