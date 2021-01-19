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

[System.Serializable]
public class WaveScene
{
    /// <summary>
    /// The Unity layer for GameObjects that are obstacles (or impedances).
    /// </summary>
    public int obstacleLayer;

    /// <summary>
    /// The Unity layer for GameObjects that are inputs.
    /// </summary>
    public int inputLayer;

    /// <summary>
    /// The draw shader for inputs, obstacles and impedances.
    /// </summary>
    public Shader drawShader;

    /// <summary>
    /// Experimental shader idea for adding the sim data.
    /// </summary>
    /// <remarks>Pretty sure this doesn't work because we blit an image of the input to the sim
    /// instead of drawing inputs directly to the sim.</remarks>
    public Shader drawShaderAdd;

    /// <summary>
    /// The material for the simulation.
    /// </summary>
    public Material simMaterial;

    /// <summary>
    /// The material for obstacles. While it will have drawShader, obstacles have their own dedicated
    /// shader because they all have the same material parameters.
    /// </summary>
    public Material matObstacle;

    /// <summary>
    /// The camera that renders obstacles directly to the sim obstacles RTT.
    /// </summary>
    public Camera simObstacleCam;

    /// <summary>
    /// The camera that renders inputs directly to the sim input RTT.
    /// </summary>
    public Camera simInputCam;

    /// <summary>
    /// The simulation object that has the simulation textures. 
    /// </summary>
    WaveSimulation waveSim;

    /// <summary>
    /// The currnt simulation time.
    /// </summary>
    float time = 0.0f;

    /// <summary>
    /// Public accessor for the current simulation time.
    /// </summary>
    public float Time {get=>this.time; }

    List<SceneActor> actors = new List<SceneActor>();
    public IEnumerable<SceneActor> Actors {get => this.actors; }

    public bool HasActor(SceneActor actor)
    { 
        return this.actors.Contains(actor);
    }

    /// <summary>
    /// Remove an actor from the scene.
    /// </summary>
    /// <param name="actor">The actor to remove.</param>
    /// <returns>True, if successful. Else, false.</returns>
    public bool RemoveActor(SceneActor actor)
    { 
        return this.actors.Remove(actor);
    }

    /// <summary>
    /// Adds an actor to the scene.
    /// </summary>
    /// <param name="actor">The actor to add.</param>
    /// <returns>True, if successful. Else, false.</returns>
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

    /// <summary>
    /// Draws the simulation and obstacle data, but doesn't perform a simulation
    /// tick.
    /// 
    /// Used to keep visual information up to date while paused. This allows the user
    /// to modify things while the simulation is paused.
    /// </summary>
    public void UpdateBuffersWithoutIntegration()
    {
        this.simInputCam.Render();
        this.simObstacleCam.Render();
    }

    /// <summary>
    /// Perform an update tick in the simulation.
    /// </summary>
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

        // Render a quad to execute the simulation shader.

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

    /// <summary>
    /// Remove an actor from the scene and destroy its assets.
    /// </summary>
    /// <param name="actor">The actor to remove and destroy.</param>
    /// <returns>True, if successful. Els, false.</returns>
    public bool DestroyActor(SceneActor actor)
    {
        bool found = this.actors.Remove(actor);
        actor.Destroy();

        return found;
    }

    /// <summary>
    /// Destroy and remove all actors from the scene.
    /// </summary>
    public void Clear()
    {
        // Might as well reset the time, hopefully it'll help maintain precision
        // if the app is running for a long period of time.
        this.time = 0.0f;

        foreach(SceneActor sa in this.actors)
        { 
            if(sa.gameObject != null)
                GameObject.Destroy(sa.gameObject);
        }
        this.actors.Clear();
    }

    /// <summary>
    /// Save all actors to an XML file.
    /// </summary>
    /// <param name="comment">The comment to append to the XML file.</param>
    /// <returns>The generated XMLDocument representing all the actors in the scene.</returns>
    public System.Xml.XmlDocument Save(string comment)
    {
        System.Xml.XmlDocument doc = new System.Xml.XmlDocument();

        System.Xml.XmlElement eleRoot = doc.CreateElement("wavscene");
        doc.AppendChild(eleRoot);

        if(string.IsNullOrEmpty(comment) == false)
        {
            System.Xml.XmlElement eleCom = doc.CreateElement("comment");
            eleCom.InnerText = comment;
            eleRoot.AppendChild(eleCom);
        }

        foreach (SceneActor sa in this.actors)
        {
            System.Xml.XmlElement eleActor = doc.CreateElement("actor");

            foreach (EditValue ev in sa.EnumerateAllParams())
            { 
                System.Xml.XmlElement eleParam = doc.CreateElement("param");
                eleParam.SetAttribute("name", ev.name);
                eleParam.SetAttribute("value", ev.val.GetString());

                eleActor.AppendChild(eleParam);
            }
            eleRoot.AppendChild(eleActor);
        }

        return doc;
    }

    /// <summary>
    /// Load a previously saved XML file.
    /// </summary>
    /// <param name="doc">The document to load.</param>
    /// <param name="clear">If true, clears out the current scene first. Else, loaded actors will be appended.</param>
    /// <param name="comment">The comment (if any found) in the document.</param>
    /// <returns>True, if successful. Else, false.</returns>
    public bool Load(System.Xml.XmlDocument doc, bool clear, out string comment)
    { 
        if(clear == true)
            this.Clear();

        comment = "";

        foreach (System.Xml.XmlElement ele in doc.DocumentElement)
        { 
            if(ele.Name == "actor")
            { 
                SceneActor sa = new SceneActor();

                foreach(System.Xml.XmlElement p in ele)
                { 
                    if(p.Name != "param")
                        continue;

                    System.Xml.XmlAttribute attrName = p.Attributes["name"];
                    if(attrName == null)
                        continue;

                    EditValue ? ev = sa.GetParam(attrName.Value);
                    if(ev.HasValue == false)
                        continue;

                    System.Xml.XmlAttribute attrValue = p.Attributes["value"];
                    if(attrValue == null)
                        continue;

                    ev.Value.val.SetString( attrValue.Value);
                }

                sa.UpdateGeometry(this);
                this.AddActor(sa);
            }
            else if(ele.Name == "comment")
                comment = ele.InnerText;
        }

        return true;
    }
}
