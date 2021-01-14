using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ValueEditor_Pulldown : ValueEditor_Base
{
    public UnityEngine.UI.Button button;
    public UnityEngine.UI.Text label;

    public override void Init(Main m, SceneActor actor, EditValue ev)
    {
        base.Init(m, actor, ev);

        this.OnUpdateValue();

        ValueEnum ve = ev.val as ValueEnum;

        this.button.onClick.AddListener(
            ()=>
            { 
                PxPre.DropMenu.StackUtil stk = 
                    new PxPre.DropMenu.StackUtil("");

                for(int i = 0; i < ve.sels.Length; ++i)
                { 
                    int iCpy = i;
                    stk.AddAction( 
                        ve.sels[iCpy],  
                        ()=>
                        { 
                            ve.i = iCpy;
                            this.OnUpdateValue();
                            this.Mgr.NotifyActorModified(actor, ve.sels[iCpy]);
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
