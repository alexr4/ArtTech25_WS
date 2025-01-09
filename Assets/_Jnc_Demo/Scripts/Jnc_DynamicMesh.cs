using UnityEngine;

public static class Jnc_DynamicMesh
{
    [System.Serializable]
    public struct TapeGeometryParameters
    {
        [Range(0.1f, 10)]
        public float width;

        [Range(0, 360)]
        public float rotation;

        public Vector3 normal;

        public static TapeGeometryParameters Default => new()
        {
            width = 1,
            rotation = 0,
            normal = Vector3.back
        };

        public override int GetHashCode()
        {
            return width.GetHashCode() ^ rotation.GetHashCode() ^ normal.GetHashCode();
        }
    }

    public static void GenerateTapeGeometry(Mesh mesh, Vector3[] positions, TapeGeometryParameters parameters)
    {
        var width = parameters.width;
        var turn = parameters.rotation / 350;
        var parameterNormal = parameters.normal;

        if (parameterNormal.sqrMagnitude < 0.01f)
            parameterNormal = Vector3.back;

        var vertices = new Vector3[positions.Length * 2];
        var uv = new Vector2[positions.Length * 2];
        var triangles = new int[(positions.Length - 1) * 6];

        var min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        var max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

        for (var i = 0; i < positions.Length; i++)
        {
            var point = positions[i];
            var tangent = i == 0
                ? positions[i + 1] - positions[i]
                : i < positions.Length - 1
                    ? positions[i + 1] - positions[i - 1]
                    : positions[i] - positions[i - 1];
            var normal = Vector3.Cross(tangent, parameterNormal).normalized;

            for (var j = 0; j < 2; j++)
            {
                var offset = (j == 0 ? -width : width) * 0.5f * normal;

                var index = i * 2 + j;
                vertices[index] = point + offset;
                uv[index] = new Vector2((float)i / (positions.Length - 1), j);

                min = Vector3.Min(min, vertices[index]);
                max = Vector3.Max(max, vertices[index]);
            }

            if (i < positions.Length - 1)
            {
                var a = i * 2 + 0;
                var b = i * 2 + 1;
                var c = (i + 1) * 2 + 0;
                var d = (i + 1) * 2 + 1;

                var index = i * 6;

                triangles[index + 0] = a;
                triangles[index + 1] = b;
                triangles[index + 2] = c;

                triangles[index + 3] = c;
                triangles[index + 4] = b;
                triangles[index + 5] = d;
            }
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.bounds = new Bounds((min + max) * 0.5f, max - min);
        mesh.RecalculateNormals();
    }

    [System.Serializable]
    public struct TubeGeometryParameters
    {
        [Range(0.1f, 10)]
        public float radius;

        [Range(2, 64)]
        public int count;

        [Range(0, 360)]
        public float rotation;

        public Vector3 normal;
    }

    public static void GenerateTubeGeometry(Mesh mesh, Vector3[] positions, TubeGeometryParameters parameters)
    {
        var radius = parameters.radius;
        var count = parameters.count;
        var turn = parameters.rotation / 350;
        var parameterNormal = parameters.normal;

        if (parameterNormal.sqrMagnitude < 0.01f)
            parameterNormal = Vector3.back;

        var vertices = new Vector3[positions.Length * (count + 1)];
        var uv = new Vector2[positions.Length * (count + 1)];
        var triangles = new int[(positions.Length - 1) * 6 * count];

        var min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        var max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

        for (var i = 0; i < positions.Length; i++)
        {
            var point = positions[i];
            var tangent = i == 0
                ? positions[i + 1] - positions[i]
                : i < positions.Length - 1
                    ? positions[i + 1] - positions[i - 1]
                    : positions[i] - positions[i - 1];
            var normal = Vector3.Cross(tangent, parameterNormal).normalized;
            var binormal = Vector3.Cross(tangent, normal).normalized;

            for (var j = 0; j <= count; j++)
            {
                var angle = Mathf.PI * 2 * ((float)j / count + turn);
                var x = Mathf.Cos(angle) * radius;
                var y = Mathf.Sin(angle) * radius;
                var offset = normal * x + binormal * y;

                var index = i * (count + 1) + j;
                vertices[index] = point + offset;
                uv[index] = new Vector2(i / (float)positions.Length, j / (float)count);

                min = Vector3.Min(min, vertices[index]);
                max = Vector3.Max(max, vertices[index]);
            }

            if (i < positions.Length - 1)
            {
                for (var j = 0; j < count; j++)
                {
                    var a = i * (count + 1) + j;
                    var b = i * (count + 1) + j + 1;
                    var c = (i + 1) * (count + 1) + j;
                    var d = (i + 1) * (count + 1) + j + 1;

                    var index = (i * count + j) * 6;

                    triangles[index + 0] = a;
                    triangles[index + 1] = b;
                    triangles[index + 2] = c;

                    triangles[index + 3] = c;
                    triangles[index + 4] = b;
                    triangles[index + 5] = d;
                }
            }
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.bounds = new Bounds((min + max) * 0.5f, max - min);
        mesh.RecalculateNormals();
    }
}
