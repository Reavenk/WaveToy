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

using System.Collections.Generic;
using UnityEngine;

namespace PxPre.Datum
{
    public class Selections
    {
        public Dictionary<int, string> itos = new Dictionary<int, string>();
        public Dictionary<string, int> stoi = new Dictionary<string, int>();

        static Dictionary<System.Type, Selections> CachedFromEnums = 
            new Dictionary<System.Type, Selections>();

        public Selections(string [] sels)
        { 
            for(int i = 0; i < sels.Length; ++i)
            { 
                itos.Add(i, sels[i]);
                stoi.Add(sels[i], i);
            }
        }

        public Selections(Dictionary<int, string> itos)
        {
            this.itos = itos;
            foreach(KeyValuePair<int, string> kvp in itos)
            {
                if(this.stoi.ContainsKey(kvp.Value) == true)
                    continue;

                this.stoi.Add(kvp.Value, kvp.Key);
            }

        }

        public Selections(Dictionary<string, int> stoi)
        {
            this.stoi = stoi;

            foreach (KeyValuePair<string, int> kvp in stoi)
            {
                if (this.itos.ContainsKey(kvp.Value) == true)
                    continue;

                this.itos.Add(kvp.Value, kvp.Key);
            }
        }

        public static Selections FromEnum<k>() where k: System.Enum
        {
            Selections ret;
            if(CachedFromEnums.TryGetValue(typeof(k), out ret) == true)
                return ret;

            // https://stackoverflow.com/a/6454988
            Dictionary<string, int> stoi = new Dictionary<string, int>();
            System.Array array = System.Enum.GetValues(typeof(k));
            foreach(var v in array)
                stoi.Add(v.ToString(), (int)v);

            ret = new Selections(stoi);
            CachedFromEnums.Add(typeof(k), ret);

            return ret;
        }

        public string GetString(int idx)
        {
            if(this.itos.TryGetValue(idx, out string v) == true)
                return v;

            return idx.ToString();
        }

        public int ? GetInt(string str)
        {
            if(this.stoi.TryGetValue(str, out int i) == true)
                return i;

            return null;
        }

        public IEnumerable<string> GetNames()
        { 
            return this.stoi.Keys;
        }
    }

    public class ValEnum : Val
    {
        public int i;

        public readonly Selections selections;

        public override Type ty { get => Type.Enum; }

        public ValEnum(int i, Selections sels)
        { 
            this.selections = sels;
        }

        public static ValEnum FromEnum<k>(int i) where k : System.Enum
        { 
            Selections sels = Selections.FromEnum<k>();
            return new ValEnum(i, sels);
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
            return this.selections.GetString(this.i);
        }

        public override bool SetString(string v)
        { 
            int ? idx = this.selections.GetInt(v);
            if(idx.HasValue)
            { 
                this.i = idx.Value;
                return true;
            }

            int ip;
            if(int.TryParse(v, out ip) == false)
                return false;

            this.i = ip;
            return true;
        }

        public override Val Clone()
        {
            return new ValEnum(this.i, this.selections);
        }

        public override bool SetValue(Val v)
        {
            this.i = v.GetInt();
            return true;
        }

        public override Val Add(Val vb)
        {
            return new ValInt(this.i + vb.GetInt());
        }

        public override Val Mul(Val vb)
        {
            return new ValInt(this.i * vb.GetInt());
        }

        public override Val Min(Val vb)
        {
            return new ValInt(Mathf.Min(this.i, vb.GetInt()));
        }

        public override Val Max(Val vb)
        {
            return new ValInt(Mathf.Max(this.i, vb.GetInt()));
        }
    }
}