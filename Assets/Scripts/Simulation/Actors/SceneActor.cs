using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PxPre.Datum;

public class SceneActor
{
    public enum Type
    { 
        Emitter,
        Barrier,
        Impedance,
        Sensor
    }

    public enum Shape
    { 
        Ellipse,
        Square
    }

    public enum Fill
    { 
        Filled,
        Hollow
    }

    public enum RadiationType
    { 
        AC,
        DC
    }

    public enum EmissionMode
    { 
        Reflective,
        Additive
    }

    public GameObject gameObject;
    MeshRenderer meshRenderer;
    MeshFilter meshFilter;
    MeshCollider meshCollider;
    Mesh mesh;


    const float posScroll = 0.01f;
    const float radScroll = 0.001f;

    public EditValue enabled = new EditValue("Enabled", new ValBool(true));

    public EditValue posx = new EditValue("X", new ValFloat(0.0f), null, null, new ValFloat(posScroll));
    public EditValue posy = new EditValue("Y", new ValFloat(0.0f), null, null, new ValFloat(posScroll));
    public EditValue rot = new EditValue("Rotation", new ValFloat(0.0f), new ValFloat(-180.0f), new ValFloat(180.0f));

    public EditValue shape = new EditValue("Shape", ValEnum.FromEnum<Shape>(0));
    public EditValue radius1 = new EditValue("Radius", new ValFloat(0.5f), null, null, new ValFloat(radScroll));
    public EditValue radius2 = new EditValue("Radius2", new ValFloat(0.5f), null, null, new ValFloat(radScroll));
    public EditValue thickness = new EditValue("Thickness", new ValFloat(0.1f), null, null, new ValFloat(radScroll));
    public EditValue squared = new EditValue("Squared", new ValBool(true));
    public EditValue fillMode = new EditValue("Fill", ValEnum.FromEnum<Fill>(0));

    public EditValue angle1 = new EditValue("Angle1", new ValFloat(-180.0f), new ValFloat(-180.0f), new ValFloat(0.0f), new ValFloat(1.0f));
    public EditValue angle2 = new EditValue("Angle2", new ValFloat(180.0f), new ValFloat(0.0f), new ValFloat(180.0f), new ValFloat(1.0f));

    public EditValue radiation = new EditValue("Radiation", ValEnum.FromEnum<RadiationType>(0));

    public EditValue actorType = new EditValue("Type", ValEnum.FromEnum<Type>(0));
    public EditValue phase = new EditValue("Phase", new ValFloat(0.0f), new ValFloat(0.0f), new ValFloat(1.0f), new ValFloat(0.01f));
    public EditValue power = new EditValue("Power", new ValFloat(1.0f), new ValFloat(-2.0f), new ValFloat(2.0f), new ValFloat(0.01f));
    public EditValue frequency = new EditValue("Freq", new ValFloat(1.0f), new ValFloat(0.1f), new ValFloat(10.0f), new ValFloat(0.01f));
    public EditValue ior = new EditValue("IOR", new ValFloat(1.0f), new ValFloat(0.1f), new ValFloat(2.0f), new ValFloat(0.1f));

    public EditValue emission = new EditValue("Emission", ValEnum.FromEnum<EmissionMode>(0));


    public void Destroy()
    { 
        if(this.gameObject == null)
            return;
        
        GameObject.Destroy(this.gameObject);
        this.gameObject = null;
        this.meshFilter = null;
        this.meshRenderer = null;
        this.meshCollider = null;
        this.mesh = null;
    }

    public void Tick(WaveScene scene)
    { 
        if(this.actorType.IntVal == (int)Type.Emitter)
        {
            float power = this.power.FloatVal;

            if(this.radiation.StringVal == "AC")
            {
                float f = Mathf.Sin((scene.Time + this.phase.FloatVal)* Mathf.PI * 2 * this.frequency.FloatVal) * power;
                this.meshRenderer.sharedMaterial.SetColor("_Color", new Color(f, f, f, 1.0f));
            }
            else
            {
                this.meshRenderer.sharedMaterial.SetColor("_Color", new Color(power, power, power, 1.0f));
            }
        }
    }

