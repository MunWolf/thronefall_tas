using UnityEngine;

namespace ThronefallTAS;

public static class Utils
{
    public static string GetPath(Transform tr)
    {
        Transform parent = tr.parent;
        return parent == null ? tr.name : GetPath(parent) + "/" + tr.name;
    }

    public static bool UnityNullCheck(object a)
    {
        return a != null && (!(a is UnityEngine.Object o) || o != null);
    }
}