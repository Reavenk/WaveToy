using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public GameObject gameObject;
    MeshRenderer meshRenderer;
    MeshFilter meshFilter;
    MeshCollider meshCollider;
    Mesh mesh;


    const float posScroll = 0.01f;
    const float radScroll = 0.001f;

    public EditValue enabled = new EditValue("Enabled", new ValueBool(true));

    public EditValue posx = new EditValue("X", new ValueFloat(0.0f), null, null, new ValueFloat(posScroll));
    public EditValue posy = new EditValue("Y", new ValueFloat(0.0f), null, null, new ValueFloat(posScroll));
    public EditValue rot = new EditValue("Rotation", new ValueFloat(0.0f), new ValueFloat(-180.0f), new ValueFloat(180.0f));

    public EditValue shape = new EditValue("Shape", new ValueEnum(0, "Ellipse", "Square"));
    public EditValue radius1 = new EditValue("Radius", new ValueFloat(0.5f), null, null, new ValueFloat(radScroll));
    public EditValue radius2 = new EditValue("Radius2", new ValueFloat(0.5f), null, null, new ValueFloat(radScroll));
    public EditValue thickness = new EditValue("Thickness", new ValueFloat(0.1f), null, null, new ValueFloat(radScroll));
    public EditValue squared = new EditValue("Squared", new ValueBool(true));
    public EditValue fillMode = new EditValue("Fill", new ValueEnum(0, "Filled", "Hollow"));

    public EditValue angle1 = new EditValue("Angle1", new ValueFloat(-180.0f), new ValueFloat(-180.0f), new ValueFloat(0.0f), new ValueFloat(1.0f));
    public EditValue angle2 = new EditValue("Angle2", new ValueFloat(180.0f), new ValueFloat(0.0f), new ValueFloat(180.0f), new ValueFloat(1.0f));

    public EditValue radiation = new EditValue("Radiation", new ValueEnum(0, "AC", "DC"));

    public EditValue actorType = new EditValue("Type", new ValueEnum(0, "Emitter", "Barrier", "Impedance", "Sensor"));
    public EditValue phase = new EditValue("Phase", new ValueFloat(0.0f), new ValueFloat(0.0f), new ValueFloat(1.0f), new ValueFloat(0.01f));
    public EditValue power = new EditValue("Power", new ValueFloat(1.0f), new ValueFloat(-2.0f), new ValueFloat(2.0f), new ValueFloat(0.01f));
    public EditValue frequency = new EditValue("Freq", new ValueFloat(1.0f), new ValueFloat(0.1f), new ValueFloat(10.0f), new ValueFloat(0.01f));
    public EditValue ior = new EditValue("IOR", new ValueFloat(1.0f), new ValueFloat(0.1f), new ValueFloat(2.0f), new ValueFloat(0.1f));


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
            this.gameObject.layer = scene.inputLayer;
            this.meshRenderer.sharedMaterial = new Material(scene.drawShader);
        }

        this.meshCollider.sharedMesh = this.mesh;
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

        yield return this.angle1;
        yield return this.angle2;

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
                break;

            case Type.Barrier:
                break;

            case Type.Impedance:
                yield return this.ior;
                break;
        }
    }
}
