using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PxPre.Datum;

public class ValueEditor_SpinnerReset : ValueEditor_Spinner
{
    public UnityEngine.UI.Button resetButton;
    public Val resetValue;

    public override void Init(Main m, SceneActor actor, EditValue ev)
    {
        this.resetValue = ev.val.Clone();
        base.Init(m, actor, ev);

        resetButton.onClick.AddListener(this.OnButtonReset);
    }

    public void OnButtonReset()
    {
        this.EV.val.SetValue(this.resetValue);
        this.Mgr.NotifyActorModified(this.actor, EV);
    }
}
