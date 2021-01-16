using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre.Tooltip
{
    public interface ITooltip
    {
        bool isDynamic {get; }
        string Tip {get; }
    }
}