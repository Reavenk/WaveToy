using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PxPre.Datum;

public class ValueEditor_Pulldown : ValueEditor_Base
{
    public UnityEngine.UI.Button button;
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
