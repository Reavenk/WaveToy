using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PxPre.Datum;

public struct EditValue
{
    public string name;

    public Val val;

    public Val min;
    public Val max;

    public Val incr;

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

    public EditValue(string name, Val val)
    {
        this.name = name;

        this.val = val;

        this.min = null;
        this.max = null;
        this.incr = null;
    }

    public EditValue(string name, Val val, Val min, Val max)
    {
        this.name = name;

        this.val = val;
        this.min = min;
        this.max = max;

        this.incr = null;
    }

    public EditValue(string name, Val val, Val min, Val max, Val incr)
    {
        this.name = name;

        this.val = val;
        this.min = min;
        this.max = max;
        this.incr = incr;
    }

    public Val Clamp(Val vb)
    { 
        if(this.min != null)
            vb = this.min.Max(vb);

        if(this.max != null)
            vb = this.max.Min(vb);

        return vb;
    }

    public Val Offset(Val orig, Val diff)
    {

        if(this.incr != null)
            diff = diff.Mul(this.incr);

        Val ret = orig.Add(diff);

        ret = this.Clamp(ret);
        return ret;
    }

}
