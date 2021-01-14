using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ValueInt : ValueBase
{
    public int i;

    public override Type ty { get => Type.Int; }

    public ValueInt(int i )
    { 
        this.i = i;
    }

    public override bool GetBool()
    { 
        return this.i != 0;
    }

    public override int GetInt()
    { 
        return this.i;
    }

    public override float GetFloat()
    { 
        return (float)this.i;
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
        return this.i.ToString();
    }

    public override bool SetString(string v)
    { 
        int ip;
        if(int.TryParse(v, out ip) == false)
            this.i = ip;

        this.i = ip;
        return true;
    }

    public override ValueBase Clone()
    {
        return new ValueInt(this.i);
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
