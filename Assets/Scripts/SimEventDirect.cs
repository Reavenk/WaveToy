using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SimEventDirect : UnityEngine.EventSystems.EventTrigger
{
    public Main manager;

    public override void OnBeginDrag(PointerEventData eventData)
    {
        base.OnBeginDrag(eventData);
        this.manager.OnSimEvent_OnBeginDrag(eventData);
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);
        this.manager.OnSimEvent_OnEndDrag(eventData);
    }

    public override void OnDrag(PointerEventData eventData)
    {
        base.OnDrag(eventData);
        this.manager.OnSimEvent_OnDrag(eventData);
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        this.manager.OnSimEvent_OnPointerDown(eventData);
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        this.manager.OnSimEvent_OnPointerUp(eventData);
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        this.manager.OnSimEvent_OnPointerEnter(eventData);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        this.manager.OnSimEvent_OnPointerExit(eventData);
    }

    public override void OnScroll(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        this.manager.OnSimEvent_OnScroll(eventData);
    }
}
