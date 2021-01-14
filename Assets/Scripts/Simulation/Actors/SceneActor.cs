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

    GameObject gameObject;
    MeshRenderer meshRenderer;
    MeshFilter meshFilter;
    Mesh mesh;


    public EditValue posx = new EditValue("X", new ValueFloat(0.0f));
    public EditValue posy = new EditValue("Y", new ValueFloat(0.0f));
    public EditValue rot = new EditValue("Rotation", new ValueFloat(0.0f), new ValueFloat(-180.0f), new ValueFloat(180.0f));

    public EditValue shape = new EditValue("Shape", new ValueEnum(0, "Ellipse", "Square"));
    public EditValue radius1 = new EditValue("Radius", new ValueFloat(1.0f));
    public EditValue radius2 = new EditValue("Radius2", new ValueFloat(1.0f));
    public EditValue thickness = new EditValue("Thickness", new ValueFloat(0.1f));
    public EditValue squared = new EditValue("Squared", new ValueBool(true));
    public EditValue fillMode = new EditValue("Fill", new ValueEnum(0, "Filled", "Hollow"));

    public EditValue actorType = new EditValue("Type", new ValueEnum(0, "Emitter", "Barrier", "Impedance", "Sensor"));
    public EditValue phase = new EditValue("Phase", new ValueFloat(0.0f));
    public EditValue power = new EditValue("Power", new ValueFloat(1.0f));
    public EditValue frequency = new EditValue("Freq", new ValueFloat(1.0f));
    public EditValue ior = new EditValue("IOR", new ValueFloat(1.0f));


    public void Destroy()
    { 
        if(this.gameObject == null)
            return;
        
        GameObject.Destroy(this.gameObject);
        this.gameObject = null;
        this.meshFilter = null;
        this.meshRenderer = null;
        this.mesh = null;
    }

    public void UpdateGeometry()
    { 

        if(this.gameObject == null)
            this.gameObject = new GameObject("SceneActor");

        this.gameObject.transform.localPosition = 
            new Vector3(
                this.posx.val.GetFloat(), 
                this.posy.val.GetFloat(), 
                0.0f);

        this.gameObject.transform.localEulerAngles =
            new Vector3(
                0.0f,
                this.rot.val.GetFloat(),
                0.0f);

        Debug.Log("UpdateGeometry()");
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
    }

    public IEnumerable<EditValue> EnumerateRelevantParams()
    { 
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
        if(this.squared.val.GetBool() == true)
            yield return this.radius2;

        yield return this.fillMode;

        if(this.fillMode.val.GetInt() == (int)Fill.Hollow)
            yield return this.thickness;

        yield return this.actorType;

        switch((Type)this.actorType.val.GetInt())
        { 
            case Type.Sensor:
                break;

            case Type.Emitter:
                yield return this.phase;
                yield return this.power;
                yield return this.frequency;
                break;

            case Type.Barrier:
                break;

            case Type.Impedance:
                yield return this.ior;
                break;
        }
    }
}
