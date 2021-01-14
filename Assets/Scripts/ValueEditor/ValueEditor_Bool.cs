using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ValueEditor_Bool : ValueEditor_Base
{
    public UnityEngine.UI.Toggle toggle;

    public override void Init(Main m, SceneActor actor, EditValue ev)
    {
        base.Init(m, actor, ev);

        this.toggle.onValueChanged.AddListener(
            (x) =>
            {
                this.EV.val.SetBool(this.toggle.isOn);
                this.OnUpdateValue();
                this.Mgr.NotifyActorModified(this.actor, ev.name);
            });

        this.OnUpdateValue();
    }

    public override void OnUpdateValue()
    {
        this.toggle.isOn = this.EV.val.GetBool();
    }
}
