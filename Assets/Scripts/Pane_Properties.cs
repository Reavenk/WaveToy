using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public bool treesSyncRecursionGuard = false;

    PxPre.Tree.Node nodeWorld;
    PxPre.Tree.Node nodeShapes;

    public GameObject prefabWCheckbox;
    public GameObject prefabWPulldown;
    public GameObject prefabWSpinner;
    public GameObject prefabWRotation;

    Dictionary<SceneActor, PxPre.Tree.Node> actorToNode = new Dictionary<SceneActor, PxPre.Tree.Node>();
    Dictionary<PxPre.Tree.Node, SceneActor> nodeToActor = new Dictionary<PxPre.Tree.Node, SceneActor>();
    Dictionary<SceneActor, NodeWidgets> actorToParams = new Dictionary<SceneActor, NodeWidgets>();


    private void Start()
    {
        this.treeScroll.onValueChanged.AddListener(
            (x)=>
            { 
                if(this.treesSyncRecursionGuard == true)
                    return;

                this.treesSyncRecursionGuard = true;

                this.optionScroll.verticalNormalizedPosition =
                    this.treeScroll.verticalNormalizedPosition;

                this.treesSyncRecursionGuard = false;
            });

        this.optionScroll.onValueChanged.AddListener(
            (x)=>
            { 
                if(this.treesSyncRecursionGuard == true)
                    return;

                this.treesSyncRecursionGuard = true;

                this.treeScroll.verticalNormalizedPosition = 
                    this.optionScroll.verticalNormalizedPosition;

                this.treesSyncRecursionGuard = false;
            });

        this.tree.subscribers.Add(this);

        this.nodeWorld = this.tree.AddNode("Environment", null);
        this.tree.AddNode("Decay", this.nodeWorld);

        this.nodeShapes = this.tree.AddNode("Shapes", null);
    }

    public override void OnActorAdded(SceneActor actor)
    { 
        PxPre.Tree.Node actorNode = this.tree.AddNode("Actor", this.nodeShapes);
        this.nodeToActor.Add(actorNode, actor);
        this.actorToNode.Add(actor, actorNode);
        this.actorToParams.Add(actor, new NodeWidgets());

        this.RefreshActorParams(actor);

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
                else if(ev.val.ty == ValueBase.Type.Float)
                {
                    goPrefab = GameObject.Instantiate(this.prefabWSpinner);
                }
                else if(ev.val.ty == ValueBase.Type.Int)
                {
                    goPrefab = GameObject.Instantiate(this.prefabWSpinner);
                }
                else if(ev.val.ty == ValueBase.Type.Bool)
                {
                    goPrefab = GameObject.Instantiate(this.prefabWCheckbox);
                }
                else if(ev.val.ty == ValueBase.Type.Enum)
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

        if(node.Expanded == false)
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

                rtParam.anchorMin = new Vector2(0.0f, 1.0f);
                rtParam.anchorMax = new Vector2(1.0f, 1.0f);
                rtParam.offsetMin = new Vector2(0.0f, Mathf.Min(rt.offsetMin.y, rt.offsetMax.y - kvp.Value.node.MinHeight));
                rtParam.offsetMax = new Vector2(0.0f, rt.offsetMax.y);
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
                ev.Value.val.ty == ValueBase.Type.Enum ||
                ev.Value.val.ty == ValueBase.Type.Bool)
            {
                RefreshActorParams(actor);
            }
        }
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

    void PxPre.Tree.ITreeHandler.OnNodeSelected(PxPre.Tree.Tree tree, PxPre.Tree.Node node, bool selected)
    {
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
    }
}