    public void UpdateGeometry(WaveScene scene)
    { 

        if(this.gameObject == null)
        {
            this.gameObject = new GameObject("SceneActor");

            SceneActorTag sat = this.gameObject.AddComponent<SceneActorTag>();
            sat.actor = this;
        }

        this.gameObject.SetActive(this.enabled.BoolVal);

        this.gameObject.transform.localPosition = 
            new Vector3(
                this.posx.val.GetFloat(), 
                this.posy.val.GetFloat(), 
                0.0f);

        this.gameObject.transform.localEulerAngles =
            new Vector3(
                0.0f,
                0.0f,
                this.rot.val.GetFloat());

        if (this.mesh == null)
            this.mesh = new Mesh();

        this.mesh.Clear();

        if(this.meshRenderer == null)
        { 
            this.meshRenderer = this.gameObject.AddComponent<MeshRenderer>();
            this.meshFilter = this.gameObject.AddComponent<MeshFilter>();
            this.meshCollider = this.gameObject.AddComponent<MeshCollider>();
            this.meshFilter.mesh = this.mesh;
        }

        float r1 = this.radius1.val.GetFloat();
        float r2 = this.squared.val.GetBool() ? r1 : this.radius2.val.GetFloat(); 

        if (this.shape.val.GetInt() == (int)Shape.Square)
        {
            float thk = this.thickness.FloatVal;
            if(this.fillMode.IntVal == (int)Fill.Filled || thk >= r1 || thk >= r2)
            {
                Vector3 [] rv = 
                    new Vector3[]
                    { 
                        new Vector3(-r1, r2, 0.0f),
                        new Vector3(-r1,-r2, 0.0f),
                        new Vector3( r1,-r2, 0.0f),
                        new Vector3( r1, r2, 0.0f)
                    };

                int [] ri = new int [] { 2, 1, 0, 3, 2, 0};

                this.mesh.SetVertices(rv);
                this.mesh.SetIndices( ri, MeshTopology.Triangles, 0);
            }
            else if(r1 > 0.0f && r2 > 0.0f)
            { 
                float cornerAng = Mathf.Atan(r1/r2) / Mathf.PI * 180.0f;

                thk = Mathf.Min(thk, r1/ r2);

                float r1i = r1 - thk;
                float r2i = r2 - thk;

                float a1v = -this.angle1.FloatVal;
                float b1v = this.angle2.FloatVal;

                List<Vector3> lstv = new List<Vector3>();
                List<int> lsti = new List<int>();

                Vector3 av1o = new Vector3(r1, 0.0f);
                Vector3 av2o = new Vector3(r1, -r2);
                Vector3 av1i = new Vector3(r1i, 0.0f);
                Vector3 av2i = new Vector3(r1i, -r2i);
                if(PerformRayQuad(av1o, av2o, av1i, av2i, Mathf.InverseLerp(0.0f, cornerAng, a1v), true, lstv, lsti) == true)
                { 
                    Vector3 av3o = new Vector3(-r1, -r2);
                    Vector3 av3i = new Vector3(-r1i, -r2i);

                    if(PerformRayQuad(av2o, av3o, av2i, av3i, Mathf.InverseLerp(cornerAng, 180.0f - cornerAng, a1v), true, lstv, lsti) == true)
                    {
                        Vector3 av4o = new Vector3(-r1, 0.0f);
                        Vector3 av4i = new Vector3(-r1i, 0.0f);

                        PerformRayQuad(av3o, av4o, av3i, av4i, Mathf.InverseLerp(180.0f - cornerAng, 180.0f, a1v), true, lstv, lsti);
                    }
                }

                Vector3 bv1o = new Vector3(r1, 0.0f);
                Vector3 bv2o = new Vector3(r1, r2);
                Vector3 bv1i = new Vector3(r1i, 0.0f);
                Vector3 bv2i = new Vector3(r1i, r2i);
                if (PerformRayQuad(bv1o, bv2o, bv1i, bv2i, Mathf.InverseLerp(0.0f, cornerAng, b1v), false, lstv, lsti) == true)
                { 
                    Vector3 bv3o = new Vector3(-r1, r2);
                    Vector3 bv3i = new Vector3(-r1i, r2i);

                    if (PerformRayQuad(bv2o, bv3o, bv2i, bv3i, Mathf.InverseLerp(cornerAng, 180.0f - cornerAng, b1v), false, lstv, lsti) == true)
                    { 
                        Vector3 bv4o = new Vector3(-r1, 0.0f);
                        Vector3 bv4i = new Vector3(-r1i, 0.0f);

                        PerformRayQuad(bv3o, bv4o, bv3i, bv4i, Mathf.InverseLerp(180.0f - cornerAng, 180.0f, b1v), false, lstv, lsti);
                    }
                }

                this.mesh.SetVertices(lstv.ToArray());
                this.mesh.SetIndices(lsti.ToArray(), MeshTopology.Triangles, 0);
            }
        }
        else if (this.shape.val.GetInt() == (int)Shape.Ellipse)
        {
            int circVerts = 24;
            float th1 = this.angle1.FloatVal / 180.0f * Mathf.PI;
            float th2 = this.angle2.FloatVal / 180.0f * Mathf.PI;

            if(this.fillMode.IntVal == (int)Fill.Filled)
            {
                Vector3 [] rv = new Vector3[circVerts + 2];
                rv[circVerts] = Vector3.zero;

                int [] ri = new int [circVerts * 3];

                for(int i = 0; i < circVerts + 1; ++i)
                { 
                    float l = (float)i/(float)circVerts;
                    float th = Mathf.Lerp(th1, th2, l);

                    float x = Mathf.Cos(th);
                    float y = Mathf.Sin(th);

                    rv[i] = new Vector3(x * r1, y * r2, 0.0f);
                }

                for(int i = 0; i < circVerts; ++i)
                { 
                    int idx = i * 3;
                    ri[idx + 2] = circVerts + 1;
                    ri[idx + 1] = i + 0;
                    ri[idx + 0] = i + 1;
                }

                this.mesh.SetVertices(rv);
                this.mesh.SetIndices(ri, MeshTopology.Triangles, 0);
            }
            else
            {
                Vector3[] rv = new Vector3[(circVerts + 1) * 2];

                float thk = this.thickness.FloatVal;

                float innerRad1 = Mathf.Max(0.0f, r1 - thk);
                float innerRad2 = Mathf.Max(0.0f, r2 - thk);

                for (int i = 0; i < circVerts + 1; ++i)
                {
                    float l = (float)i / (float)circVerts;
                    float th = Mathf.Lerp(th1, th2, l);

                    float x = Mathf.Cos(th);
                    float y = Mathf.Sin(th);

                    rv[i * 2 + 0] = new Vector3(x * r1, y * r2, 0.0f);
                    rv[i * 2 + 1] = new Vector3(x * innerRad1, y * innerRad2, 0.0f);
                }
                int[] ri = new int[(circVerts + 1) * 6];
                int idxT = 0;
                int idxV = 0;
                for(int i = 0; i < circVerts; ++i)
                { 
                    ri[idxT + 0] = idxV + 0;
                    ri[idxT + 1] = idxV + 1;
                    ri[idxT + 2] = idxV + 2;
                    ri[idxT + 3] = idxV + 1;
                    ri[idxT + 4] = idxV + 3;
                    ri[idxT + 5] = idxV + 2;

                    idxV += 2;
                    idxT += 6;
                }

                this.mesh.SetVertices(rv);
                this.mesh.SetIndices(ri, MeshTopology.Triangles, 0);
            }
        }

        int ty = this.actorType.val.GetInt();
        if (ty == (int)Type.Barrier)
        {
            this.gameObject.layer = scene.obstacleLayer;
            this.meshRenderer.sharedMaterial = scene.matObstacle;
        }
        else if (ty == (int)Type.Emitter)
        {
            Shader sDraw = null;
            switch((EmissionMode)this.emission.IntVal)
            { 
                case EmissionMode.Additive:
                    sDraw = scene.drawShaderAdd;
                    break;

                case EmissionMode.Reflective:
                    sDraw = scene.drawShader;
                    break;
            }

            this.gameObject.layer = scene.inputLayer;
            this.meshRenderer.sharedMaterial = new Material(sDraw);
        }

        this.meshCollider.sharedMesh = this.mesh;
    }

