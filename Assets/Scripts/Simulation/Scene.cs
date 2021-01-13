using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WaveScene
{
    public LayerMask obstacleLayer;
    public LayerMask inputLayer;
    public LayerMask impedanceLayer;

    public Shader drawShader;
    public Material simMaterial;

    public Camera simObstacleCam;
    public Camera simInputCam;

    WaveSimulation waveSim;

    public void Initialize(WaveSimulation ws)
    { 
        this.waveSim = ws;
        this.simInputCam.targetTexture = this.waveSim.InputTarget;
        this.simInputCam.cullingMask = this.inputLayer;

        this.simObstacleCam.targetTexture = this.waveSim.SimObstacles;
        this.simObstacleCam.cullingMask = this.obstacleLayer;
    }

    public void Integrate()
    {
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
    }
}
