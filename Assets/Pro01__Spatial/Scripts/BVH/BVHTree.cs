using UnityEngine;
using System.Collections.Generic;

class BVHNode
{
    public BoundingBox box;
    public BVHNode left;
    public BVHNode right;
    public List<GameObject> objects;

    public BVHNode(BoundingBox bBox)
    {
        box = bBox;
        objects = new List<GameObject>();
        left = null;
        right = null;
    }

    public void DebugDraw()
    {
        Gizmos.DrawLine(new Vector3(box.min.x, 0, box.min.z), new Vector3(box.max.x, 0, box.min.z));
        Gizmos.DrawLine(new Vector3(box.max.x, 0, box.min.z), new Vector3(box.max.x, 0, box.max.z));
        Gizmos.DrawLine(new Vector3(box.max.x, 0, box.max.z), new Vector3(box.min.x, 0, box.max.z));
        Gizmos.DrawLine(new Vector3(box.min.x, 0, box.max.z), new Vector3(box.min.x, 0, box.min.z));

        if (left != null)
        {
            left.DebugDraw();
        }

        if (right != null) {
            right.DebugDraw();
        }
    }
}

class BVHTree : ISpace
{
    public BVHNode root;

    public override void Build(List<GameObject> objects)
    {
        objects.Sort((a, b) => {
            if (a.transform.position.x != b.transform.position.x)
            {
                return a.transform.position.x.CompareTo(b.transform.position.x);
            }
            return a.transform.position.z.CompareTo(b.transform.position.z);
        });
        root = BuildRecursive(objects, 0, objects.Count - 1);
    }

    public BVHNode BuildRecursive(List<GameObject> objects, int start, int end)
    {
        if(start > end)
        {
            return null;
        }

        BVHNode node = new BVHNode(GetBoundingBox(objects, start, end));

        if (start == end)
        {
            node.objects.Add(objects[start]);
            return node;
        }
        
        var mid = (start + end) / 2;

        node.left = BuildRecursive(objects, start, mid);
        node.right = BuildRecursive(objects, mid + 1, end);
        return node;
    }

    private BoundingBox GetBoundingBox(List<GameObject> objects, int start, int end)
    {
        if(start > end)
        {
            return new BoundingBox(Vector3.zero, Vector3.zero);
        }

        List<BoundingBox> boxes = new List<BoundingBox>();
        for (int i = start; i <= end; i++)
        {
            var min = objects[i].transform.position - Vector3.one * 0.5f;
            var max = objects[i].transform.position + Vector3.one * 0.5f;
            var tmpBox = new BoundingBox(min, max);
            boxes.Add(tmpBox);
        }

        BoundingBox box = BoundingBox.CombineBoxList(boxes);
        return box;
    }

    public override List<GameObject> GetGameObjectsByNear(Rect range)
    {
        return QueryRecursive(root, range);
    }

    private List<GameObject> QueryRecursive(BVHNode node, Rect range)
    {
        List<GameObject> result = new List<GameObject>();

        if (node == null)
        {
            return result;
        }

        var box = new BoundingBox(new Vector3(range.xMin, 0, range.yMin), new Vector3(range.xMax, 0, range.yMax));
        if (node.box.Intersect(box))
        {
            if (node.left == null && node.right == null)
            {
                result.AddRange(node.objects);
            }
            else
            {
                result.AddRange(QueryRecursive(node.left, range));
                result.AddRange(QueryRecursive(node.right, range));
            }
        }

        return result;
    }

    public override void DebugDraw()
    {
        root.DebugDraw();
    }
}