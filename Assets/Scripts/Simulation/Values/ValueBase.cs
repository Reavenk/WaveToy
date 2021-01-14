using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ValueBase
{ 
    public enum Type
    { 
        Bool,
        Enum,
        Int,
        Float
    }


    public abstract Type ty {get; }

    public abstract bool GetBool();
    public abstract int GetInt();
    public abstract float GetFloat();

    public abstract void SetBool(bool v);
    public abstract void SetInt(int v);
    public abstract void SetFloat(float v);


    public abstract string GetString();
    public abstract bool SetString(string v);

    public abstract ValueBase Clone();

    public abstract bool SetValue(ValueBase v);
    public abstract ValueBase Add(ValueBase vb);
    public abstract ValueBase Mul(ValueBase vb);

    public abstract ValueBase Min(ValueBase vb);
    public abstract ValueBase Max(ValueBase vb);
}
