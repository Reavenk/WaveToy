using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    // https://web.archive.org/web/20160116150939/http://freespace.virgin.net/hugo.elias/graphics/x_water.htm
    public Texture2D tex;
    public Texture2D tex2;
    int idx = 0;
    const int dim = 256;

    public float divAmt = 50.0f;

    public UnityEngine.UI.RawImage img;


    void Start()
    {
        this.tex = new Texture2D(dim, dim);
        this.tex2 = new Texture2D(dim, dim);

        for(int i = 0; i < dim; ++i)
        { 
            for(int j = 0; j < dim; ++j)
            { 
                tex.SetPixel(i, j, new Color(0.0f, 0.0f, 0.0f, 0.0f));
                tex2.SetPixel(i, j, new Color(0.0f, 0.0f, 0.0f, 0.0f));
            }
        }
        tex.Apply();
        tex2.Apply();
    }

    void Update()
    {
        this.Integrate();
    }

    void Integrate()
    { 
        int id2 = (idx + 1) % 2;


        Texture2D cur = idx == 0 ? tex : tex2;
        Texture2D oth = idx == 0 ? tex2 : tex;

        float f = Mathf.Sin(Time.time * 2.0f * Mathf.PI) * 10.0f;
        cur.SetPixel((int)(100 + (dim / divAmt) * 1), dim / 2, new Color(f, f, f, f));
        cur.SetPixel((int)(100 + (dim / divAmt) * 2), dim / 2, new Color(f, f, f, f));
        cur.SetPixel((int)(100 + (dim / divAmt) * 3), dim / 2, new Color(f, f, f, f));
        cur.SetPixel((int)(100 + (dim / divAmt) * 4), dim / 2, new Color(f, f, f, f));
        cur.SetPixel((int)(100 + (dim / divAmt) * 5), dim / 2, new Color(f, f, f, f));
        cur.SetPixel((int)(100 + (dim / divAmt) * 6), dim / 2, new Color(f, f, f, f));
        cur.SetPixel((int)(100 + (dim / divAmt) * 7), dim / 2, new Color(f, f, f, f));



        //if(Input.GetKey(KeyCode.A) == true)
        //{ 
        //    float f = 1.0f;
        //    cur.SetPixel(dim/2, dim/2, new Color(f, f, f, f));
        //}

        for (int y = 1; y < cur.height - 1; ++y )
        { 
            for(int x = 1; x < cur.width - 1; ++x)
            {
               float b2 = 
                    ((
                        cur.GetPixel(x - 1, y + 0).r + 
                        cur.GetPixel(x + 1, y + 0).r + 
                        cur.GetPixel(x + 0, y - 1).r + 
                        cur.GetPixel(x + 0, y + 1).r) / 2.0f - oth.GetPixel(x, y).r) * 0.9999999701f;

                oth.SetPixel(x, y, new Color(b2, b2, b2, b2));
            }
        }

        oth.Apply();
        img.texture = oth;

        Texture2D swp = tex;
        tex = tex2;
        tex2 = swp;

    }
}
