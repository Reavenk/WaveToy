using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre.Tooltip
{
    public class TextTooltipMgr : 
        MonoBehaviour,
        ITooltipMgr
    {
        public string defaultValue = "";
        public UnityEngine.UI.Text tipOutput;

        ITooltip currentTip = null;

        void Update()
        {
            if(this.currentTip != null && this.currentTip.isDynamic == true)
                tipOutput.text = this.currentTip.Tip;
        }

        void Awake()
        { 
            TooltipMgr.RegisterManager(this);
            this.tipOutput.text = this.defaultValue;
        }

        void ITooltipMgr.SetTooltip(ITooltip tip)
        { 
            this.currentTip = tip;
            this.tipOutput.text = tip.Tip;
        }

        void ITooltipMgr.UnsetTooltip(ITooltip tip)
        { 
            if(this.currentTip == tip)
                ((ITooltipMgr)this).UnsetTooltip();
        }

        void ITooltipMgr.UnsetTooltip()
        { 
            this.currentTip = null;
            this.tipOutput.text = this.defaultValue;
        }

    }
}