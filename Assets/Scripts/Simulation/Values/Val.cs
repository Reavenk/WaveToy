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
    public abstract class Val
    { 
        public enum Type
        { 
            None,
            Bool,
            Enum,
            Int,
            Float,
            String
        }


        public abstract Type ty {get; }

        public abstract bool GetBool();
        public abstract int GetInt();
        public abstract float GetFloat();
        public abstract string GetString();

        public abstract bool SetBool(bool v);
        public abstract bool SetInt(int v);
        public abstract bool SetFloat(float v);
        public abstract bool SetString(string v);

        public abstract Val Clone();

        public abstract bool SetValue(Val v);
        public abstract Val Add(Val v);
        public abstract Val Mul(Val v);

        public abstract Val Min(Val v);
        public abstract Val Max(Val v);

        public virtual int ChildrenCount(){return 0; }
        public virtual bool IsContainer(){return false; }
        public virtual Val GetIndex(int i){ return null; }
        public virtual Val GetIndex(Val i){ return null; }
    }
}