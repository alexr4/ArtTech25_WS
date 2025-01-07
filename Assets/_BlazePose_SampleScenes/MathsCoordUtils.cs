using UnityEngine;

namespace Bonjour
{
	public static class MathsCoordUtils
	{

        public static Vector4 RemapKeyPointScreenSpace(Vector4 vertex)
        {
            Vector4 remap = Vector4.zero;

            remap.x = vertex.x * Screen.width;
            remap.y = (1.0f - vertex.y) * Screen.height;
            remap.z = vertex.z;
            remap.w = vertex.w;
            return remap;
        }

        public static Vector4 RemapKeyPointSquareScreenSpace(Vector4 vertex)
        {
            Vector4 remap = Vector4.zero;

            remap.x = vertex.x * Screen.width;
            remap.y = (1.0f - vertex.y) * Screen.width;
            remap.z = vertex.z;
            remap.w = vertex.w;
            return remap;
        }
    }

}
