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
/// The pulldown widget for enums.
/// </summary>
public class ValueEditor_Pulldown : ValueEditor_Base
{
    /// <summary>
    /// The spinner button.
    /// </summary>
    public UnityEngine.UI.Button button;

    /// <summary>
    /// The display of the current value.
    /// </summary>
    public UnityEngine.UI.Text label;

    public override void Init(Main m, SceneActor actor, EditValue ev)
    {
        base.Init(m, actor, ev);

        this.OnUpdateValue();

        ValEnum ve = ev.val as ValEnum;

        this.button.onClick.AddListener(
            ()=>
            { 
                PxPre.DropMenu.StackUtil stk = 
                    new PxPre.DropMenu.StackUtil("");

                foreach(string sel in ve.selections.GetNames())
                { 
                    string selCpy = sel;
                    stk.AddAction( 
                        selCpy,  
                        ()=>
                        {
                            int ? n = ve.selections.GetInt(selCpy);
                            if(n.HasValue == true)
                            {
                                ve.i = n.Value;
                                this.OnUpdateValue();
                                this.Mgr.NotifyActorModified(actor, this.EV.name);
                            }
                        });
                }

                PxPre.DropMenu.DropMenuSingleton.MenuInst.CreateDropdownMenu(
                    CanvasSingleton.Instance,
                    stk.Root,
                    button.GetComponent<RectTransform>());
            });

    }

    public override void OnUpdateValue()
    {
        this.label.text = this.EV.val.GetString();
    }
}
