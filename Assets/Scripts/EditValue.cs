// MIT License
// 
// Copyright (c) 2021 Pixel Precision, LLC
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PxPre.Datum;

/// <summary>
/// SceneActor values.
/// </summary>
public struct EditValue
{
    /// <summary>
    /// The name of the value.
    /// </summary>
    public string name;

    /// <summary>
    /// The current value.
    /// </summary>
    public Val val;

    /// <summary>
    /// The minimum allowed value. Set to null if there is no minimum.
    /// </summary>
    public Val min;

    /// <summary>
    /// The maximum allowed value. Set to null if there is no maximum.
    /// </summary>
    public Val max;

    /// <summary>
    /// The increment amount (per pixel drag). Set to null to default to 1.
    /// </summary>
    public Val incr;

    /// <summary>
    /// The value as a bool.
    /// </summary>
    public bool BoolVal
    { 
        get => this.val.GetBool();
        set{ this.val.SetBool(value); }
    }

    /// <summary>
    /// The value as an int.
    /// </summary>
    public int IntVal
    { 
        get => this.val.GetInt();
        set{ this.val.SetInt(value); }
    }

    /// <summary>
    /// The value as a float.
    /// </summary>
    public float FloatVal
    { 
        get => this.val.GetFloat();
        set{ this.val.SetFloat(value); }
    }

    /// <summary>
    /// The value as a string.
    /// </summary>
    public string StringVal
    { 
        get => this.val.GetString();
    }

    /// <summary>
    /// Constructor - only supply a value.
    /// </summary>
    /// <param name="name">The name of the value.</param>
    /// <param name="val">The starting value.</param>
    public EditValue(string name, Val val)
    {
        this.name = name;

        this.val = val;

        this.min = null;
        this.max = null;
        this.incr = null;
    }

    /// <summary>
    /// Constructor with clamp parameters.
    /// </summary>
    /// <param name="name">The name of the value.</param>
    /// <param name="val">The starting value.</param>
    /// <param name="min">The minimum clamp value.</param>
    /// <param name="max">The maximum clamp value.</param>
    public EditValue(string name, Val val, Val min, Val max)
    {
        this.name = name;

        this.val = val;
        this.min = min;
        this.max = max;

        this.incr = null;
    }

    /// <summary>
    /// Constructor, all parameters.
    /// </summary>
    /// <param name="name">The name of the value.</param>
    /// <param name="val">The starting value.</param>
    /// <param name="min">The minimum clamp value.</param>
    /// <param name="max">The maximum clamp value.</param>
    /// <param name="incr">The increment rate.</param>
    public EditValue(string name, Val val, Val min, Val max, Val incr)
    {
        this.name = name;

        this.val = val;
        this.min = min;
        this.max = max;
        this.incr = incr;
    }

    /// <summary>
    /// Clamp a value based on the editing parameters.
    /// </summary>
    /// <param name="vb">The value to clamp.</param>
    /// <returns></returns>
    /// <remarks>This function is not concerned with the the actual value - neither setting it
    /// or referencing it - it instead of concerned with applying the clamp properties.</remarks>
    public Val Clamp(Val vb)
    { 
        if(this.min != null)
            vb = this.min.Max(vb);

        if(this.max != null)
            vb = this.max.Min(vb);

        return vb;
    }

    /// <summary>
    /// Offset a value based on the editing parameters.
    /// </summary>
    /// <param name="orig">The original value.</param>
    /// <param name="diff">The pixel offset.</param>
    /// <returns>The final offset value.</returns>
    /// <remarks>This function is not concerned with the the actual value - neither setting it
    /// or referencing it - it instead of concerned with applying the offset properties.</remarks>
    public Val Offset(Val orig, Val diff)
    {

        if(this.incr != null)
            diff = diff.Mul(this.incr);

        Val ret = orig.Add(diff);

        ret = this.Clamp(ret);
        return ret;
    }

}