    static bool PerformRayQuad(
        Vector2 outerStart, 
        Vector2 outerEnd, 
        Vector2 innerStart, 
        Vector2 innerEnd,
        float lambda,
        bool invWinding,
        List<Vector3> lstv,
        List<int> lsti)
    { 
        if(lambda <= 0.0f)
            return false;

        int idx = lstv.Count;
        if(lambda == 1.0f)
        { 
            lstv.Add(outerStart);
            lstv.Add(outerEnd);
            lstv.Add(innerStart);
            lstv.Add(innerEnd);
        }
        else
        {
            lstv.Add(outerStart);
            lstv.Add(Vector3.Lerp(outerStart, outerEnd, lambda));
            lstv.Add(innerStart);
            lstv.Add(Vector3.Lerp(innerStart, innerEnd, lambda));
        }

        if (invWinding == false)
        {
            lsti.Add(idx + 2);
            lsti.Add(idx + 1);
            lsti.Add(idx + 0);
            //
            lsti.Add(idx + 2);
            lsti.Add(idx + 3);
            lsti.Add(idx + 1);
        }
        else
        {
            lsti.Add(idx + 0);
            lsti.Add(idx + 1);
            lsti.Add(idx + 2);
            //
            lsti.Add(idx + 1);
            lsti.Add(idx + 3);
            lsti.Add(idx + 2);
        }

        return true;
    }

