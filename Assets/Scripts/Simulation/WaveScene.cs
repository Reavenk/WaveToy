using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WaveScene
{
    public int obstacleLayer;
    public int inputLayer;
    public int impedanceLayer;

    public Shader drawShader;
    public Material simMaterial;
    public Material matObstacle;

    public Camera simObstacleCam;
    public Camera simInputCam;

    WaveSimulation waveSim;

    float time = 0.0f;
    public float Time {get=>this.time; }

    List<SceneActor> actors = new List<SceneActor>();
    public IEnumerable<SceneActor> Actors {get => this.actors; }

    public bool HasActor(SceneActor actor)
    { 
        return this.actors.Contains(actor);
    }

    public bool RemoveActor(SceneActor actor)
    { 
        return this.actors.Remove(actor);
    }

    public bool AddActor(SceneActor actor)
    {
        if(this.actors.Contains(actor) == true)
            return false;

        this.actors.Add(actor);
        return true;
    }

    public void Initialize(WaveSimulation ws)
    { 
        this.waveSim = ws;
        this.simInputCam.targetTexture = this.waveSim.InputTarget;
        this.simInputCam.cullingMask = 1 << this.inputLayer;

        this.simObstacleCam.targetTexture = this.waveSim.SimObstacles;
        this.simObstacleCam.cullingMask = 1 << this.obstacleLayer;
    }

    public void UpdateBuffersWithoutIntegration()
    {
        this.simInputCam.Render();
        this.simObstacleCam.Render();
    }

    public void Integrate()
    {
        foreach(SceneActor sa in this.actors)
            sa.Tick(this);

        this.simInputCam.Render();
        this.simObstacleCam.Render();

        simMaterial.SetTexture("_T1", this.waveSim.SignalRecent);
        simMaterial.SetTexture("_T2", this.waveSim.SignalOlder);
        simMaterial.SetTexture("_Input", this.waveSim.InputTarget);
        simMaterial.SetTexture("_Obs", this.waveSim.SimObstacles);
        simMaterial.SetVector(
            "_InvDim", 
            new Vector4(
                1.0f / (float)this.waveSim.Dim, 
                1.0f / (float)this.waveSim.Dim, 
                0.0f, 
                0.0f));

        this.simMaterial.SetPass(0);

        RenderTexture.active = this.waveSim.SignalRenderTarget;
        GL.PushMatrix();
        GL.LoadOrtho();

        GL.Begin(GL.QUADS);
        GL.TexCoord2(0.0f, 0.0f);
        GL.Vertex3(0.0f, 0.0f, 0.0f);

        GL.TexCoord2(0.0f, 1.0f);
        GL.Vertex3(0.0f, 1.0f, 0.0f);

        GL.TexCoord2(1.0f, 1.0f);
        GL.Vertex3(1.0f, 1.0f, 0.0f);

        GL.TexCoord2(1.0f, 0.0f);
        GL.Vertex3(1.0f, 0.0f, 0.0f);
        GL.End();

        GL.PopMatrix();
        RenderTexture.active = null;

        this.waveSim.CycleSignalBuffers();

        this.time += 1.0f / 30.0f;
    }

    public bool DestroyActor(SceneActor actor)
    {
        bool found = this.actors.Remove(actor);
        actor.Destroy();

        return found;
    }
}
