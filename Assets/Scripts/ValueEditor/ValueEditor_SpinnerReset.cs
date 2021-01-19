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
/// A spinner widget that has a reset button to reset the value to a hard-coded value.
/// </summary>
/// <remarks>Is not meant for actor parameters, only environment parameters.</remarks>
public class ValueEditor_SpinnerReset : ValueEditor_Spinner
{
    /// <summary>
    /// The reset button.
    /// </summary>
    public UnityEngine.UI.Button resetButton;

    /// <summary>
    /// The value the reset button sets the value to.
    /// </summary>
    public Val resetValue;

    public override void Init(Main m, SceneActor actor, EditValue ev)
    {
        this.resetValue = ev.val.Clone();
        base.Init(m, actor, ev);

        resetButton.onClick.AddListener(this.OnButtonReset);
    }

    /// <summary>
    /// Callback for the reset button.
    /// </summary>
    public void OnButtonReset()
    {
        this.EV.val.SetValue(this.resetValue);
        this.Mgr.NotifyActorModified(this.actor, EV);
    }
}
