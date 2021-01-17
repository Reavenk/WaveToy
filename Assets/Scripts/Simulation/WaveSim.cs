using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WaveSimulation
{
    RenderTexture[] renderTargs;
    RenderTexture inputTarg;
    RenderTexture simObstacles;

    private int _dim = 0;
    public int Dim { get=>this._dim; }

    public RenderTexture InputTarget { get => this.inputTarg; }
    public RenderTexture SimObstacles { get=>this.simObstacles; }

    public RenderTexture SignalRenderTarget {get=>this.renderTargs[2]; }
    public RenderTexture SignalRecent { get=>this.renderTargs[0]; }
    public RenderTexture SignalOlder {get=>this.renderTargs[1]; }

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

        RenderTexture.active = rtActive;
    }

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
