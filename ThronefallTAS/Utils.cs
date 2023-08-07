using UnityEngine;

namespace ThronefallTAS;

public static class Utils
{
    public static string GetPath(Transform tr)
    {
        var parent = tr.parent;
        return parent == null ? tr.name : GetPath(parent) + "/" + tr.name;
    }

    public static bool UnityNullCheck(object a)
    {
        return a != null && (a is not Object o || o != null);
    }
}