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

using PxPre.Datum;

/// <summary>
/// The docked properties window that shows the scene tree and editing widgets 
/// of actors in the scene.
/// </summary>
public class Pane_Properties : 
    Pane_Base,
    PxPre.Tree.ITreeHandler
{
    /// <summary>
    /// A registry of a node in the tree and references to the current widgets
    /// created for it.
    /// </summary>
    public class NodeWidgets
    {
        /// <summary>
        /// A pairing of a tree node for an actor parameter and its instanced widget.
        /// </summary>
        public struct ParamNodePair
        {
            public PxPre.Tree.Node node;
            public ValueEditor_Base param;
        }

        /// <summary>
        /// A lookup of parameter widgets given a parameter name.
        /// </summary>
        public Dictionary<string, ParamNodePair> widgets =
            new Dictionary<string, ParamNodePair>();

        /// <summary>
        /// Destroy all the widgets referenced in the object.
        /// </summary>
        public void Destroy()
        {
            foreach (KeyValuePair<string, ParamNodePair> kvp in this.widgets)
                GameObject.Destroy(kvp.Value.param.gameObject);

            this.widgets.Clear();
        }
    }

    /// <summary>
    /// The tree control for actors and properties.
    /// </summary>
    public PxPre.Tree.Tree tree;

    /// <summary>
    /// The ScrollRect containing the tree.
    /// </summary>
    public UnityEngine.UI.ScrollRect treeScroll;

    /// <summary>
    /// The ScrollRect containing the options.
    /// </summary>
    public UnityEngine.UI.ScrollRect optionScroll;

    /// <summary>
    /// A semaphore to avoid recursive callbacks that could lead to a stack overflow.
    /// </summary>
    public int treesSyncRecursionGuard = 0;

    /// <summary>
    /// The node in the tree for the world properties.
    /// </summary>
    PxPre.Tree.Node nodeWorld;

    /// <summary>
    /// The node in the tree for the decay property.
    /// </summary>
    PxPre.Tree.Node nodeDecay;

    /// <summary>
    /// The node in the tree containing the actors.
    /// </summary>
    PxPre.Tree.Node nodeShapes;
    
    /// <summary>
    /// The prefab for the bool checkbox.
    /// </summary>
    public GameObject prefabWCheckbox;

    /// <summary>
    /// The prefab for the enum pulldown widgets.
    /// </summary>
    public GameObject prefabWPulldown;

    /// <summary>
    /// The prefab for the spinner widgets.
    /// </summary>
    public GameObject prefabWSpinner;

    /// <summary>
    /// The prefab for rotation widgets.
    /// </summary>
    public GameObject prefabWRotation;

    /// <summary>
    /// The prefab for reset-able spinner widgets.
    /// </summary>
    public GameObject prefabSpinnerReset;

    /// <summary>
    /// The icon for actors in the tree that are filled fllipses.
    /// </summary>
    public Sprite tyicoEllipseFilled;

    /// <summary>
    /// The icon for actors in the tree that are hollow ellipses.
    /// </summary>
    public Sprite tyicoEllipseHollow;

    /// <summary>
    /// The icon for actors in the tree that are filled rectangles.
    /// </summary>
    public Sprite tyicoRectFilled;

    /// <summary>
    /// The icon for actors in the tree that are hollow rectangles.
    /// </summary>
    public Sprite tyicoRectHollow;

    /// <summary>
    /// The icon for actors in the tree that are toggled on.
    /// </summary>
    public Sprite icotogOn;

    /// <summary>
    /// The icon for actors in the tree that are toggled off.
    /// </summary>
    public Sprite icotogOff;

    /// <summary>
    /// A lookup of tree nodes given an actor. This container should be synced with nodeToActor and
    /// actorToParams.
    /// </summary>
    Dictionary<SceneActor, PxPre.Tree.Node> actorToNode = new Dictionary<SceneActor, PxPre.Tree.Node>();

    /// <summary>
    /// A lookup of actors given a tree node. This container should be synced with actorToNode and 
    /// actorToParams.
    /// </summary>
    Dictionary<PxPre.Tree.Node, SceneActor> nodeToActor = new Dictionary<PxPre.Tree.Node, SceneActor>();

    /// <summary>
    /// A lookup of NodeWidgets given a scene actor. This container should be synced with actorToNode
    /// and nodeToActor.
    /// </summary>
    Dictionary<SceneActor, NodeWidgets> actorToParams = new Dictionary<SceneActor, NodeWidgets>();

    /// <summary>
    /// A lookup of non-actor parameter widgets given a tree node.
    /// </summary>
    /// <remarks>As of writing this comment, only the Decay parameter is managed.</remarks>
    Dictionary<PxPre.Tree.Node, ValueEditor_Base> environmentItems = new Dictionary<PxPre.Tree.Node, ValueEditor_Base>();

    /// <summary>
    /// The editor widget for the environmeny's decay.
    /// </summary>
    ValueEditor_Base decayEd;

    /// <summary>
    /// Toggle to select if all nodes should be in the tree, or if only the
    /// selected node should be displayed.
    /// </summary>
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

        this.tree.LayoutTree();
    }

    /// <summary>
    /// Utility coroutine function used to sync the two ScrollRects.
    /// </summary>
    /// <param name="sr">The ScrollRect that was modified that invoked the sync.</param>
    /// <param name="normPos">The new normalized position to sync to.</param>
    /// <returns>Coroutine.</returns>
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

    /// <summary>
    /// Create an tree node for an actor and populate it with the correct
    /// contents.
    /// </summary>
    /// <param name="actor">The actor to add to the tree.</param>
    public void CreateActorTreeNode(SceneActor actor)
    {
        PxPre.Tree.Node actorNode = this.tree.AddNode("Actor", this.nodeShapes);
        RefreshActorNodeIcons(actor, actorNode);
        this.nodeToActor.Add(actorNode, actor);
        this.actorToNode.Add(actor, actorNode);
        this.actorToParams.Add(actor, new NodeWidgets());
        this.RefreshActorParams(actor);
    }

    /// <summary>
    /// Rebuild the entire tree.
    /// </summary>
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

    /// <summary>
    /// Refresh the icons for an actor's tree node.
    /// </summary>
    /// <param name="actor">The actor to refresh for.</param>
    /// <param name="actorNode">The actor's tree node.</param>
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

    /// <summary>
    /// For a specified actor, rebuild its tree of parameters to make sure
    /// they are up to date.
    /// </summary>
    /// <param name="actor">The actor to rebuilt the subtree for.</param>
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
                    // Rotation is an edge case where the parameter gets its 
                    // own specific widget.
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

    /// <summary>
    /// Makes sure the parameters shown for a specific actor is correct.
    /// </summary>
    /// <param name="actor">The actor to refresh for.</param>
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
