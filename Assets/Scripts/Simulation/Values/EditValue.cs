using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct EditValue
{
    public string name;

    public ValueBase val;

    public ValueBase min;
    public ValueBase max;

    public ValueBase decr;
    public ValueBase incr;

    public EditValue(string name, ValueBase val)
    {
        this.name = name;

        this.val = val;

        this.min = null;
        this.max = null;
        this.decr = null;
        this.incr = null;
    }

    public EditValue(string name, ValueBase val, ValueBase min, ValueBase max)
    {
        this.name = name;

        this.val = val;
        this.min = min;
        this.max = max;

        this.decr = null;
        this.incr = null;
    }

    public EditValue(string name, ValueBase val, ValueBase min, ValueBase max, ValueBase decr, ValueBase incr)
    {
        this.name = name;

        this.val = val;
        this.min = min;
        this.max = max;
        this.decr = decr;
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

}
