using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    // https://web.archive.org/web/20160116150939/http://freespace.virgin.net/hugo.elias/graphics/x_water.htm
    //public Texture2D tex;
    //public Texture2D tex2;

    public RenderTexture [] renderTargs;
    public RenderTexture inputTarg;
    public RenderTexture simObstacles;

    int idx = 0;
    const int dim = 1024;

    public float divAmt = 50.0f;

    public UnityEngine.UI.RawImage img;

    public Material simMat;
    public Material drawMat;

    public Camera simInputCam;
    public Camera simObstablesCam;

    public float modulateRate = 5.0f;

    public PxPre.UIDock.Root dockSys;

    private void Awake()
    {
        this.renderTargs = 
            new RenderTexture[]
            { 
                new RenderTexture(dim, dim, 32, RenderTextureFormat.ARGBFloat),
                new RenderTexture(dim, dim, 32, RenderTextureFormat.ARGBFloat),
                new RenderTexture(dim, dim, 32, RenderTextureFormat.ARGBFloat)
            };

        foreach(Texture t in this.renderTargs)
        {
            t.wrapMode = TextureWrapMode.Clamp;
        }

        this.inputTarg = new RenderTexture(dim, dim, 32, RenderTextureFormat.ARGBFloat);
        this.simInputCam.targetTexture = this.inputTarg;

        this.simObstacles = new RenderTexture(dim, dim, 32, RenderTextureFormat.ARGBFloat);
        this.simObstablesCam.targetTexture = this.simObstacles;

    }

    void Start()
    {
        RenderTexture.active = this.renderTargs[0];
        GL.Clear(false, true, Color.black);
        RenderTexture.active = this.renderTargs[1];
        GL.Clear(false, true, Color.black);
        RenderTexture.active = this.renderTargs[2];
        GL.Clear(false, true, Color.black);

        this.img.material.SetTexture("_Obs", this.simObstablesCam.targetTexture);
    }

    void Update()
    {
        this.simInputCam.Render();
        this.simObstablesCam.Render();

        
        simMat.SetTexture("_T1", this.renderTargs[0]);
        simMat.SetTexture("_T2", this.renderTargs[1]);
        simMat.SetTexture("_Input", this.inputTarg);
        simMat.SetTexture("_Obs", this.simObstacles);
        simMat.SetVector("_InvDim", new Vector4( 1.0f / (float)dim, 1.0f / (float)dim, 0.0f, 0.0f));

        this.simMat.SetPass(0);
        RenderTexture.active = this.renderTargs[2];
        GL.PushMatrix();
        GL.LoadIdentity();
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

        this.renderTargs =
            new RenderTexture[]
            {
                this.renderTargs[2],
                this.renderTargs[0],
                this.renderTargs[1]
            };
        
        this.img.texture = this.renderTargs[0];

        //simMat.SetTexture(
        //this.Integrate();

        float f = Mathf.Sin(Time.time * Mathf.PI * 2 * this.modulateRate) * 0.5f;
        drawMat.SetColor("_Color", new Color(f, f, f, 1.0f));
    }

    private void OnGUI()
    {
    }
}
