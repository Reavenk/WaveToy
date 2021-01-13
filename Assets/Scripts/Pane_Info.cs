using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pane_Info : Pane_Base
{
    public PxPre.UIL.Factory uiFactory;

    public void Start()
    {
        this.TestUILayout();
    }

    void TestUILayout()
    {
        PxPre.UIL.EleHost host = new PxPre.UIL.EleHost(this.rectTransform, true);

        PxPre.UIL.UILStack stk = new PxPre.UIL.UILStack(this.uiFactory, host);
        stk.PushVertSizer();
        stk.AddWrapText("Yo ho ho and a bottle of rum!");

    }
}
