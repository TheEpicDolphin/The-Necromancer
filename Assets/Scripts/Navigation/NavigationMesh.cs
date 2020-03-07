using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class HeapElement<TKey, TValue> where TKey : IComparable
{

    public TKey key;
    public TValue value;

    public HeapElement(TKey key, TValue value)
    {
        this.key = key;
        this.value = value;
    }

}
public class MinHeap<TKey, TValue> where TKey : IComparable
{
    public List<HeapElement<TKey, TValue>> heap;
    public Dictionary<TValue, int> indexMap;

    //Assumes that it is already heapified
    public MinHeap()
    {
        this.heap = new List<HeapElement<TKey, TValue>>();
        this.indexMap = new Dictionary<TValue, int>();
    }

    public HeapElement<TKey, TValue> ExtractMin()
    {
        if (heap.Count == 0)
        {
            return null;
        }
        else if (heap.Count == 1)
        {
            HeapElement<TKey, TValue> min = heap[0];
            heap.RemoveAt(0);
            indexMap.Remove(min.value);
            return min;
        }
        else
        {
            int n = heap.Count;
            HeapElement<TKey, TValue> min = heap[0];
            indexMap[heap[n - 1].value] = 0;
            heap[0] = heap[n - 1];
            heap.RemoveAt(n - 1);
            indexMap.Remove(min.value);
            MinHeapifyDown(0);
            return min;
        }


    }

    public void Insert(TKey key, TValue value)
    {
        heap.Add(new HeapElement<TKey, TValue>(key, value));
        indexMap[value] = heap.Count - 1;
        MinHeapifyUp(heap.Count - 1);
    }

    public void Update(TKey key, TValue value)
    {
        int i = indexMap[value];
        if (heap[i].key.CompareTo(key) < 0)
        {
            heap[i].key = key;
            MinHeapifyDown(i);
        }
        else
        {
            heap[i].key = key;
            MinHeapifyUp(i);
        }

    }

    private void MinHeapifyUp(int i)
    {
        int p = Parent(i);

        if (heap[i].key.CompareTo(heap[p].key) < 0)
        {
            indexMap[heap[p].value] = i;
            indexMap[heap[i].value] = p;
            HeapElement<TKey, TValue> temp = heap[p];
            heap[p] = heap[i];
            heap[i] = temp;
            MinHeapifyUp(p);
        }
    }

    private void MinHeapifyDown(int i)
    {
        int l = Left(i);
        int r = Right(i);
        int c = heap[r].key.CompareTo(heap[l].key) < 0 ? r : l;

        if (heap[c].key.CompareTo(heap[i].key) < 0)
        {
            indexMap[heap[c].value] = i;
            indexMap[heap[i].value] = c;
            HeapElement<TKey, TValue> temp = heap[c];
            heap[c] = heap[i];
            heap[i] = temp;
            MinHeapifyDown(c);
        }

    }

    private int Parent(int i)
    {
        int p = (i - 1) / 2;
        return i > 0 ? p : i;
    }

    private int Left(int i)
    {
        int l = 2 * i + 1;
        return l < heap.Count ? l : i;
    }

    private int Right(int i)
    {
        int r = 2 * i + 2;
        return r < heap.Count ? r : i;
    }


}

public class LineSegment
{
    public Vector2 p1;
    public Vector2 p2;
    public Vector2 Dir
    {
        get
        {
            return (p2 - p1).normalized;
        }
    }

    public LineSegment(Vector2 p1, Vector2 p2)
    {
        this.p1 = p1;
        this.p2 = p2;
    }

    public LineSegment Intersect(HalfPlane halfPlane)
    {
        Vector2 n = Vector2.Perpendicular(Dir);
        float D = halfPlane.n.x * n.y - halfPlane.n.y * n.x;
        float Dx = Vector2.Dot(halfPlane.n, halfPlane.p) * n.y - halfPlane.n.y * Vector2.Dot(n, this.p1);
        float Dy = halfPlane.n.x * Vector2.Dot(n, this.p1) - Vector2.Dot(halfPlane.n, halfPlane.p) * n.x;

        Vector2 intersection = new Vector2(Dx / D, Dy / D);

        //Check if intersection happens within bounds of line segment. If line segment is unbounded at one end (this.next is null), 
        //then we simply create a dummy end point that is far down the direction of the line segment.

        if (Vector2.Dot(intersection - this.p1, intersection - this.p2) <= 1e-5f)
        {
            return new LineSegment(intersection, -Vector2.Perpendicular(halfPlane.n));
        }

        return null;
    }

    /*
     *  Returns intersection point of two LineSegments 
     * 
     */
    public Vector2 Intersect(LineSegment lineSegment)
    {
        Vector2 n1 = Vector2.Perpendicular(Dir);
        Vector2 n2 = Vector2.Perpendicular(lineSegment.Dir);
        float D = n2.x * n1.y - n2.y * n1.x;
        float Dx = Vector2.Dot(n2, lineSegment.p1) * n1.y - n2.y * Vector2.Dot(n1, this.p1);
        float Dy = n2.x * Vector2.Dot(n1, this.p1) - Vector2.Dot(n2, lineSegment.p1) * n1.x;

        Vector2 intersection = new Vector2(Dx / D, Dy / D);

        //TODO: Check if intersection is past any of the bounds

        return intersection;
    }

}

public class BoundaryEdge
{
    public Vector2 v1;
    public Vector2 v2;
    public Vector2 n;

    public BoundaryEdge(Vector2 v1, Vector2 v2, Vector2 n)
    {
        this.v1 = v1;
        this.v2 = v2;
        this.n = n;
    }
}


public class HalfPlane
{
    public Vector2 n;
    public Vector2 p;
    public float weight;

    public HalfPlane(Vector2 n, Vector2 p, float weight)
    {
        this.n = n;
        this.p = p;
        this.weight = weight;
    }
}

public interface INode
{
    void AddNeighbor(int neighbor, int weight);

    List<int> GetNeighbors();

    List<int> GetWeights();
}

