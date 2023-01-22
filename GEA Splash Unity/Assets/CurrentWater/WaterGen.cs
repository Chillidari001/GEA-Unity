using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterGen : MonoBehaviour
{
    public int DimensionSize = 10;
    public Octave[] Octaves;
    public float UVScale;

    protected MeshFilter MeshFilter;
    protected Mesh Mesh;

    void Start()
    {
        //mesh creation
        Mesh = new Mesh();
        Mesh.name = gameObject.name;

        Mesh.vertices = GenerateVerts();
        Mesh.triangles = GenerateTris();
        Mesh.uv = GenerateUVs();
        Mesh.RecalculateBounds();
        Mesh.RecalculateNormals();

        MeshFilter = gameObject.AddComponent<MeshFilter>();
        MeshFilter.mesh = Mesh;
    }

    private Vector3[] GenerateVerts()
    {
        var verts = new Vector3[(DimensionSize + 1) * (DimensionSize + 1)];

        //equally distribute verts
        for (int i = 0; i <= DimensionSize; i++)
        {
            for (int b = 0; b <= DimensionSize; b++)
            {
                verts[index(i, b)] = new Vector3(i, 0, b);
            }
        }

        return verts;
    }

    private int index(float i, float b) //changed parameters to floats and converted them to integer for execution to fix error in GetHeight function
    {
        //i = 0 b = 0 index = 0... i = 0, b = 9, index = 9... i = 1 b = 0, index = 11
        return (int)(i * (DimensionSize + 1) + b);
    }

    private Vector2[] GenerateUVs()
    {
        var uvs = new Vector2[Mesh.vertices.Length];

        //set one uv over n tiles then flip and set it again
        for(int x = 0; x <= DimensionSize; x++)
        {
            for(int z = 0; z <= DimensionSize; z++)
            {
                var vec = new Vector2((x / UVScale) % 2, (z / UVScale) % 2);
                uvs[index(x, z)] = new Vector2(vec.x <= 1 ? vec.x : 2 - vec.x, vec.y <= 1 ? vec.y : 2 - vec.y);
            }
        }

        return uvs;
    }


    private int[] GenerateTris()
    {
        var tris = new int[Mesh.vertices.Length * 6];

        //two triangles is one tile
        for(int i = 0; i < DimensionSize; i++)
        {
            for(int b = 0; b < DimensionSize; b++)
            {
                tris[index(i, b) * 6 + 0] = index(i, b);
                tris[index(i, b) * 6 + 1] = index(i + 1, b + 1);
                tris[index(i, b) * 6 + 2] = index(i + 1, b);
                tris[index(i, b) * 6 + 3] = index(i, b);
                tris[index(i, b) * 6 + 4] = index(i, b + 1);
                tris[index(i, b) * 6 + 5] = index(i + 1, b + 1);
            }
        }

        return tris;
    }

    void Update()
    {
        var verts = Mesh.vertices;
        for (int x = 0; x <= DimensionSize; x++)
        {
            for (int z = 0; z <= DimensionSize; z++)
            {
                var y = 0f;
                for (int i = 0; i < Octaves.Length; i++)
                {
                    if (Octaves[i].alternate)
                    {
                        var perl = Mathf.PerlinNoise((x * Octaves[i].scale.x) / DimensionSize, (z * Octaves[i].scale.y) / DimensionSize) * Mathf.PI * 2f; 
                        y += Mathf.Cos(perl + Octaves[i].speed.magnitude * Time.time) * Octaves[i].height;
                    }
                    else
                    {
                        var perl = Mathf.PerlinNoise((x * Octaves[i].scale.x + Time.time * Octaves[i].speed.x) / DimensionSize, 
                            (z * Octaves[i].scale.y + Time.time * Octaves[i].speed.y) / DimensionSize) - 0.5f;
                        y += perl * Octaves[i].height;
                    }
                }

                verts[index(x, z)] = new Vector3(x, y, z);
            }
        }

        Mesh.vertices = verts;
        Mesh.RecalculateNormals();
    }

    [Serializable]
    public struct Octave
    {
        public Vector2 speed;
        public Vector2 scale;
        public float height;
        public bool alternate;
    }

    public float GetHeight(Vector3 position)
    {
        //scale factor and position in local space
        var scale = new Vector3(1 / transform.lossyScale.x, 0, 1 / transform.lossyScale.z);
        var localPos = Vector3.Scale((position - transform.position), scale);

        //get edge points
        var p1 = new Vector3(Mathf.Floor(localPos.x), 0, Mathf.Floor(localPos.z));
        var p2 = new Vector3(Mathf.Floor(localPos.x), 0, Mathf.Ceil(localPos.z));
        var p3 = new Vector3(Mathf.Ceil(localPos.x), 0, Mathf.Floor(localPos.z));
        var p4 = new Vector3(Mathf.Ceil(localPos.x), 0, Mathf.Ceil(localPos.z));

        //clamp if position is outside of plane
        p1.x = Mathf.Clamp(p1.x, 0, DimensionSize);
        p1.z = Mathf.Clamp(p1.z, 0, DimensionSize);
        p2.x = Mathf.Clamp(p2.x, 0, DimensionSize);
        p2.z = Mathf.Clamp(p2.z, 0, DimensionSize);
        p3.x = Mathf.Clamp(p3.x, 0, DimensionSize);
        p3.z = Mathf.Clamp(p3.z, 0, DimensionSize);
        p4.x = Mathf.Clamp(p4.x, 0, DimensionSize);
        p4.z = Mathf.Clamp(p4.z, 0, DimensionSize);

        //get max distance to an edge and take that to compute max - dist
        var max = Mathf.Max(Vector3.Distance(p1, localPos), Vector3.Distance(p2, localPos), Vector3.Distance(p3, localPos), Vector3.Distance(p4, localPos)
            + Mathf.Epsilon);
        var dist = (max - Vector3.Distance(p1, localPos))
                 + (max - Vector3.Distance(p2, localPos))
                 + (max - Vector3.Distance(p3, localPos))
                 + (max - Vector3.Distance(p4, localPos)) + Mathf.Epsilon;

        //weighted sum
        var height = Mesh.vertices[index(p1.x, p1.z)].y * (max - Vector3.Distance(p1, localPos))
                   + Mesh.vertices[index(p2.x, p2.z)].y * (max - Vector3.Distance(p2, localPos))
                   + Mesh.vertices[index(p3.x, p3.z)].y * (max - Vector3.Distance(p3, localPos))
                   + Mesh.vertices[index(p4.x, p4.z)].y * (max - Vector3.Distance(p4, localPos));

        //scale
        return height * transform.lossyScale.y / dist;
    }
}