    public EditValue ? GetParam(string name)
    { 
        foreach(EditValue ev in this.EnumerateAllParams())
        { 
            if(ev.name == name)
                return ev;
        }

        return null;
    }

    public IEnumerable<EditValue> EnumerateAllParams()
    {
        yield return this.enabled;
        yield return this.posx;
        yield return this.posy;
        yield return this.rot;
        yield return this.shape;
        yield return this.radius1;
        yield return this.radius2;
        yield return this.thickness;
        yield return this.squared;
        yield return this.fillMode;
        yield return this.actorType;
        yield return this.phase;
        yield return this.power;
        yield return this.frequency;
        yield return this.ior;
        yield return this.angle1;
        yield return this.angle2;
        yield return this.radiation;
        yield return this.emission;
    }

    public IEnumerable<EditValue> EnumerateRelevantParams()
    { 
        yield return this.enabled;
        yield return this.posx;
        yield return this.posy;
        yield return this.rot;

        yield return this.shape;

        switch((Shape)this.shape.val.GetInt())
        {
            case Shape.Ellipse:
                yield return this.squared;
                break;

            case Shape.Square:
                yield return this.squared;
                break;
        }

        yield return this.radius1;
        if(this.squared.val.GetBool() == false)
            yield return this.radius2;

        if(
            this.shape.IntVal == (int)Shape.Ellipse || 
            this.fillMode.IntVal == (int)Fill.Hollow)
        {
            yield return this.angle1;
            yield return this.angle2;
        }

        yield return this.fillMode;

        if(this.fillMode.val.GetInt() == (int)Fill.Hollow)
        {
            yield return this.thickness;
        }

        yield return this.actorType;

        switch((Type)this.actorType.val.GetInt())
        { 
            case Type.Sensor:
                break;

            case Type.Emitter:
                yield return this.radiation;

                if(this.radiation.StringVal == "AC")
                {
                    yield return this.phase;
                    yield return this.power;
                    yield return this.frequency;
                }

                yield return this.emission;
                break;

            case Type.Barrier:
                break;

            case Type.Impedance:
                yield return this.ior;
                break;
        }
    }
}
