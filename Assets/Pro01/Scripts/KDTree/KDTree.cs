using System.Collections.Generic;
using UnityEngine;

class KDTreeNode
{
    public Vector3 point;
    public KDTreeNode left;
    public KDTreeNode right;
    public KDTreeNode parent;
    public GameObject obj;

    public KDTreeNode(GameObject go)
    {
        obj = go;
        point = go.transform.position;
        left = null;
        right = null;
    }
}

class KDTree : ISpace
{
    private KDTreeNode root;
    private int dimension = 2;

    public override void Build(List<GameObject> objects)
    {
        root = BuildTree(objects, 0);
    }

    private KDTreeNode BuildTree(List<GameObject> objects, int depth)
    {
        if (objects.Count == 0)
        {
            return null;
        }
        Sort(objects, depth);
        var mid = objects.Count / 2;
        KDTreeNode node = new KDTreeNode(objects[mid]);

        node.left = BuildTree(objects.GetRange(0, mid), depth + 1);
        node.right = BuildTree(objects.GetRange(mid + 1, objects.Count - mid - 1), depth + 1);
        return node;
    }

    private void Sort(List<GameObject> objects, int depth)
    {
        int kd = depth % dimension;
        objects.Sort((a, b) =>
        {
            var a_pos = a.transform.position;
            var b_pos = b.transform.position;

            if (kd == 0)
            {
                return a_pos.x.CompareTo(b_pos.x);
            }
            else
            {
                return a_pos.z.CompareTo(b_pos.z);
            }
        });
    }

    public override void DebugDraw()
    {

    }

    public override List<GameObject> GetGameObjectsByNear(Rect range)
    {
        return Query(root, range, 0);
    }

    private List<GameObject> Query(KDTreeNode node, Rect range, int depth)
    {
        List<GameObject> result = new List<GameObject>();

        if (node == null)
        {
            return result;
        }

        int kd = depth % dimension;
        var pos = node.point;

        if (range.Contains(new Vector2(pos.x, pos.z)))
        {
            result.Add(node.obj);
        }

        if (kd == 0)
        {
            if (range.xMin < pos.x)
            {
                result.AddRange(Query(node.left, range, depth + 1));
            }
            if (range.xMax >= pos.x)
            {
                result.AddRange(Query(node.right, range, depth + 1));
            }
        }
        else
        {
            if (range.yMin < pos.z)
            {
                result.AddRange(Query(node.left, range, depth + 1));
            }
            if (range.yMax >= pos.z)
            {
                result.AddRange(Query(node.right, range, depth + 1));
            }
        }

        return result;
    }
}
