using UnityEngine;
using System.Collections.Generic;

class BoundingBox
{
    public Vector3 min;
    public Vector3 max;

    public BoundingBox(Vector3 min, Vector3 max)
    {
        this.min = min;
        this.max = max;
    }

    public bool Intersect(BoundingBox other)
    {
        if (min.x > other.max.x || max.x < other.min.x)
        {
            return false;
        }

        if (min.y > other.max.y || max.y < other.min.y)
        {
            return false;
        }

        if (min.z > other.max.z || max.z < other.min.z)
        {
            return false;
        }

        return true;
    }

    public static BoundingBox Combine(BoundingBox a, BoundingBox b)
    {
        Vector3 min = new Vector3(Mathf.Min(a.min.x, b.min.x), Mathf.Min(a.min.y, b.min.y), Mathf.Min(a.min.z, b.min.z));
        Vector3 max = new Vector3(Mathf.Max(a.max.x, b.max.x), Mathf.Max(a.max.y, b.max.y), Mathf.Max(a.max.z, b.max.z));
        return new BoundingBox(min, max);
    }

    public bool Contains(Vector3 point)
    {
        return point.x >= min.x && point.x <= max.x &&
               point.y >= min.y && point.y <= max.y &&
               point.z >= min.z && point.z <= max.z;
    }

    public bool Contains(BoundingBox other)
    {
        return min.x <= other.min.x && max.x >= other.max.x &&
               min.y <= other.min.y && max.y >= other.max.y &&
               min.z <= other.min.z && max.z >= other.max.z;
    }

    public static BoundingBox CombineBoxList(List<BoundingBox> boxes)
    {
        Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

        foreach (BoundingBox box in boxes)
        {
            min = new Vector3(Mathf.Min(min.x, box.min.x), Mathf.Min(min.y, box.min.y), Mathf.Min(min.z, box.min.z));
            max = new Vector3(Mathf.Max(max.x, box.max.x), Mathf.Max(max.y, box.max.y), Mathf.Max(max.z, box.max.z));
        }

        return new BoundingBox(min, max);
    }
}
