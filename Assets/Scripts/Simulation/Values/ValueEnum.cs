using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ValueEnum : ValueBase
{
    public int i;

    public string [] sels = new string []{ };

    public override Type ty { get => Type.Enum; }

    public ValueEnum(int i, params string [] vals)
    { 
        for(int j = 0; j < vals.Length; ++j)
            this.sels = vals;
    }

    public override bool GetBool()
    { 
        return (this.i != 0) ? true : false;
    }

    public override int GetInt()
    { 
        return this.i;
    }

    public override float GetFloat()
    { 
        return this.i;
    }

    public override void SetBool(bool v)
    { 
        this.i = v ? 1 : 0;
    }

    public override void SetInt(int v)
    { 
        this.i = v;
    }

    public override void SetFloat(float v)
    { 
        this.i = (int)v;
    }


    public override string GetString()
    {
        if(this.i >= 0 && this.i < this.sels.Length)
            return this.sels[this.i];

        return this.i.ToString();
    }

    public override bool SetString(string v)
    { 
        for(int j = 0; j < this.sels.Length; ++j)
        { 
            if(this.sels[j] == v)
            {
                this.i = j;
                return true;
            }
        }

        int ip;
        if(int.TryParse(v, out ip) == false)
            return false;

        this.i = ip;
        return true;
    }

    public override ValueBase Clone()
    {
        return new ValueEnum(this.i, this.sels);
    }

    public override bool SetValue(ValueBase v)
    {
        this.i = v.GetInt();
        return true;
    }

    public override ValueBase Add(ValueBase vb)
    {
        return new ValueInt(this.i + vb.GetInt());
    }

    public override ValueBase Mul(ValueBase vb)
    {
        return new ValueInt(this.i * vb.GetInt());
    }

    public override ValueBase Min(ValueBase vb)
    {
        return new ValueInt(Mathf.Min(this.i, vb.GetInt()));
    }

    public override ValueBase Max(ValueBase vb)
    {
        return new ValueInt(Mathf.Max(this.i, vb.GetInt()));
    }
}
