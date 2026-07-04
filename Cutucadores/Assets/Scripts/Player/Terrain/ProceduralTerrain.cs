using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class ProceduralTerrain : MonoBehaviour
{
    [Header("Test")]
    [SerializeField] private Transform player;
    [SerializeField] private bool drawGizmos = false;

    [Header("Grid Config")]
    [SerializeField] private int resolution = 30;
    [SerializeField] private float mapSize = 140;
    private float CellSize => mapSize / resolution;
    [SerializeField] private int verticalResolution = 5;
    [SerializeField] private float height = 15;
    private float CellHeight => height / verticalResolution;
    private float[,,] grid;

    [Header("Components")]
    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private MeshCollider meshCollider;

    private List<Vector3> vertices = new();
    private List<int> triangles = new();

    [Button("Generate mesh")]
    private void Start()
    {
        grid = new float[resolution + 1, verticalResolution + 1, resolution + 1];
        for (int x = 0; x < resolution + 1; x++)
        {
            for (int z = 0; z < resolution + 1; z++)
            {
                for (int y = 0; y < verticalResolution + 1; y++)
                {
                    if (x == resolution || x == 0 || z == resolution || z == 0 || y == verticalResolution || y == 0)
                        grid[x, y, z] = 0;
                    else
                        grid[x, y, z] = 1;
                }
            }
        }
        MarchCubes();
        SetMesh();
        SetCollider();
    }

    public void BreakTerrain(Vector3 impactPoint, float radius)
    {
        bool hasGridChanged = false;
        impactPoint.y = 0;
        Vector3Int targetCenter = WorldToGridPosition(impactPoint);
        int targetRadius = Mathf.CeilToInt(radius / CellSize);

        for (int x = targetCenter.x - targetRadius; x <= targetCenter.x + targetRadius; x++)
        {
            if (x < 0 || x > resolution)
                continue;
            for (int z = targetCenter.z - targetRadius; z <= targetCenter.z + targetRadius; z++)
            {
                if (z < 0 || z > resolution)
                    continue;
                if (grid[x, 1, z] == 0)
                    continue;

                Vector3 voxelPosition = GridToWorldPosition(x, 0, z);
                Vector3 delta = voxelPosition - impactPoint;
                if (delta.sqrMagnitude <= radius * radius)
                {
                    hasGridChanged = true;
                    for (int y = 0; y < verticalResolution; y++)
                        grid[x, y, z] = 0;
                }
            }
        }
        if (hasGridChanged)
        {
            MarchCubes();
            SetMesh();
            SetCollider();
        }
    }
    public void BreakTerrain(Vector3[] impactPoints, float radius)
    {
        foreach (Vector3 impactPoint in impactPoints)
        {
            BreakTerrain(impactPoint, radius);
        }
    }

    private int GetConfigIndex(float[] cubeCorners)
    {
        int configIndex = 0;

        for (int i = 0; i < 8; i++)
        {
            if (cubeCorners[i] > 0)
            {
                configIndex |= 1 << i;
            }
        }

        return configIndex;
    }

    private void MarchCubes()
    {
        vertices.Clear();
        triangles.Clear();

        for (int x = 0; x < resolution; x++)
        {
            for (int z = 0; z < resolution; z++)
            {
                for (int y = 0; y < verticalResolution; y++)
                {
                    float[] cubeCorners = new float[8];

                    for (int i = 0; i < 8; i++)
                    {
                        Vector3Int corner = new Vector3Int(x, y, z) + MarchingTable.Corners[i];
                        cubeCorners[i] = grid[corner.x, corner.y, corner.z];
                    }

                    MarchCube(GridToWorldPosition(x, y, z), cubeCorners);
                }
            }
        }
    }

    private void MarchCube(Vector3 position, float[] cubeCorners)
    {
        int configIndex = GetConfigIndex(cubeCorners);

        if (configIndex == 0 || configIndex == 255)
            return;

        int edgeIndex = 0;

        for (int t = 0; t < 5; t++)
        {
            Vector3[] triVerts = new Vector3[3];

            for (int v = 0; v < 3; v++)
            {
                int triTableValue = MarchingTable.Triangles[configIndex, edgeIndex];

                if (triTableValue == -1)
                    return;

                Vector3 edgeStartSize = new(
                    MarchingTable.Edges[triTableValue, 0].x * CellSize,
                    MarchingTable.Edges[triTableValue, 0].y * CellHeight,
                    MarchingTable.Edges[triTableValue, 0].z * CellSize);

                Vector3 edgeEndSize = new(
                    MarchingTable.Edges[triTableValue, 1].x * CellSize,
                    MarchingTable.Edges[triTableValue, 1].y * CellHeight,
                    MarchingTable.Edges[triTableValue, 1].z * CellSize);

                Vector3 edgeStart = position + edgeStartSize;
                Vector3 edgeEnd = position + edgeEndSize;

                triVerts[v] = (edgeStart + edgeEnd) / 2;

                edgeIndex++;
            }

            int start = vertices.Count;

            vertices.Add(triVerts[0]);
            vertices.Add(triVerts[1]);
            vertices.Add(triVerts[2]);

            triangles.Add(start);
            triangles.Add(start + 2);
            triangles.Add(start + 1);
        }
    }

    private void SetMesh()
    {
        meshFilter.mesh.Clear();
        meshFilter.mesh.vertices = vertices.ToArray();
        meshFilter.mesh.triangles = triangles.ToArray();
        meshFilter.mesh.RecalculateNormals();
    }

    private void SetCollider()
    {
        meshCollider.sharedMesh = null;
        meshCollider.sharedMesh = meshFilter.mesh;
    }

    private Vector3 GridToWorldPosition(int x, int y, int z)
    {
        return transform.position + new Vector3(
            x * CellSize - mapSize / 2,
            y * CellHeight,
            z * CellSize - mapSize / 2
            );
    }

    private Vector3Int WorldToGridPosition(Vector3 worldPosition)
    {
        Vector3 localPosition = worldPosition - transform.position;
        localPosition.x += mapSize / 2;
        localPosition.z += mapSize / 2;
        return new Vector3Int(
            Mathf.Clamp(Mathf.FloorToInt(localPosition.x / CellSize), 0, resolution),
            Mathf.Clamp(Mathf.FloorToInt(localPosition.y / CellHeight), 0, verticalResolution),
            Mathf.Clamp(Mathf.FloorToInt(localPosition.z / CellSize), 0, resolution)
            );
    }

    private void OnDrawGizmos()
    {
        if (grid == null || !drawGizmos)
            return;
        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int z = 0; z < grid.GetLength(2); z++)
            {
                for (int y = 0; y < grid.GetLength(1); y++)
                {
                    Color color = Color.white * grid[x, y, z];
                    color.a = .3f;
                    Gizmos.color = color;
                    Gizmos.DrawSphere(GridToWorldPosition(x, y, z), 1f);
                }
            }
        }
    }
}
