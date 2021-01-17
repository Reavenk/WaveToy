using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PxPre.Datum;

public class Pane_Properties : 
    Pane_Base,
    PxPre.Tree.ITreeHandler
{
    public class NodeWidgets
    {
        public struct ParamNodePair
        {
            public PxPre.Tree.Node node;
            public ValueEditor_Base param;
        }

        public Dictionary<string, ParamNodePair> widgets =
            new Dictionary<string, ParamNodePair>();

        public void Destroy()
        {
            foreach (KeyValuePair<string, ParamNodePair> kvp in this.widgets)
                GameObject.Destroy(kvp.Value.param.gameObject);

            this.widgets.Clear();
        }
    }

    public PxPre.Tree.Tree tree;

    public UnityEngine.UI.ScrollRect treeScroll;
    public UnityEngine.UI.ScrollRect optionScroll;

    public int treesSyncRecursionGuard = 0;

    PxPre.Tree.Node nodeWorld;
    PxPre.Tree.Node nodeDecay;
    PxPre.Tree.Node nodeShapes;

    public GameObject prefabWCheckbox;
    public GameObject prefabWPulldown;
    public GameObject prefabWSpinner;
    public GameObject prefabWRotation;
    public GameObject prefabSpinnerReset;

    public Sprite tyicoEllipseFilled;
    public Sprite tyicoEllipseHollow;
    public Sprite tyicoRectFilled;
    public Sprite tyicoRectHollow;

    public Sprite icotogOn;
    public Sprite icotogOff;

    Dictionary<SceneActor, PxPre.Tree.Node> actorToNode = new Dictionary<SceneActor, PxPre.Tree.Node>();
    Dictionary<PxPre.Tree.Node, SceneActor> nodeToActor = new Dictionary<PxPre.Tree.Node, SceneActor>();
    Dictionary<SceneActor, NodeWidgets> actorToParams = new Dictionary<SceneActor, NodeWidgets>();

    Dictionary<PxPre.Tree.Node, ValueEditor_Base> environmentItems = 
        new Dictionary<PxPre.Tree.Node, ValueEditor_Base>();

    ValueEditor_Base decayEd;

    public UnityEngine.UI.Toggle showAllToggle;


    private void Start()
    {
        const int treeRecursionLimit = 0;
        this.treeScroll.onValueChanged.AddListener(
            (x)=>
            { 
                if(this.treesSyncRecursionGuard > treeRecursionLimit)
                    return;
                
                ++this.treesSyncRecursionGuard;
                
                this.optionScroll.verticalNormalizedPosition =
                    this.treeScroll.verticalNormalizedPosition;
                this.StartCoroutine(_SetVertScroll(this.optionScroll, this.treeScroll.verticalNormalizedPosition));

                --this.treesSyncRecursionGuard;
            });

        this.optionScroll.onValueChanged.AddListener(
            (x)=>
            { 
                if(this.treesSyncRecursionGuard > treeRecursionLimit)
                    return;
                
                ++this.treesSyncRecursionGuard;
                
                this.treeScroll.verticalNormalizedPosition = 
                    this.optionScroll.verticalNormalizedPosition;
                this.StartCoroutine(_SetVertScroll(this.treeScroll, this.optionScroll.verticalNormalizedPosition));

                --this.treesSyncRecursionGuard;
            });

        this.tree.subscribers.Add(this);

        this.nodeWorld = this.tree.AddNode("Environment", null);
        this.nodeDecay = this.tree.AddNode("Decay", this.nodeWorld);
        this.nodeShapes = this.tree.AddNode("Shapes", null);

        GameObject goDecay = GameObject.Instantiate(this.prefabSpinnerReset);
        goDecay.transform.SetParent(this.optionScroll.content);
        this.decayEd = goDecay.GetComponent<ValueEditor_Base>();
        this.decayEd.Init(this.mgr, null, this.mgr.evDecay);
        this.decayEd.OnUpdateValue();
        this.environmentItems.Add(this.nodeDecay, this.decayEd);
    }

    public IEnumerator _SetVertScroll(UnityEngine.UI.ScrollRect sr, float normPos)
    {
        yield return new WaitForEndOfFrame();

        ++this.treesSyncRecursionGuard;
        sr.verticalNormalizedPosition = normPos;
        --this.treesSyncRecursionGuard;
    }

    public override void OnActorAdded(SceneActor actor)
    { 
        if(this.showAllToggle.isOn == false)
            return;

        CreateActorTreeNode(actor);
    }

    public void CreateActorTreeNode(SceneActor actor)
    {
        PxPre.Tree.Node actorNode = this.tree.AddNode("Actor", this.nodeShapes);
        RefreshActorNodeIcons(actor, actorNode);
        this.nodeToActor.Add(actorNode, actor);
        this.actorToNode.Add(actor, actorNode);
        this.actorToParams.Add(actor, new NodeWidgets());
        this.RefreshActorParams(actor);
    }

    public void RebuildActorTree()
    { 
        this.nodeShapes.DestroyChildren();

        foreach(KeyValuePair < SceneActor, NodeWidgets > kvp in this.actorToParams)
            kvp.Value.Destroy();

        if(this.showAllToggle.isOn == true)
            this.nodeShapes.Label = "Shapes";
        else
            this.nodeShapes.Label = "Selected";

        this.actorToNode.Clear();
        this.nodeToActor.Clear();
        this.actorToParams.Clear();

        if (this.showAllToggle.isOn == true)
        { 
            foreach(SceneActor actor in this.mgr.waveScene.Actors)
            {
                PxPre.Tree.Node actorNode = this.tree.AddNode("Actor", this.nodeShapes);
                RefreshActorNodeIcons(actor, actorNode);
                this.nodeToActor.Add(actorNode, actor);
                this.actorToNode.Add(actor, actorNode);
                this.actorToParams.Add(actor, new NodeWidgets());

                if(actor == this.mgr.Selected)
                { 
                    ++this.recurseGuard;
                    actorNode.Selected = true;
                    --this.recurseGuard;
                }
            }

            foreach (SceneActor actor in this.mgr.waveScene.Actors)
                this.RefreshActorParams(actor);
        }
        else if(this.mgr.Selected != null)
        {
            CreateActorTreeNode(this.mgr.Selected);
        }
    }

    protected void RefreshActorNodeIcons(SceneActor actor, PxPre.Tree.Node actorNode)
    {
        actorNode.SetIcon(
            "enabled", 
            true,
            actor.enabled.BoolVal ? this.icotogOn : this.icotogOff, 
            (x, y, z) => 
            {
                actor.enabled.BoolVal = !actor.enabled.BoolVal;
                this.mgr.NotifyActorModified(actor, actor.enabled);
            });

        int fill = actor.fillMode.IntVal;
        switch(actor.shape.IntVal)
        {
            case (int)SceneActor.Shape.Ellipse:
                if(fill == (int)SceneActor.Fill.Filled)
                    actorNode.SetIcon("icon", true, this.tyicoEllipseFilled, (x, y, z) => { });
                else
                    actorNode.SetIcon("icon", true, this.tyicoEllipseHollow, (x, y, z) => { });
                break;

            case (int)SceneActor.Shape.Square:
                if (fill == (int)SceneActor.Fill.Filled)
                    actorNode.SetIcon("icon", true, this.tyicoRectFilled, (x, y, z) => { });
                else
                    actorNode.SetIcon("icon", true, this.tyicoRectHollow, (x, y, z) => { });
                break;
        }
    }

    void RefreshActorParams(SceneActor actor)
    {
        PxPre.Tree.Node n;
        if(this.actorToNode.TryGetValue(actor, out n) == false)
            return;

        // We are going to re-add items from scratch
        n.DestroyChildren();

        NodeWidgets nw;
        this.actorToParams.TryGetValue(actor, out nw);
        HashSet<string> foundParams = new HashSet<string>(nw.widgets.Keys);

        foreach(EditValue ev in actor.EnumerateRelevantParams())
        { 
            PxPre.Tree.Node np = this.tree.AddNode(ev.name, n);
            foundParams.Remove(ev.name);

            NodeWidgets.ParamNodePair pnp;
            if(nw.widgets.TryGetValue(ev.name, out pnp) == true)
            { 
                pnp.node = np;
                np.MinHeight = pnp.param.rectTransform.sizeDelta.y;
                nw.widgets[ev.name] = pnp;
            }
            else
            {
                pnp = new NodeWidgets.ParamNodePair();
                pnp.node = np;

                GameObject goPrefab = null;
                if (ev.name == "Rotation")
                {
                    goPrefab = GameObject.Instantiate(this.prefabWRotation);
                }
                else if(ev.val.ty == Val.Type.Float)
                {
                    goPrefab = GameObject.Instantiate(this.prefabWSpinner);
                }
                else if(ev.val.ty == Val.Type.Int)
                {
                    goPrefab = GameObject.Instantiate(this.prefabWSpinner);
                }
                else if(ev.val.ty == Val.Type.Bool)
                {
                    goPrefab = GameObject.Instantiate(this.prefabWCheckbox);
                }
                else if(ev.val.ty == Val.Type.Enum)
                {
                    goPrefab = GameObject.Instantiate(this.prefabWPulldown);
                }

                if(goPrefab != null)
                { 
                    ValueEditor_Base veb = goPrefab.GetComponent<ValueEditor_Base>();
                    veb.transform.SetParent(this.optionScroll.content, false);
                    veb.Init(this.mgr, actor, ev);

                    np.MinHeight = goPrefab.GetComponent<RectTransform>().sizeDelta.y;

                    pnp.param = veb;
                    nw.widgets.Add(ev.name, pnp);
                }

            }
        }

        // Remove anything we originally had that's no longer needed.
        foreach(string str in foundParams)
        { 
            GameObject.Destroy(nw.widgets[str].param.gameObject);
            nw.widgets.Remove(str);
        }
    }

    void RefreshParamPositions(SceneActor actor)
    { 
        PxPre.Tree.Node node;
        if(this.actorToNode.TryGetValue(actor, out node) == false)
            return;

        NodeWidgets nw;
        if(this.actorToParams.TryGetValue(actor, out nw) == false)
            return;

        if(node.IsVisible() == false || node.Expanded == false)
        { 
            foreach(KeyValuePair<string, NodeWidgets.ParamNodePair> kvp in nw.widgets)
                kvp.Value.param.gameObject.SetActive(false);
        }
        else
        {
            foreach (KeyValuePair<string, NodeWidgets.ParamNodePair> kvp in nw.widgets)
            {
                RectTransform rt = this.tree.GetNodeRectTransform(kvp.Value.node);
                if(rt == null)
                {
                    kvp.Value.param.gameObject.SetActive(false);
                    continue;
                }

                kvp.Value.param.gameObject.SetActive(true);
                RectTransform rtParam = kvp.Value.param.rectTransform;

                const float horizPadd = 5.0f;
                rtParam.anchorMin = new Vector2(0.0f, 1.0f);
                rtParam.anchorMax = new Vector2(1.0f, 1.0f);
                rtParam.offsetMin = new Vector2(horizPadd, Mathf.Min(rt.offsetMin.y, rt.offsetMax.y - kvp.Value.node.MinHeight));
                rtParam.offsetMax = new Vector2(-horizPadd, rt.offsetMax.y);
            }
        }
    }

    public override void OnActorDeleted(SceneActor actor)
    { 
        if(this.actorToNode.TryGetValue(actor, out PxPre.Tree.Node n) == true)
        {
            this.actorToNode.Remove(actor);
            this.nodeToActor.Remove(n);
            n.Destroy();

            this.actorToParams[actor].Destroy();
            this.actorToParams.Remove(actor);
        }


    }

    public override void OnActorModified(SceneActor actor, string paramName)
    {
        if(actor == null)
        { 
            if(paramName == "Decay")
                this.decayEd.OnUpdateValue();

            return;
        }

        NodeWidgets nw;
        if(this.actorToParams.TryGetValue(actor, out nw) == false)
            return;

        NodeWidgets.ParamNodePair pnp;
        if(nw.widgets.TryGetValue(paramName, out pnp) == false)
            return;

        pnp.param.OnUpdateValue();

        EditValue ? ev = actor.GetParam(paramName);
        if(ev.HasValue)
        { 
            if(
                ev.Value.val.ty == Val.Type.Enum ||
                ev.Value.val.ty == Val.Type.Bool)
            {
                RefreshActorParams(actor);

                if(this.actorToNode.TryGetValue(actor, out PxPre.Tree.Node v) == true)
                    RefreshActorNodeIcons(actor, v);
            }
        }
    }

    public override void OnActorSelected(SceneActor actor)
    { 
        if(this.showAllToggle.isOn == false)
            this.RebuildActorTree();
        else if(actor != null)
        {
            if(this.actorToNode.TryGetValue(actor, out PxPre.Tree.Node v) == true)
            { 
                this.tree.SelectNode(v, true);
            }
        }
        else
        { 
            this.tree.DeselectAll();
        }
    }

    public override void OnLoaded()
    {
        this.RebuildActorTree();
    }

    public override void OnCleared()
    {
        this.RebuildActorTree();
    }

    public void OnToggle_ViewAll()
    { 
        this.RebuildActorTree();
    }

    ////////////////////////////////////////////////////////////////////////////////
    //
    //      INTERFACE : ITreeHandler
    //
    ////////////////////////////////////////////////////////////////////////////////

    void PxPre.Tree.ITreeHandler.OnNodeDelete(PxPre.Tree.Tree tree, PxPre.Tree.Node node)
    {
    }

    void PxPre.Tree.ITreeHandler.OnNodeAdd(PxPre.Tree.Tree tree, PxPre.Tree.Node node)
    {
    }

    void PxPre.Tree.ITreeHandler.OnNodeExpanded(PxPre.Tree.Tree tree, PxPre.Tree.Node node, bool expanded)
    {
    }

    int recurseGuard = 0;
    void PxPre.Tree.ITreeHandler.OnNodeSelected(PxPre.Tree.Tree tree, PxPre.Tree.Node node, bool selected)
    {
        if(this.showAllToggle.isOn == false)
            return;

        if(this.recurseGuard > 0)
            return;

        ++this.recurseGuard;

        for(PxPre.Tree.Node it = node; it != null; it = it.Parent)
        {
            if(nodeToActor.TryGetValue(it, out SceneActor actor) == true)
            { 
                this.mgr.SelectActor(actor);
                --this.recurseGuard;
                return;
            }
        }

        this.mgr.SelectActor(null);

        --this.recurseGuard;
    }

    void PxPre.Tree.ITreeHandler.OnNodeClicked(PxPre.Tree.Tree tree, PxPre.Tree.Node node)
    {
    }

    void PxPre.Tree.ITreeHandler.OnTreeLayout(PxPre.Tree.Tree tree, PxPre.Tree.Node.DirtyItems allIssues, Vector2 size)
    {
        this.optionScroll.content.sizeDelta = 
            new Vector2(
                this.optionScroll.content.sizeDelta.x,
                size.y);

        foreach(KeyValuePair<SceneActor, PxPre.Tree.Node> kvp in this.actorToNode)
            this.RefreshParamPositions(kvp.Key);

        if(this.nodeWorld.Expanded == false)
        {
            foreach(KeyValuePair< PxPre.Tree.Node, ValueEditor_Base> kvp in this.environmentItems)
                kvp.Value.gameObject.SetActive(false);
        }
        else
        {
            foreach (KeyValuePair<PxPre.Tree.Node, ValueEditor_Base> kvp in this.environmentItems)
            {
                kvp.Value.gameObject.SetActive(true);
                RectTransform rtParam = kvp.Value.rectTransform;

                RectTransform rt = this.tree.GetNodeRectTransform(kvp.Key);

                const float horizPadd = 5.0f;
                rtParam.anchorMin = new Vector2(0.0f, 1.0f);
                rtParam.anchorMax = new Vector2(1.0f, 1.0f);
                rtParam.offsetMin = new Vector2(horizPadd, Mathf.Min(rt.offsetMin.y, rt.offsetMax.y - kvp.Key.MinHeight));
                rtParam.offsetMax = new Vector2(-horizPadd, rt.offsetMax.y);
            }
        }
    }
}
