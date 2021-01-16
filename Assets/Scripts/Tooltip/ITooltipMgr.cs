using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre.Tooltip
{
    public interface ITooltipMgr
    {
        void SetTooltip(ITooltip tip);
        void UnsetTooltip(ITooltip tip);
        void UnsetTooltip();
    }

    public static class TooltipMgr
    {
        static ITooltipMgr manager;
        public static ITooltipMgr Manager {get=>manager; }

        public static void RegisterManager(ITooltipMgr mgr)
        { 
            manager = mgr;
        }
    }
}
