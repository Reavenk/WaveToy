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

using UnityEngine;

namespace PxPre.Datum
{
    public class ValInt : Val
    {
        public int i;

        public override Type ty { get => Type.Int; }

        public ValInt(int i )
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

        public override bool SetBool(bool v)
        { 
            this.i = v ? 1 : 0;
            return true;
        }

        public override bool SetInt(int v)
        { 
            this.i = v;
            return true;
        }

        public override bool SetFloat(float v)
        { 
            this.i = (int)v;
            return true;
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

        public override Val Clone()
        {
            return new ValInt(this.i);
        }

        public override bool SetValue(Val v)
        {
            this.i = v.GetInt();
            return true;
        }

        public override Val Add(Val v)
        { 
            return new ValInt(this.i + v.GetInt());
        }

        public override Val Mul(Val v)
        { 
            return new ValInt(this.i * v.GetInt());
        }

        public override Val Min(Val v)
        { 
            return new ValInt(Mathf.Min(this.i, v.GetInt()));
        }

        public override Val Max(Val v)
        { 
            return new ValInt(Mathf.Max(this.i, v.GetInt()));
        }
    }
}