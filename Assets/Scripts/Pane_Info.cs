using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pane_Info : Pane_Base
{
    public PxPre.UIL.Factory uiFactory;

    PxPre.UIL.EleHost host;
    public PxPre.UIL.EleVertScrollRgn scroll;
    public PxPre.UIL.EleText message;

    public void Start()
    {
        this.host = new PxPre.UIL.EleHost(this.rectTransform, true);
        PxPre.UIL.UILStack stk = new PxPre.UIL.UILStack(this.uiFactory, host);
        stk.PushVertSizer();
        this.scroll = stk.PushVertScrollRect(1.0f, PxPre.UIL.LFlag.Grow);
        stk.PushVertSizer().Chn_Border(10.0f, 0.0f, 10.0f, 0.0f);
        this.message = stk.AddWrapText("Add content to create sim.");
    }

    public override void OnCleared()
    {
        this.message.text.text = "";
    }

    public override void OnLoaded()
    {
        host.LayoutInRT();
        this.message.text.text = this.mgr.documentComment;

    }
}
