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

/// <summary>
/// Core wave simulation class.
/// </summary>
[System.Serializable]
public class WaveSimulation
{
    /// <summary>
    /// The three cycled render targets.
    /// </summary>
    RenderTexture[] renderTargs;

    /// <summary>
    /// The signal input texture.
    /// </summary>
    RenderTexture inputTarg;

    /// <summary>
    /// The signal obstacles texture.
    /// </summary>
    RenderTexture simObstacles;

    /// <summary>
    /// The dimensions of the simulation.
    /// This is the size of each texture involved - only one value here because
    /// the textures are square.
    /// </summary>
    private int _dim = 0;
    public int Dim { get=>this._dim; }


    /// <summary>
    /// Public accessor to the input signal texture.
    /// </summary>
    public RenderTexture InputTarget { get => this.inputTarg; }

    /// <summary>
    /// Public accessor to the obstacles texture.
    /// </summary>
    public RenderTexture SimObstacles { get=>this.simObstacles; }

    public RenderTexture SignalRenderTarget {get=>this.renderTargs[2]; }
    public RenderTexture SignalRecent { get=>this.renderTargs[0]; }
    public RenderTexture SignalOlder {get=>this.renderTargs[1]; }

    /// <summary>
    /// Cycle between the textures after a simulation step.
    /// </summary>
    public void CycleSignalBuffers()
    { 
        if(this.renderTargs == null)
            return;

        this.renderTargs =
            new RenderTexture[]
            {
                this.renderTargs[2],
                this.renderTargs[0],
                this.renderTargs[1]
            };
    }

    public void ReverseCycleSignalBuffers()
    {
        this.renderTargs =
            new RenderTexture[]
            {
                this.renderTargs[1],
                this.renderTargs[0],
                this.renderTargs[2]
            };
    }

    /// <summary>
    /// Clear all the buffers of their contents.
    /// </summary>
    public void ClearBuffers()
    {
        if(this.renderTargs == null)
            return;

        RenderTexture rtActive = RenderTexture.active;

        foreach(RenderTexture rt in this.renderTargs)
        {
            RenderTexture.active = rt;
            GL.Clear(false, true, Color.black);
        }

        RenderTexture.active = this.InputTarget;
        GL.Clear(false, true, Color.black);

        RenderTexture.active = this.simObstacles;
        GL.Clear(false, true, Color.white);

        RenderTexture.active = rtActive;
    }

    /// <summary>
    /// Allocate new buffers for the sim.
    /// </summary>
    /// <param name="dim">The dimensions of the textures.</param>
    /// <param name="clear">If true, clears the allocated textures.</param>
    public void AllocateBuffers(int dim, bool clear = true)
    { 
        _dim = Mathf.Max(dim, 2);

        this.renderTargs =
            new RenderTexture[]
            {
                new RenderTexture(dim, dim, 32, RenderTextureFormat.ARGBFloat),
                new RenderTexture(dim, dim, 32, RenderTextureFormat.ARGBFloat),
                new RenderTexture(dim, dim, 32, RenderTextureFormat.ARGBFloat)
            };

        foreach(Texture t in this.renderTargs)
            t.wrapMode = TextureWrapMode.Clamp;

        this.inputTarg = new RenderTexture(dim, dim, 32, RenderTextureFormat.ARGBFloat);
        this.inputTarg.wrapMode = TextureWrapMode.Clamp;

        this.simObstacles = new RenderTexture(dim, dim, 32, RenderTextureFormat.ARGBFloat);
        this.simObstacles.wrapMode = TextureWrapMode.Clamp;

        if (clear == true)
            this.ClearBuffers();
    }
}