public class NavMeshTriangle : INode
{
    public List<int> neighbors;
    public List<int> weights;
    public Vector3[] verts;
    public Vector3 centroid;

    public int navMeshIdx;
    public float area;
    public List<BoundaryEdge> boundaryEdges;

    public NavMeshTriangle()
    {
        neighbors = new List<int>();
        weights = new List<int>();
        boundaryEdges = new List<BoundaryEdge>();
        verts = new Vector3[3];
        centroid = Vector3.zero;
    }

    public void AddNeighbor(int neighbor, int weight)
    {
        this.neighbors.Add(neighbor);
        this.weights.Add(weight);
    }

    public List<int> GetNeighbors()
    {
        return this.neighbors;
    }

    public List<int> GetWeights()
    {
        return this.weights;
    }
}

public class Graph<T> where T : INode
{
    public T[] nodes;

    public Graph(T[] nodes)
    {
        this.nodes = nodes;
    }

    public int[] DijkstrasAlgorithm(int start)
    {
        HashSet<int> sptSet = new HashSet<int>();
        int[] backPointers = new int[this.nodes.Length];

        MinHeap<int, int> frontier = new MinHeap<int, int>();
        for (int i = 0; i < this.nodes.Length; i++)
        {
            if (i == start)
            {
                frontier.Insert(0, i);
            }
            else
            {
                frontier.Insert(int.MaxValue, i);
            }
        }

        backPointers[start] = -1;
        HeapElement<int, int> curNode = frontier.ExtractMin();
        while (curNode != null)
        {
            if (!sptSet.Contains(curNode.value))
            {
                List<int> neighbors = this.nodes[curNode.value].GetNeighbors();
                List<int> weights = this.nodes[curNode.value].GetWeights();
                for (int i = 0; i < neighbors.Count; i++)
                {
                    int neighbor = neighbors[i];
                    int edgeWeight = weights[i];

                    if (!sptSet.Contains(neighbor))
                    {
                        frontier.Update(edgeWeight + curNode.key, neighbor);
                        backPointers[neighbor] = curNode.value;
                    }
                }
                sptSet.Add(curNode.value);
            }
            curNode = frontier.ExtractMin();
        }
        return backPointers;
    }

    public List<int> TraceBackPointers(int[] backPointers, int end)
    {
        List<int> path = new List<int>();
        int curNode = end;
        while (curNode != -1)
        {
            path.Add(curNode);
            curNode = backPointers[curNode];

        }
        return path;
    }

    public T BreadthFirstSearch(int start, Func<T, bool> stopCondition)
    {
        HashSet<int> sptSet = new HashSet<int>();

        LinkedList<int> frontier = new LinkedList<int>();
        frontier.AddLast(start);

        LinkedListNode<int> curNode;
        int t = 0;

        while (frontier.Count > 0 && t < 100)
        {
            curNode = frontier.First;
            frontier.RemoveFirst();


            if (!sptSet.Contains(curNode.Value))
            {
                if (stopCondition(this.nodes[curNode.Value]))
                {
                    return this.nodes[curNode.Value];
                }

                List<int> neighbors = this.nodes[curNode.Value].GetNeighbors();
                for (int i = 0; i < neighbors.Count; i++)
                {
                    int neighbor = neighbors[i];

                    if (!sptSet.Contains(neighbor))
                    {
                        frontier.AddLast(neighbor);
                    }
                }
                sptSet.Add(curNode.Value);
            }
            t += 1;

        }
        return default(T);
    }


    public List<T> NearestNeighbors(int start, int n)
    {
        HashSet<int> sptSet = new HashSet<int>();
        List<T> nearest = new List<T>();

        LinkedList<int> frontier = new LinkedList<int>();
        frontier.AddLast(start);
        nearest.Add(this.nodes[start]);
        int t = 1;

        LinkedListNode<int> curNode;


        while (frontier.Count > 0 && t < n)
        {
            curNode = frontier.First;
            frontier.RemoveFirst();

            if (!sptSet.Contains(curNode.Value))
            {
                List<int> neighbors = this.nodes[curNode.Value].GetNeighbors();
                for (int i = 0; i < neighbors.Count; i++)
                {
                    int neighbor = neighbors[i];

                    if (!sptSet.Contains(neighbor))
                    {
                        frontier.AddLast(neighbor);
                        nearest.Add(this.nodes[neighbor]);
                        t += 1;
                    }
                }
                sptSet.Add(curNode.Value);
            }

        }
        return nearest;
    }

}




public class NavigationMesh : MonoBehaviour
{
    const float LARGE_FLOAT = 10000.0f;
    const float MIN_X_VEL = -20.0f;
    const float MAX_X_VEL = 20.0f;
    const float MIN_Y_VEL = -20.0f;
    const float MAX_Y_VEL = 20.0f;
    const float tau = 3.0f;

    public Graph<NavMeshTriangle> navMeshGraph;
    HashSet<int> boundaryVerts = new HashSet<int>();
    Mesh mesh;

    //Maps 2 triangle indices to the edge separating them. The edge is represented by
    //2 vertex indices
    Dictionary<Vector2Int, Vector2Int> triPairToEdgeMap;

