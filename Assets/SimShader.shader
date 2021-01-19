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

// The main FDTD wave simulation shader.
Shader "Unlit/SimShader"
{
    Properties
    {
        // The most recent texture (t0)
        _T1 ("Texture", 2D) = "white" {}

        // The 2nd most recent texture (t-1)
        _T2("Texture", 2D) = "white" {}

        // The input wave data to inject into the simulation. The component
        // values can be positive or negative.
        _Input("Input", 2D) = "black" {}

        // The texture defining where the obstacles are.
        // 1.0 for no obstacle.
        // 0.0 for impassible obstacle
        // Any value in between for an impediding material
        _Obs("Obstacles", 2D) = "black"{}

        // The inverse dimensions of the image. Image addresses in the shader are
        // set by UV when we want pixels. So we need to change the UV to pixel addresses
        // by dividing the UV by the image dimensions.
        _InvDim("Sim Incr", Vector) = (0,0,0,0)

        // The amount to decay the simulation.
        _Decay("Decay", Range(0,1)) = 0.9999
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _T1;      // Texture for last frame
            sampler2D _T2;      // Texture for frame before the previous frame.
            sampler2D _Obs;     // Rendered obstacles [0,1]
            sampler2D _Input;   // Rendered input 
            
            float4 _T1_ST;      // The UV mapping for _T1 is going to be used for everything

            float2 _InvDim;     // Inverse dimensions, to turn UV space into per-pixel sampled space
            float _Decay;       // The amount of signal decay per frame.

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _T1);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 pxTm1 = tex2D(_T2, i.uv);

                // Sample the obstacles. A value of 1.0 means unobstructed. A value of 
                // 0.0 means completly obstructed.
                float4 obj = tex2D(_Obs, i.uv);
                if (obj.a <= 0.0)
                {
                    float4 pxT = 0;// tex2D(_T2, i.uv);
                    return pxT;
                }

                // Not the actual IOR, just a coeff modeling its behaviour.
                // The larger the number (up to 1.0), the more freely the wave
                // can move.
                // When it gets closer to 0, we lower the kernel to simulate slower
                // movement. When we get to 0, that's a special value that represents 
                // an obstable (already handled in the if statement above).
                float invior = obj.a;
                float2 kern = _InvDim.x * invior;

                // KERNEL 1
                //////////////////////////////////////////////////
                float4 pxl = tex2D(_T1, i.uv + float2(-kern.x, 0.0));

                // If it's near/off the edge, we do some shenanegans to try to avoid reflections
                // as much as possible.
                if (i.uv.x - _InvDim.x <= _InvDim.x * 0.5)
                    pxl = pxTm1;

                // KERNEL 2
                //////////////////////////////////////////////////
                float4 pxr = tex2D(_T1, i.uv + float2(kern.x, 0.0));
                if (i.uv.x + _InvDim.x >= 1.0 - _InvDim.x * 0.5)
                    pxr = pxTm1;

                // KERNEL 3
                ////////////////////////////////////////////////// 
                float4 pxt = tex2D(_T1, i.uv + float2(0.0, kern.y));
                if (i.uv.y + _InvDim.y >= 1.0 - _InvDim.y * 0.5)
                    pxt = pxTm1;

                // KERNEL 4
                //////////////////////////////////////////////////
                float4 pxb = tex2D(_T1, i.uv + float2(0.0, -kern.y));
                if (i.uv.y - _InvDim.y <= _InvDim.y * 0.5)
                    pxb = pxTm1;

                //
                //////////////////////////////////////////////////
                float4 pxC = (pxl + pxr + pxt + pxb);
                float4 ip = tex2D(_Input, i.uv);
                pxC = lerp(pxC, ip, ip.w);


                // Apply the decay factor
                //////////////////////////////////////////////////
                float4 v = (pxC / 2.0 - pxTm1) * _Decay;
                v = lerp(v, ip, ip.w);
                return v;
            }
            ENDCG
        }
    }
}
