using UnityEngine;

namespace Core.Utils
{
    public static class Extensions
    {
        public static Vector3 CancelIfBelowThreshold(this Vector3 vector, float threshold)
        {
            return vector.sqrMagnitude > threshold * threshold ? vector : Vector3.zero;
        }
    }
}
