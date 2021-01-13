using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pane_Properties : 
    Pane_Base,
    PxPre.Tree.ITreeHandler
{
    public PxPre.Tree.Tree tree;

    public UnityEngine.UI.ScrollRect treeScroll;
    public UnityEngine.UI.ScrollRect optionScroll;

    public bool treesSyncRecursionGuard = false;

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

        this.tree.AddNode("Hello", null);
        this.tree.AddNode("Jello", null);
        this.tree.AddNode("Man", null);


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
    }
}
