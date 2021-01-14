using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ValueBool : ValueBase
{
    public bool b;

    public override Type ty { get => Type.Bool; }

    public ValueBool(bool b)
    { 
        this.b = b;
    }

    public override bool GetBool()
    { 
        return this.b;
    }

    public override int GetInt()
    { 
        return this.b ? 1 : 0;
    }

    public override float GetFloat()
    { 
        return this.b ? 1.0f : 0.0f;
    }

    public override void SetBool(bool v)
    { 
        this.b = v;
    }

    public override void SetInt(int v)
    { 
        this.b = (v != 0);
    }

    public override void SetFloat(float v)
    { 
        this.b = v != 0.0f;
    }

    public override string GetString()
    { 
        return this.b.ToString();
    }

    public override bool SetString(string v)
    { 
        bool bp;
        if(bool.TryParse(v, out bp) == false)
            return false;

        this.b = bp;
        return true;
    }

    public override ValueBase Clone()
    {
        return new ValueBool(this.b);
    }

    public override bool SetValue(ValueBase v)
    {
        this.b = v.GetBool();
        return true;
    }

    public override ValueBase Add(ValueBase vb)
    {
        return new ValueBool(this.b || vb.GetBool());
    }

    public override ValueBase Mul(ValueBase vb)
    {
        return new ValueBool(this.b && vb.GetBool());
    }

    public override ValueBase Min(ValueBase vb)
    {
        return new ValueBool(this.b && vb.GetBool());
    }

    public override ValueBase Max(ValueBase vb)
    {
        return new ValueBool(this.b || vb.GetBool());
    }
}
