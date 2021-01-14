using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ValueFloat : ValueBase
{
    public float f;

    public override Type ty { get => Type.Float; }

    public ValueFloat(float f)
    { 
        this.f = f;
    }

    public override bool GetBool()
    { 
        return this.f != 0.0f;
    }

    public override int GetInt()
    { 
        return (int)this.f;
    }

    public override float GetFloat()
    { 
        return this.f;
    }

    public override void SetBool(bool v)
    { 
        this.f = v ? 1.0f : 0.0f;
    }

    public override void SetInt(int v)
    { 
        this.f = v;
    }

    public override void SetFloat(float v)
    { 
        this.f = v;
    }

    public override string GetString()
    { 
        return f.ToString();
    }

    public override bool SetString(string v)
    { 
        float fp;
        if(float.TryParse(v, out fp) == false)
            return false;

        this.f = fp;
        return true;
    }

    public override ValueBase Clone()
    {
        return new ValueFloat(this.f);
    }

    public override bool SetValue(ValueBase v)
    {
        this.f = v.GetFloat();
        return true;
    }

    public override ValueBase Add(ValueBase vb)
    {
        return new ValueFloat(this.f + vb.GetFloat());
    }

    public override ValueBase Mul(ValueBase vb)
    {
        return new ValueFloat(this.f * vb.GetFloat());
    }

    public override ValueBase Min(ValueBase vb)
    {
        return new ValueFloat(Mathf.Min(this.f, vb.GetFloat()));
    }

    public override ValueBase Max(ValueBase vb)
    {
        return new ValueFloat(Mathf.Max(this.f, vb.GetFloat()));
    }
}
