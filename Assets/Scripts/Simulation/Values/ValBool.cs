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

namespace PxPre.Datum
{
    public class ValBool : Val
    {
        public bool b;

        public override Type ty { get => Type.Bool; }

        public ValBool(bool b)
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

        public override bool SetBool(bool v)
        { 
            this.b = v;
            return true;
        }

        public override bool SetInt(int v)
        { 
            this.b = (v != 0);
            return true;
        }

        public override bool SetFloat(float v)
        { 
            this.b = v != 0.0f;
            return true;
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

        public override Val Clone()
        {
            return new ValBool(this.b);
        }

        public override bool SetValue(Val v)
        {
            this.b = v.GetBool();
            return true;
        }

        public override Val Add(Val vb)
        {
            return new ValBool(this.b || vb.GetBool());
        }

        public override Val Mul(Val vb)
        {
            return new ValBool(this.b && vb.GetBool());
        }

        public override Val Min(Val vb)
        {
            return new ValBool(this.b && vb.GetBool());
        }

        public override Val Max(Val vb)
        {
            return new ValBool(this.b || vb.GetBool());
        }
    }
}