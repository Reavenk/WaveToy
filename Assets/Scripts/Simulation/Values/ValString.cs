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
    public class ValString : Val
    {
        string value = string.Empty;

        public override Type ty { get => Type.String; }

        public ValString(string value)
        { 
            this.value = value;
        }

        public override bool GetBool()
        { 
            return this.value != string.Empty;
        }

        public override int GetInt()
        { 
            if(int.TryParse(this.value, out int n) == true)
                return n;

            return 0;
        }

        public override float GetFloat()
        { 
            if(float.TryParse(this.value, out float f) == true)
                return f;

            return 0.0f;
        }

        public override bool SetBool(bool v)
        { 
            this.value = v.ToString();
            return true;
        }

        public override bool SetInt(int v)
        { 
            this.value = v.ToString();
            return true;
        }

        public override bool SetFloat(float v)
        { 
            this.value = v.ToString();
            return true;
        }

        public override string GetString()
        { 
            return this.value;
        }

        public override bool SetString(string v)
        { 
            this.value = v;
            return true;
        }

        public override Val Clone()
        { 
            return new ValString(this.value);
        }

        public override bool SetValue(Val v)
        { 
            this.value = v.GetString();
            return true;
        }

        public override Val Add(Val v)
        {
            return new ValString(this.value + v.GetString());
        }

        public override Val Mul(Val v)
        { 
            return new ValString(string.Empty);
        }

        public override Val Min(Val v)
        {
            return new ValString(string.Empty);
        }

        public override Val Max(Val v)
        {
            return new ValString(string.Empty);
        }

        public override int ChildrenCount() 
        { 
            return this.value.Length; 
        }

        public override bool IsContainer() 
        { 
            return true; 
        }

        public override Val GetIndex(int i) 
        { 
            if(i < 0 || i >= this.value.Length)
                return ValNone.Inst;

            return new ValInt(this.value[i]); 
        }

        public override Val GetIndex(Val i) 
        {
            return this.GetIndex(i.GetInt());
        }
    }
}
