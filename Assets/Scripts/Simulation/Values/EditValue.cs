using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct EditValue
{
    public string name;

    public ValueBase val;

    public ValueBase min;
    public ValueBase max;

    public ValueBase incr;

    public bool BoolVal
    { 
        get => this.val.GetBool();
        set{ this.val.SetBool(value); }
    }

    public int IntVal
    { 
        get => this.val.GetInt();
        set{ this.val.SetInt(value); }
    }

    public float FloatVal
    { 
        get => this.val.GetFloat();
        set{ this.val.SetFloat(value); }
    }

    public string StringVal
    { 
        get => this.val.GetString();
    }

    public EditValue(string name, ValueBase val)
    {
        this.name = name;

        this.val = val;

        this.min = null;
        this.max = null;
        this.incr = null;
    }

    public EditValue(string name, ValueBase val, ValueBase min, ValueBase max)
    {
        this.name = name;

        this.val = val;
        this.min = min;
        this.max = max;

        this.incr = null;
    }

    public EditValue(string name, ValueBase val, ValueBase min, ValueBase max, ValueBase incr)
    {
        this.name = name;

        this.val = val;
        this.min = min;
        this.max = max;
        this.incr = incr;
    }

    public ValueBase Clamp(ValueBase vb)
    { 
        if(this.min != null)
            vb = this.min.Max(vb);

        if(this.max != null)
            vb = this.max.Min(vb);

        return vb;
    }

    public ValueBase Offset(ValueBase orig, ValueBase diff)
    {

        if(this.incr != null)
            diff = diff.Mul(this.incr);

        ValueBase ret = orig.Add(diff);

        ret = this.Clamp(ret);
        return ret;
    }

}
