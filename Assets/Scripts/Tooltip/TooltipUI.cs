using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre.Tooltip
{
    public class TooltipUI : 
        MonoBehaviour,
        ITooltip,
        UnityEngine.EventSystems.IPointerEnterHandler,
        UnityEngine.EventSystems.IPointerExitHandler

    {
        public string tip = "";

        bool ITooltip.isDynamic { get => false; }
        string ITooltip.Tip { get => this.tip; }

        void UnityEngine.EventSystems.IPointerEnterHandler.OnPointerEnter(UnityEngine.EventSystems.PointerEventData eventData)
        { 
            if(TooltipMgr.Manager != null)
                TooltipMgr.Manager.SetTooltip(this);
        }

        void UnityEngine.EventSystems.IPointerExitHandler.OnPointerExit(UnityEngine.EventSystems.PointerEventData eventData)
        {
            if (TooltipMgr.Manager != null)
                TooltipMgr.Manager.UnsetTooltip(this);
        }
    }
}