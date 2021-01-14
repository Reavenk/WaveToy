using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ValueEditor_Spinner : ValueEditor_Base
{
    public UnityEngine.UI.InputField input;
    public UnityEngine.UI.Button btnSpinner;

    static ValueBase startingDragValue = null;
    static Vector2 startDrag = Vector2.zero;

    public override void Init(Main m, SceneActor actor, EditValue ev)
    { 
        base.Init(m, actor, ev);

        this.input.onEndEdit.AddListener(
            (x) =>
            {
                this.OnTextChange();
            });

        SetupWidgetDrag(this.Mgr, this.actor, this.btnSpinner, this.EV);

        this.OnUpdateValue();
    }

    public override void OnUpdateValue()
    { 
        this.input.text = this.EV.val.GetString();
    }

    public void OnTextChange()
    { 
        ValueBase vb = null;
        if(this.EV.val.ty == ValueBase.Type.Float)
        { 
            if(float.TryParse(this.input.text, out float f) == true)
                vb = new ValueFloat(f);
        }
        else
        {
            if (int.TryParse(this.input.text, out int i) == true)
                vb = new ValueInt(i);
        }

        this.EV.val.SetValue(this.EV.Clamp(vb));
        this.OnUpdateValue();
    }

    public static void SetupWidgetDrag(Main mgr, SceneActor actor, UnityEngine.UI.Button btn, EditValue ev)
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

                startingDragValue = ev.val.Clone();
                startDrag = ped.position;
            });

        UnityEngine.EventSystems.EventTrigger.Entry aEndDrag = new UnityEngine.EventSystems.EventTrigger.Entry();
        aEndDrag.eventID = UnityEngine.EventSystems.EventTriggerType.EndDrag;
        aEndDrag.callback.AddListener(
            (x)=>
            {
                UnityEngine.EventSystems.PointerEventData ped = x as UnityEngine.EventSystems.PointerEventData;

                ValueFloat diff = new ValueFloat(-(startDrag.y - ped.position.y));
                ValueBase vb = startingDragValue.Add(diff);
                vb = ev.Clamp(vb);
                ev.val.SetValue(vb);

                mgr.NotifyActorModified(actor, ev.name);
            });

        UnityEngine.EventSystems.EventTrigger.Entry aDrag = new UnityEngine.EventSystems.EventTrigger.Entry();
        aDrag.eventID = UnityEngine.EventSystems.EventTriggerType.Drag;
        aDrag.callback.AddListener(
            (x)=>
            {
                UnityEngine.EventSystems.PointerEventData ped = x as UnityEngine.EventSystems.PointerEventData;

                ValueFloat diff = new ValueFloat(-(startDrag.y - ped.position.y));
                ValueBase vb = startingDragValue.Add(diff);
                vb = ev.Clamp(vb);
                ev.val.SetValue(vb);

                mgr.NotifyActorModified(actor, ev.name);
            });

        et.triggers.Add(aBeginDrag);
        et.triggers.Add(aEndDrag);
        et.triggers.Add(aDrag);


    }
}
