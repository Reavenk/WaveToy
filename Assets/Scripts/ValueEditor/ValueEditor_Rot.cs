using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ValueEditor_Rot : ValueEditor_Base
{
    public UnityEngine.UI.InputField input;
    public UnityEngine.UI.Button btnSpinner;
    public UnityEngine.UI.Button btnCCW;
    public UnityEngine.UI.Button btnCW;
    public UnityEngine.UI.Slider slider;

    int recurseGuard = 0;

    public override void Init(Main m, SceneActor actor, EditValue ev)
    {
        base.Init(m, actor, ev);

        this.btnCCW.onClick.AddListener(this.OnButtonCCW);
        this.btnCW.onClick.AddListener(this.OnButtonCW);

        this.slider.onValueChanged.AddListener((x)=>{ this.OnSlider(); });
        this.input.onValueChanged.AddListener((x)=>{this.OnTextChange(); });


        ValueEditor_Spinner.SetupWidgetDrag(
            this.Mgr, 
            this.actor, 
            this.btnSpinner, 
            this.EV);

        this.OnUpdateValue();
    }

    public override void OnUpdateValue()
    {
        if(this.recurseGuard > 0)
            return;

        ++this.recurseGuard;

        this.input.text = this.EV.val.GetString();
        this.slider.value = Mathf.InverseLerp(-180.0f, 180.0f, this.EV.val.GetFloat());

        --this.recurseGuard;
    }

    public void OnInputChanged()
    {
        float f;
        if(float.TryParse(this.input.text, out f) == true)
            this.SetAngle(f);
        else
            this.OnUpdateValue();
    }

    public void OnButtonCCW()
    { 
        this.SetAngle(this.EV.val.GetFloat() - 15.0f);
    }

    public void OnButtonCW()
    {
        this.SetAngle(this.EV.val.GetFloat() + 15.0f);
    }

    public void OnSlider()
    {
        if(this.recurseGuard > 0)
            return;

        this.SetAngle(Mathf.Lerp( -180.0f, 180.0f, this.slider.value));
    }

    public void SetAngle(float f)
    { 
        if(this.recurseGuard > 0)
            return;

        f = ((f + 180.0f) % 360.0f + 360.0f) % 360.0f - 180.0f;
        this.EV.val.SetFloat(f);
        this.Mgr.NotifyActorModified(this.actor, this.EV.name);
        this.OnUpdateValue();
    }

    public void OnTextChange()
    {
        if(this.recurseGuard > 0)
            return;

        if (float.TryParse(this.input.text, out float f) == true)
        {
            if (this.EV.min != null && this.EV.max != null)
                this.EV.val.SetFloat(Mathf.Clamp(f, this.EV.min.GetFloat(), this.EV.max.GetFloat()));
            else if (this.EV.min != null)
                this.EV.val.SetFloat(Mathf.Min(f, this.EV.min.GetFloat()));
            else if (this.EV.max != null)
                this.EV.val.SetFloat(Mathf.Max(f, this.EV.max.GetFloat()));
            else
                this.EV.val.SetFloat(f);
        }
       
        this.OnUpdateValue();
    }
}