    // Start is called before the first frame update
    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        navMeshGraph = NavMeshToGraph();
    }

    int NavMeshTriFromPos(Vector3 pos)
    {
        RaycastHit hit;
        if (Physics.Raycast(pos + Vector3.up, Vector3.down, out hit, Mathf.Infinity))
        {
            return hit.triangleIndex;
        }
        return -1;
    }

    Graph<NavMeshTriangle> NavMeshToGraph()
    {
        NavMeshTriangle[] navMeshTris = new NavMeshTriangle[mesh.triangles.Length / 3];
        for (int i = 0; i < navMeshTris.Length; i++)
        {
            navMeshTris[i] = new NavMeshTriangle();
        }
        triPairToEdgeMap = new Dictionary<Vector2Int, Vector2Int>();
        Dictionary<Vector2Int, int> edgeMap = new Dictionary<Vector2Int, int>();
        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {
            int vi1 = mesh.triangles[i];
            int vi2 = mesh.triangles[i + 1];
            int vi3 = mesh.triangles[i + 2];

            Vector2Int e1 = vi1 < vi2 ? new Vector2Int(vi1, vi2) : new Vector2Int(vi2, vi1);
            Vector2Int e2 = vi2 < vi3 ? new Vector2Int(vi2, vi3) : new Vector2Int(vi3, vi2);
            Vector2Int e3 = vi3 < vi1 ? new Vector2Int(vi3, vi1) : new Vector2Int(vi1, vi3);

            int tri = i / 3;
            navMeshTris[tri].navMeshIdx = tri;
            navMeshTris[tri].centroid = (mesh.vertices[vi1] + mesh.vertices[vi2] + mesh.vertices[vi3]) / 3;
            navMeshTris[tri].verts = new Vector3[] { mesh.vertices[vi1], mesh.vertices[vi2], mesh.vertices[vi3] };
            Vector2 p0 = new Vector2(mesh.vertices[vi1].x, mesh.vertices[vi1].z);
            Vector2 p1 = new Vector2(mesh.vertices[vi2].x, mesh.vertices[vi2].z);
            Vector2 p2 = new Vector2(mesh.vertices[vi3].x, mesh.vertices[vi3].z);
            navMeshTris[tri].area = 0.5f * (-p1.y * p2.x + p0.y * (-p1.x + p2.x) + p0.x * (p1.y - p2.y) + p1.x * p2.y);

            if (edgeMap.ContainsKey(e1))
            {
                navMeshTris[tri].AddNeighbor(edgeMap[e1], 1);
                navMeshTris[edgeMap[e1]].AddNeighbor(tri, 1);

                Vector2Int triPair = tri < edgeMap[e1] ? new Vector2Int(tri, edgeMap[e1]) : new Vector2Int(edgeMap[e1], tri);
                triPairToEdgeMap[triPair] = new Vector2Int(vi1, vi2);

                edgeMap.Remove(e1);
            }
            else
            {
                edgeMap[e1] = tri;
            }

            if (edgeMap.ContainsKey(e2))
            {
                navMeshTris[tri].AddNeighbor(edgeMap[e2], 1);
                navMeshTris[edgeMap[e2]].AddNeighbor(tri, 1);

                Vector2Int triPair = tri < edgeMap[e2] ? new Vector2Int(tri, edgeMap[e2]) : new Vector2Int(edgeMap[e2], tri);
                triPairToEdgeMap[triPair] = new Vector2Int(vi2, vi3);

                edgeMap.Remove(e2);
            }
            else
            {
                edgeMap[e2] = tri;
            }

            if (edgeMap.ContainsKey(e3))
            {
                navMeshTris[tri].AddNeighbor(edgeMap[e3], 1);
                navMeshTris[edgeMap[e3]].AddNeighbor(tri, 1);

                Vector2Int triPair = tri < edgeMap[e3] ? new Vector2Int(tri, edgeMap[e3]) : new Vector2Int(edgeMap[e3], tri);
                triPairToEdgeMap[triPair] = new Vector2Int(vi3, vi1);

                edgeMap.Remove(e3);
            }
            else
            {
                edgeMap[e3] = tri;
            }
        }

        //The keys that are left in edgeMap correspond to edges that are only part of one triangle. In other words, these edges make up
        //any boundaries of the navigation mesh
        foreach (var e in edgeMap.Keys)
        {
            int tri = edgeMap[e];
            int vi1 = mesh.triangles[3 * tri];
            int vi2 = mesh.triangles[3 * tri + 1];
            int vi3 = mesh.triangles[3 * tri + 2];
            int vi;
            if (vi1 != e[0] && vi1 != e[1])
            {
                vi = vi1;
            }
            else if (vi2 != e[0] && vi2 != e[1])
            {
                vi = vi2;
            }
            else
            {
                vi = vi3;
            }

            boundaryVerts.Add(e[0]);
            boundaryVerts.Add(e[1]);

            Vector2 ev0 = new Vector2(mesh.vertices[e[0]].x, mesh.vertices[e[0]].z);
            Vector2 ev1 = new Vector2(mesh.vertices[e[1]].x, mesh.vertices[e[1]].z);
            Vector2 v = new Vector2(mesh.vertices[vi].x, mesh.vertices[vi].z);
            Vector2 dir = (ev1 - ev0).normalized;
            Vector2 n = Vector2.Perpendicular(dir);
            if (Vector2.Dot(v - ev0, n) > 0)
            {
                navMeshTris[tri].boundaryEdges.Add(new BoundaryEdge(ev0, ev1, n));
            }
            else
            {
                navMeshTris[tri].boundaryEdges.Add(new BoundaryEdge(ev1, ev0, -n));
            }

        }

        return new Graph<NavMeshTriangle>(navMeshTris);
    }

    /*
     * if(c_origin - source).magnitude > c_radius, it returns the two points that form tangent lines from source to the circle of radius c_radius centered at c_origin. 
     * else if (c_origin - source).magnitude == c_radius, it returns the single point of tangency, twice.
     * else if (c_origin - source).magnitude < c_radius, it returns the intersections of the chord perpendicular to (c_origin - source) and passing through source.
     * 
     * */
    Vector3[] CircleTangentPoints(Vector3 c_origin, float c_radius, Vector3 source)
    {
        Vector3 dp = c_origin - source;
        if (dp.magnitude > c_radius)
        {
            float phi = Mathf.Abs(Mathf.Asin(c_radius / dp.magnitude)) * Mathf.Rad2Deg;
            Vector3 b1 = (Quaternion.AngleAxis(phi, Vector3.up) * dp).normalized;
            Vector3 b2 = (Quaternion.AngleAxis(-phi, Vector3.up) * dp).normalized;
            Vector3 p1 = Vector3.Dot(dp, b1) * b1;
            Vector3 p2 = Vector3.Dot(dp, b2) * b2;
            return new Vector3[] { p1 + source, p2 + source };
        }
        else if (dp.magnitude == c_radius)
        {
            return new Vector3[] { source, source };
        }
        else
        {
            dp = source - c_origin;
            float phi = Mathf.Abs(Mathf.Acos(dp.magnitude / c_radius)) * Mathf.Rad2Deg;
            Vector3 b1 = (Quaternion.AngleAxis(phi, Vector3.up) * dp).normalized;
            Vector3 b2 = (Quaternion.AngleAxis(-phi, Vector3.up) * dp).normalized;
            Vector3 p1 = Vector3.Dot(dp, b1) * b1;
            Vector3 p2 = Vector3.Dot(dp, b2) * b2;
            // returns chord
            return new Vector3[] { p1 + c_origin, p2 + c_origin };
        }

    }



    List<Vector3> StringPullingAlgorithm(List<int> triPath, Vector3 startPos, Vector3 targetPos, float agentRadius)
    {
        if (triPath.Count == 1)
        {
            return new List<Vector3> { targetPos };
        }

        Vector2Int triPair = triPath[0] < triPath[1] ? new Vector2Int(triPath[0], triPath[1]) : new Vector2Int(triPath[1], triPath[0]);
        Vector2Int prevE = triPairToEdgeMap[triPair];
        Vector3 v1 = mesh.vertices[prevE[0]];
        Vector3 v2 = mesh.vertices[prevE[1]];

        List<Vector2Int> edgePortals = new List<Vector2Int>();
        Dictionary<int, int> vertIndexMap = new Dictionary<int, int> { { prevE[0], 0 }, { prevE[1], 1 } };
        List<Vector3> portalVerts = new List<Vector3> { mesh.vertices[prevE[0]], mesh.vertices[prevE[1]] };

        HashSet<int> mappedBoundaryVerts = new HashSet<int>();

        if (Vector3.SignedAngle(v1 - startPos, v2 - startPos, Vector3.up) < 0)
        {
            Vector2Int eMapped = new Vector2Int(vertIndexMap[prevE[0]], vertIndexMap[prevE[1]]);
            edgePortals.Add(eMapped);

            if (boundaryVerts.Contains(prevE[0]))
            {
                mappedBoundaryVerts.Add(0);
            }
            if (boundaryVerts.Contains(prevE[1]))
            {
                mappedBoundaryVerts.Add(1);
            }
        }
        else
        {
            Vector2Int eMapped = new Vector2Int(vertIndexMap[prevE[1]], vertIndexMap[prevE[0]]);
            edgePortals.Add(eMapped);
            prevE = new Vector2Int(prevE[1], prevE[0]);


            if (boundaryVerts.Contains(prevE[0]))
            {
                mappedBoundaryVerts.Add(1);
            }
            if (boundaryVerts.Contains(prevE[1]))
            {
                mappedBoundaryVerts.Add(0);
            }
        }

        for (int i = 2; i < triPath.Count; i++)
        {
            int tri1 = triPath[i - 1];
            int tri2 = triPath[i];
            triPair = tri1 < tri2 ? new Vector2Int(tri1, tri2) : new Vector2Int(tri2, tri1);
            Vector2Int e = triPairToEdgeMap[triPair];

            if (!vertIndexMap.ContainsKey(e[0]))
            {
                vertIndexMap[e[0]] = portalVerts.Count;
                if (boundaryVerts.Contains(e[0]))
                {
                    mappedBoundaryVerts.Add(portalVerts.Count);
                }
                portalVerts.Add(mesh.vertices[e[0]]);
            }

            if (!vertIndexMap.ContainsKey(e[1]))
            {
                vertIndexMap[e[1]] = portalVerts.Count;
                if (boundaryVerts.Contains(e[1]))
                {
                    mappedBoundaryVerts.Add(portalVerts.Count);
                }
                portalVerts.Add(mesh.vertices[e[1]]);
            }

            if (e[1] == prevE[1] || e[0] == prevE[0])
            {
                Vector2Int eMapped = new Vector2Int(vertIndexMap[e[0]], vertIndexMap[e[1]]);
                edgePortals.Add(eMapped);
                prevE = e;
            }
            else if (e[1] == prevE[0] || e[0] == prevE[1])
            {
                Vector2Int eMapped = new Vector2Int(vertIndexMap[e[1]], vertIndexMap[e[0]]);
                edgePortals.Add(eMapped);
                prevE = new Vector2Int(e[1], e[0]);
            }
        }

        Vector2Int lastEdge = edgePortals[edgePortals.Count - 1];
        edgePortals.Add(new Vector2Int(portalVerts.Count, portalVerts.Count));
        portalVerts.Add(targetPos);

        foreach (Vector2Int edgePortal in edgePortals)
        {
            Debug.DrawLine(new Vector3(portalVerts[edgePortal[0]][0], 0.1f, portalVerts[edgePortal[0]][2]),
                            new Vector3(portalVerts[edgePortal[1]][0], 0.1f, portalVerts[edgePortal[1]][2]), Color.clear);
        }


        //Run Simple Stupid Funnel Algorithm.
        List<Vector3> breadCrumbs = new List<Vector3> { startPos };

        int n = 6;
        float R = agentRadius / Mathf.Cos(Mathf.PI / n);

        Vector3 minkowskiFunnelL = portalVerts[edgePortals[0][0]] - breadCrumbs[breadCrumbs.Count - 1];
        Vector3 minkowskiFunnelR = portalVerts[edgePortals[0][1]] - breadCrumbs[breadCrumbs.Count - 1];

        if (mappedBoundaryVerts.Contains(edgePortals[0][0]))
        {
            Vector3[] funnelLTangents = CircleTangentPoints(portalVerts[edgePortals[0][0]], agentRadius, breadCrumbs[breadCrumbs.Count - 1]);

            if (Vector3.SignedAngle(minkowskiFunnelL, funnelLTangents[0] - breadCrumbs[breadCrumbs.Count - 1], Vector3.up) <= 0)
            {
                minkowskiFunnelL = (R * (funnelLTangents[0] - portalVerts[edgePortals[0][0]]).normalized + portalVerts[edgePortals[0][0]]) - breadCrumbs[breadCrumbs.Count - 1];
            }
            else
            {
                minkowskiFunnelL = (R * (funnelLTangents[1] - portalVerts[edgePortals[0][0]]).normalized + portalVerts[edgePortals[0][0]]) - breadCrumbs[breadCrumbs.Count - 1];
            }
        }

        if (mappedBoundaryVerts.Contains(edgePortals[0][1]))
        {
            Vector3[] funnelRTangents = CircleTangentPoints(portalVerts[edgePortals[0][1]], agentRadius, breadCrumbs[breadCrumbs.Count - 1]);

            if (Vector3.SignedAngle(minkowskiFunnelR, funnelRTangents[0] - breadCrumbs[breadCrumbs.Count - 1], Vector3.up) >= 0)
            {
                minkowskiFunnelR = (R * (funnelRTangents[0] - portalVerts[edgePortals[0][1]]).normalized + portalVerts[edgePortals[0][1]]) - breadCrumbs[breadCrumbs.Count - 1];
            }
            else
            {
                minkowskiFunnelR = (R * (funnelRTangents[1] - portalVerts[edgePortals[0][1]]).normalized + portalVerts[edgePortals[0][1]]) - breadCrumbs[breadCrumbs.Count - 1];
            }
        }


        int epL = 1;
        int epR = 1;
        int ep = 1;

        int t = 0;

        while (ep < edgePortals.Count && t < 1000)
        {

            Vector3 newMinkowskiFunnelL = portalVerts[edgePortals[ep][0]] - breadCrumbs[breadCrumbs.Count - 1];
            if (mappedBoundaryVerts.Contains(edgePortals[ep][0]))
            {
                Vector3[] funnelLTangents = CircleTangentPoints(portalVerts[edgePortals[ep][0]], agentRadius, breadCrumbs[breadCrumbs.Count - 1]);
                if (Vector3.SignedAngle(newMinkowskiFunnelL, funnelLTangents[0] - breadCrumbs[breadCrumbs.Count - 1], Vector3.up) <= 0)
                {
                    newMinkowskiFunnelL = (R * (funnelLTangents[0] - portalVerts[edgePortals[ep][0]]).normalized + portalVerts[edgePortals[ep][0]]) - breadCrumbs[breadCrumbs.Count - 1];
                }
                else
                {
                    newMinkowskiFunnelL = (R * (funnelLTangents[1] - portalVerts[edgePortals[ep][0]]).normalized + portalVerts[edgePortals[ep][0]]) - breadCrumbs[breadCrumbs.Count - 1];
                }

            }

            if (Vector3.SignedAngle(minkowskiFunnelL, newMinkowskiFunnelL, Vector3.up) <= 0)
            {
                if (edgePortals[ep][0] == edgePortals[epL][0] || Vector3.SignedAngle(newMinkowskiFunnelL, minkowskiFunnelR, Vector3.up) <= 0)
                {
                    minkowskiFunnelL = newMinkowskiFunnelL;
                    epL = ep;
                }
                else
                {
                    breadCrumbs.Add(breadCrumbs[breadCrumbs.Count - 1] + minkowskiFunnelR);
                    ep = epR;
                    epL = ep;
                    epR = ep;
                    minkowskiFunnelL = portalVerts[edgePortals[epL][0]] - breadCrumbs[breadCrumbs.Count - 1];
                    if (mappedBoundaryVerts.Contains(edgePortals[epL][0]))
                    {
                        Vector3[] funnelLTangents = CircleTangentPoints(portalVerts[edgePortals[epL][0]], agentRadius, breadCrumbs[breadCrumbs.Count - 1]);
                        if (Vector3.SignedAngle(minkowskiFunnelL, funnelLTangents[0] - breadCrumbs[breadCrumbs.Count - 1], Vector3.up) <= 0)
                        {
                            minkowskiFunnelL = (R * (funnelLTangents[0] - portalVerts[edgePortals[epL][0]]).normalized + portalVerts[edgePortals[epL][0]]) - breadCrumbs[breadCrumbs.Count - 1];
                        }
                        else
                        {
                            minkowskiFunnelL = (R * (funnelLTangents[1] - portalVerts[edgePortals[epL][0]]).normalized + portalVerts[edgePortals[epL][0]]) - breadCrumbs[breadCrumbs.Count - 1];
                        }

                    }
                    minkowskiFunnelR = portalVerts[edgePortals[epR][1]] - breadCrumbs[breadCrumbs.Count - 1];
                    if (mappedBoundaryVerts.Contains(edgePortals[epR][1]))
                    {
                        Vector3[] funnelRTangents = CircleTangentPoints(portalVerts[edgePortals[epR][1]], agentRadius, breadCrumbs[breadCrumbs.Count - 1]);
                        if (Vector3.SignedAngle(minkowskiFunnelR, funnelRTangents[0] - breadCrumbs[breadCrumbs.Count - 1], Vector3.up) >= 0)
                        {
                            minkowskiFunnelR = (R * (funnelRTangents[0] - portalVerts[edgePortals[epR][1]]).normalized + portalVerts[edgePortals[epR][1]]) - breadCrumbs[breadCrumbs.Count - 1];
                        }
                        else
                        {
                            minkowskiFunnelR = (R * (funnelRTangents[1] - portalVerts[edgePortals[epR][1]]).normalized + portalVerts[edgePortals[epR][1]]) - breadCrumbs[breadCrumbs.Count - 1];
                        }
                    }
                    continue;
                }

            }

            Vector3 newMinkowskiFunnelR = portalVerts[edgePortals[ep][1]] - breadCrumbs[breadCrumbs.Count - 1];
            if (mappedBoundaryVerts.Contains(edgePortals[ep][1]))
            {
                Vector3[] funnelRTangents = CircleTangentPoints(portalVerts[edgePortals[ep][1]], agentRadius, breadCrumbs[breadCrumbs.Count - 1]);
                newMinkowskiFunnelR = Vector3.SignedAngle(newMinkowskiFunnelR, funnelRTangents[0] - breadCrumbs[breadCrumbs.Count - 1], Vector3.up) >= 0 ?
                                funnelRTangents[0] - breadCrumbs[breadCrumbs.Count - 1] : funnelRTangents[1] - breadCrumbs[breadCrumbs.Count - 1];

                if (Vector3.SignedAngle(newMinkowskiFunnelR, funnelRTangents[0] - breadCrumbs[breadCrumbs.Count - 1], Vector3.up) >= 0)
                {
                    newMinkowskiFunnelR = (R * (funnelRTangents[0] - portalVerts[edgePortals[ep][1]]).normalized + portalVerts[edgePortals[ep][1]]) - breadCrumbs[breadCrumbs.Count - 1];
                }
                else
                {
                    newMinkowskiFunnelR = (R * (funnelRTangents[1] - portalVerts[edgePortals[ep][1]]).normalized + portalVerts[edgePortals[ep][1]]) - breadCrumbs[breadCrumbs.Count - 1];
                }
            }

            if (Vector3.SignedAngle(minkowskiFunnelR, newMinkowskiFunnelR, Vector3.up) >= 0)
            {
                if (edgePortals[ep][1] == edgePortals[epR][1] || Vector3.SignedAngle(minkowskiFunnelL, newMinkowskiFunnelR, Vector3.up) <= 0)
                {
                    minkowskiFunnelR = newMinkowskiFunnelR;
                    epR = ep;
                }
                else
                {
                    //breadCrumbs.Add(portalVerts[edgePortals[epL][0]]);
                    breadCrumbs.Add(breadCrumbs[breadCrumbs.Count - 1] + minkowskiFunnelL);
                    ep = epL;
                    epL = ep;
                    epR = ep;
                    minkowskiFunnelL = portalVerts[edgePortals[epL][0]] - breadCrumbs[breadCrumbs.Count - 1];
                    if (mappedBoundaryVerts.Contains(edgePortals[epL][0]))
                    {
                        Vector3[] funnelLTangents = CircleTangentPoints(portalVerts[edgePortals[epL][0]], agentRadius, breadCrumbs[breadCrumbs.Count - 1]);
                        if (Vector3.SignedAngle(minkowskiFunnelL, funnelLTangents[0] - breadCrumbs[breadCrumbs.Count - 1], Vector3.up) <= 0)
                        {
                            minkowskiFunnelL = (R * (funnelLTangents[0] - portalVerts[edgePortals[epL][0]]).normalized + portalVerts[edgePortals[epL][0]]) - breadCrumbs[breadCrumbs.Count - 1];
                        }
                        else
                        {
                            minkowskiFunnelL = (R * (funnelLTangents[1] - portalVerts[edgePortals[epL][0]]).normalized + portalVerts[edgePortals[epL][0]]) - breadCrumbs[breadCrumbs.Count - 1];

                        }

                    }
                    minkowskiFunnelR = portalVerts[edgePortals[epR][1]] - breadCrumbs[breadCrumbs.Count - 1];
                    if (mappedBoundaryVerts.Contains(edgePortals[epR][1]))
                    {
                        Vector3[] funnelRTangents = CircleTangentPoints(portalVerts[edgePortals[epR][1]], agentRadius, breadCrumbs[breadCrumbs.Count - 1]);
                        if (Vector3.SignedAngle(minkowskiFunnelR, funnelRTangents[0] - breadCrumbs[breadCrumbs.Count - 1], Vector3.up) >= 0)
                        {
                            minkowskiFunnelR = (R * (funnelRTangents[0] - portalVerts[edgePortals[epR][1]]).normalized + portalVerts[edgePortals[epR][1]]) - breadCrumbs[breadCrumbs.Count - 1];
                        }
                        else
                        {
                            minkowskiFunnelR = (R * (funnelRTangents[1] - portalVerts[edgePortals[epR][1]]).normalized + portalVerts[edgePortals[epR][1]]) - breadCrumbs[breadCrumbs.Count - 1];
                        }
                    }
                    continue;
                }
            }

            ep += 1;
            t += 1;

        }

        if (t == 100)
        {
            Debug.LogError("FUNNEL ALG DIDNT WORK");
        }

        breadCrumbs.Add(targetPos);


        //Draws path points
        for (int i = 1; i < breadCrumbs.Count; i++)
        {
            Debug.DrawLine(breadCrumbs[i - 1], breadCrumbs[i], Color.green);
        }

        breadCrumbs.RemoveAt(0);
        return breadCrumbs;
    }


    void LateUpdate()
    {
        List<NavAgent> agents = AIManager.Instance.agents;

        for (int i = 0; i < agents.Count; i++)
        {
            int targetTriIdx = NavMeshTriFromPos(agents[i].target.position);
            if (targetTriIdx >= 0)
            {
                agents[i].targetNavMeshTriIdx = targetTriIdx;
            }

            int agentTriIdx = NavMeshTriFromPos(agents[i].transform.position);
            if (agentTriIdx >= 0)
            {
                agents[i].navMeshTriIdx = agentTriIdx;
                int[] backPointers = navMeshGraph.DijkstrasAlgorithm(agents[i].targetNavMeshTriIdx);
                List<int> triPath = navMeshGraph.TraceBackPointers(backPointers, agents[i].navMeshTriIdx);
                Vector3 agentStartPos = new Vector3(agents[i].transform.position.x, 0.0f, agents[i].transform.position.z);
                List<Vector3> shortestPathPoints = StringPullingAlgorithm(triPath, agentStartPos, agents[i].target.position, agents[i].radius);
                agents[i].pathPoints = shortestPathPoints;
                agents[i].ORCAHalfPlanes = new List<HalfPlane>();

            }
            else
            {
                //AI agent is out of bounds. Make it head towards last navigation mesh triangle
                agents[i].pathPoints = new List<Vector3>() { navMeshGraph.nodes[agents[i].navMeshTriIdx].centroid };
                agents[i].ORCAHalfPlanes = new List<HalfPlane>();
            }

        }

        //TODO: Optimize this
        for (int i = 0; i < agents.Count; i++)
        {
            NavAgent agentA = agents[i];
            Player2AgentORCA(ref AIManager.Instance.playerAgent, ref agentA);
            for (int j = i + 1; j < agents.Count; j++)
            {
                NavAgent agentB = agents[j];
                Agent2AgentORCA(ref agentA, ref agentB);
            }
        }


        foreach (NavAgent agentA in agents)
        {
            bool noValidVelocity = false;
            Vector2 desiredVelocity = new Vector2(agentA.desiredHeading.x, agentA.desiredHeading.z);
            Vector2 optimalHeading = desiredVelocity;

            List<HalfPlane> bounds = new List<HalfPlane>();
            bounds.Add(new HalfPlane(Vector2.down, new Vector2(0, 10), 1.0f));
            bounds.Add(new HalfPlane(Vector2.up, new Vector2(0, -10), 1.0f));
            bounds.Add(new HalfPlane(Vector2.left, new Vector2(10, 0), 1.0f));
            bounds.Add(new HalfPlane(Vector2.right, new Vector2(-10, 0), 1.0f));

            foreach (HalfPlane halfPlane in agentA.ORCAHalfPlanes)
            {
                /*
                if(agentA.id == "A")
                {
                    TraceHalfPlane(agentA, halfPlane);
                }
                */
                

                if (Vector2.Dot(optimalHeading - halfPlane.p, halfPlane.n) < 0)
                {
                    Vector2 dir = Vector2.Perpendicular(halfPlane.n);
                    LineSegment optimalInterval = new LineSegment(halfPlane.p - LARGE_FLOAT * dir, halfPlane.p + LARGE_FLOAT * dir);
                    foreach (HalfPlane bound in bounds)
                    {
                        float D = halfPlane.n.x * bound.n.y - halfPlane.n.y * bound.n.x;
                        float Dx = Vector2.Dot(halfPlane.n, halfPlane.p) * bound.n.y - halfPlane.n.y * Vector2.Dot(bound.n, bound.p);
                        float Dy = halfPlane.n.x * Vector2.Dot(bound.n, bound.p) - Vector2.Dot(halfPlane.n, halfPlane.p) * bound.n.x;
                        Vector2 intersection = new Vector2(Dx / D, Dy / D);

                        if (Vector2.Dot(bound.n, dir) > 0)
                        {
                            if (Vector2.Dot(intersection - optimalInterval.p1, dir) > 0)
                            {
                                optimalInterval.p1 = intersection;
                            }
                        }
                        else
                        {
                            if (Vector2.Dot(intersection - optimalInterval.p2, -dir) > 0)
                            {
                                optimalInterval.p2 = intersection;
                            }
                        }
                    }

                    if (Vector2.Dot(optimalInterval.Dir, dir) > 0)
                    {
                        optimalHeading = Vector2.Distance(optimalInterval.p1, desiredVelocity) < Vector2.Distance(optimalInterval.p2, desiredVelocity) ? optimalInterval.p1 : optimalInterval.p2;

                        //Test perpendicular distance from desiredVelocity to optimalInterval
                        Vector2 n = Vector2.Perpendicular(optimalInterval.Dir);
                        float D = optimalInterval.Dir.x * n.y - optimalInterval.Dir.y * n.x;
                        float Dx = Vector2.Dot(optimalInterval.Dir, desiredVelocity) * n.y - optimalInterval.Dir.y * Vector2.Dot(n, optimalInterval.p1);
                        float Dy = optimalInterval.Dir.x * Vector2.Dot(n, optimalInterval.p1) - Vector2.Dot(optimalInterval.Dir, desiredVelocity) * n.x;
                        Vector2 potentialHeading = new Vector2(Dx / D, Dy / D);
                        if (Vector2.Distance(potentialHeading, desiredVelocity) < Vector2.Distance(optimalHeading, desiredVelocity) &&
                            Vector2.Dot(optimalInterval.p2 - potentialHeading, optimalInterval.p1 - potentialHeading) < 0)
                        {
                            optimalHeading = potentialHeading;
                        }
                    }
                    else
                    {
                        noValidVelocity = true;
                        break;
                    }
                }
                bounds.Add(halfPlane);
            }



            if (noValidVelocity)
            {
                //There is no velocity that avoids obstacles. Find velocity that satisfies the least squares
                //distances from each half plane

                float[,] AT_A = new float[2, 2] { {0.0f, 0.0f },
                                              {0.0f, 0.0f } };
                float[] AT_b = new float[2] { 0.0f, 0.0f };

                foreach (HalfPlane halfPlane in agentA.ORCAHalfPlanes)
                {
                    Vector2 Ai = halfPlane.n / halfPlane.weight;
                    float bi = Vector2.Dot(halfPlane.n, halfPlane.p) / halfPlane.weight;
                    AT_A[0, 0] += Ai[0] * Ai[0];
                    AT_A[1, 0] += Ai[1] * Ai[0];
                    AT_A[0, 1] += Ai[0] * Ai[1];
                    AT_A[1, 1] += Ai[1] * Ai[1];
                    AT_b[0] += Ai[0] * bi;
                    AT_b[1] += Ai[1] * bi;
                }

                float[,] AT_A_inv = new float[2, 2];
                float det = AT_A[0, 0] * AT_A[1, 1] - AT_A[1, 0] * AT_A[0, 1];
                AT_A_inv[0, 0] = AT_A[1, 1] / det;
                AT_A_inv[1, 0] = -AT_A[1, 0] / det;
                AT_A_inv[0, 1] = -AT_A[0, 1] / det;
                AT_A_inv[1, 1] = AT_A[0, 0] / det;

                float xVel = AT_A_inv[0, 0] * AT_b[0] + AT_A_inv[0, 1] * AT_b[1];
                float zVel = AT_A_inv[1, 0] * AT_b[0] + AT_A_inv[1, 1] * AT_b[1];
                optimalHeading = new Vector2(xVel, zVel);
                optimalHeading = Mathf.Clamp(optimalHeading.magnitude, 0.0f, 10.0f) * optimalHeading.normalized;
            }

            agentA.MoveAgent(new Vector3(optimalHeading.x, 0.0f, optimalHeading.y));
        }


    }

    void TraceHalfPlane(NavAgent agent, HalfPlane halfPlane)
    {
        //Debug.Log(halfPlane.n);
        Vector3 start = new Vector3(agent.transform.position.x, 1.0f, agent.transform.position.z) + new Vector3(halfPlane.p.x, 0.0f, halfPlane.p.y);
        Vector2 lineDir = Vector2.Perpendicular(halfPlane.n);
        Debug.DrawLine(start, start + 30.0f * new Vector3(lineDir.x, 0, lineDir.y), Color.yellow);
        Debug.DrawLine(start, start - 30.0f * new Vector3(lineDir.x, 0, lineDir.y), Color.yellow);
        Debug.DrawLine(start, start + 5.0f * new Vector3(halfPlane.n.x, 0, halfPlane.n.y), Color.cyan);

    }

    //Create Optimal reciprocal Collision Avoidance half plane
    void Agent2AgentORCA(ref NavAgent agentA, ref NavAgent agentB)
    {
        Vector3 dp = agentB.transform.position - agentA.transform.position;
        Vector2 vCenter = new Vector2(dp.x, dp.z);
        Vector2 vCenterScaled = new Vector2(dp.x, dp.z) / tau;
        float r = agentA.radius + agentB.radius;
        float r_scaled = r / tau;
        float phi = Mathf.Abs(Mathf.Asin(Mathf.Min(r / vCenter.magnitude, 1.0f))) * Mathf.Rad2Deg;
        float alpha = 90.0f - phi;

        Vector2 velObstacleDir = (vCenter - vCenterScaled).normalized;

        Vector2 vOptA = new Vector2(agentA.desiredHeading.x, agentA.desiredHeading.z);
        Vector2 vOptB = new Vector2(agentB.desiredHeading.x, agentB.desiredHeading.z);
        Vector2 vx = vOptA - vOptB;
        Vector2 vx_c = vx - vCenterScaled;

        float theta = Vector2.SignedAngle(velObstacleDir, vx_c);
        //float theta = Vector3.SignedAngle(new Vector3(velObstacleDir[0], 0, velObstacleDir[1]), new Vector3(vx_c[0], 0, vx_c[1]), Vector3.up);
        Vector2 u;
        Vector2 n;

        if (theta < 0.0f && theta > -(180.0f - alpha))
        {
            Vector2 b1 = Quaternion.AngleAxis(-phi, new Vector3(0, 0, 1)) * vCenter;
            u = Vector2.Dot(vx, b1.normalized) * b1.normalized - vx;
            n = -Vector2.Perpendicular(b1).normalized;
        }
        else if (theta >= 0.0f && theta < (180.0f - alpha))
        {
            Vector2 b2 = Quaternion.AngleAxis(phi, new Vector3(0, 0, 1)) * vCenter;
            u = Vector2.Dot(vx, b2.normalized) * b2.normalized - vx;
            n = Vector2.Perpendicular(b2).normalized;
        }
        else
        {
            u = (r_scaled * vx_c.normalized + vCenterScaled) - vx;
            n = vx_c.magnitude < Mathf.Epsilon ? -vCenter.normalized : vx_c.normalized;
        }


        agentA.ORCAHalfPlanes.Add(new HalfPlane(n, vOptA + 0.5f * u, (vCenter - vCenterScaled).magnitude));
        agentB.ORCAHalfPlanes.Add(new HalfPlane(-n, vOptB - 0.5f * u, (vCenter - vCenterScaled).magnitude));
    }

   
    void Player2AgentORCA(ref PlayerMovementController agentA, ref NavAgent agentB)
    {
        Vector3 dp = agentB.transform.position - agentA.transform.position;
        Vector2 vCenter = new Vector2(dp.x, dp.z);
        Vector2 vCenterScaled = new Vector2(dp.x, dp.z) / tau;
        float r = agentA.radius + agentB.radius;
        float r_scaled = r / tau;
        float phi = Mathf.Abs(Mathf.Asin(Mathf.Min(r / vCenter.magnitude, 1.0f))) * Mathf.Rad2Deg;
        float alpha = 90.0f - phi;

        Vector2 velObstacleDir = (vCenter - vCenterScaled).normalized;

        Vector2 vOptA = new Vector2(agentA.velocity.x, agentA.velocity.z);
        Vector2 vOptB = new Vector2(agentB.desiredHeading.x, agentB.desiredHeading.z);
        Vector2 vx = vOptA - vOptB;
        Vector2 vx_c = vx - vCenterScaled;

        float theta = Vector2.SignedAngle(velObstacleDir, vx_c);
        //float theta = Vector3.SignedAngle(new Vector3(velObstacleDir[0], 0, velObstacleDir[1]), new Vector3(vx_c[0], 0, vx_c[1]), Vector3.up);
        Vector2 u;
        Vector2 n;

        if (theta < 0.0f && theta > -(180.0f - alpha))
        {
            Vector2 b1 = Quaternion.AngleAxis(-phi, new Vector3(0, 0, 1)) * vCenter;
            u = Vector2.Dot(vx, b1.normalized) * b1.normalized - vx;
            n = -Vector2.Perpendicular(b1).normalized;
        }
        else if (theta >= 0.0f && theta < (180.0f - alpha))
        {
            Vector2 b2 = Quaternion.AngleAxis(phi, new Vector3(0, 0, 1)) * vCenter;
            u = Vector2.Dot(vx, b2.normalized) * b2.normalized - vx;
            n = Vector2.Perpendicular(b2).normalized;
        }
        else
        {
            u = (r_scaled * vx_c.normalized + vCenterScaled) - vx;
            n = vx_c.magnitude < Mathf.Epsilon ? -vCenter.normalized : vx_c.normalized;
        }

        agentB.ORCAHalfPlanes.Add(new HalfPlane(-n, vOptB - u, 0.01f * (vCenter - vCenterScaled).magnitude));
    }
    
}


