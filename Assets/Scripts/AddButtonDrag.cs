using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddButtonDrag : UnityEngine.EventSystems.EventTrigger
{
    public Main manager;
    public RectTransform icon;

    Vector3 originalIconLocalPos;
    Vector3 draggOffset;

    public UnityEngine.UI.Button button;

    private void Awake()
    {
        this.originalIconLocalPos = this.icon.localPosition;
    }

    public override void OnBeginDrag(UnityEngine.EventSystems.PointerEventData eventData)
    {
        base.OnBeginDrag(eventData);
        this.manager.DefferedAddDrag_OnBeginDrag(eventData);

        this.draggOffset = 
            this.icon.worldToLocalMatrix.MultiplyPoint(eventData.position);

        this.button.enabled = false;
        this.button.enabled = true;
    }

    public override void OnEndDrag(UnityEngine.EventSystems.PointerEventData eventData)
    {
        base.OnEndDrag(eventData);
        this.manager.DefferedAddDrag_OnEndDrag(eventData);

        this.icon.localPosition = 
            this.originalIconLocalPos;
    }

    public override void OnDrag(UnityEngine.EventSystems.PointerEventData eventData)
    {
        base.OnDrag(eventData);
        this.manager.DefferedAddDrag_OnDrag(eventData);

        Vector3 diff = this.icon.localToWorldMatrix.MultiplyPoint(this.draggOffset);
        this.icon.position += ((Vector3)eventData.position - diff);
    }
}
