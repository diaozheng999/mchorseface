using UnityEngine;


namespace McHorseface.LawnDart
{
    static class Extensions
    {
        /// <summary>
        /// Averages two degree vectors and returns an averaged degree vector.
        /// </summary>
        /// <param name="me">Vector of degrees</param>
        /// <param name="other">Vector of degrees</param>
        /// <param name="weight">Weights of the second vector</param>
        /// <returns>Vector of degrees</returns>
        public static Vector3 WeightedAvgAngle(this Vector3 me, Vector3 other, float weight = 1)
        {
            return me.WeightedAvgAngle(other, Vector3.one * weight);
        }

        /// <summary>
        /// Averages two degree vectors and returns an averaged degree vector.
        /// </summary>
        /// <param name="me">Vector of degrees</param>
        /// <param name="other">Vector of degrees</param>
        /// <param name="weight">Weights of the second vector</param>
        /// <returns>Vector of degrees</returns>
        public static Vector3 WeightedAvgAngle(this Vector3 me, Vector3 other, Vector3 weight)
        {
            var me_rad = me * Mathf.Deg2Rad;
            var other_rad = other * Mathf.Deg2Rad;

            var sin = new Vector3(
                (Mathf.Sin(me_rad.x) + weight.x * Mathf.Sin(other_rad.x)) / (1 + weight.x),
                (Mathf.Sin(me_rad.y) + weight.y * Mathf.Sin(other_rad.y)) / (1 + weight.y),
                (Mathf.Sin(me_rad.z) + weight.z * Mathf.Sin(other_rad.z)) / (1 + weight.z)
            );

            var cos = new Vector3(
                (Mathf.Cos(me_rad.x) + weight.x * Mathf.Cos(other_rad.x)) / (1 + weight.x),
                (Mathf.Cos(me_rad.y) + weight.y * Mathf.Cos(other_rad.y)) / (1 + weight.y),
                (Mathf.Cos(me_rad.z) + weight.z * Mathf.Cos(other_rad.z)) / (1 + weight.z)
            );

            return new Vector3(
                Mathf.Atan2(sin.x, cos.x),
                Mathf.Atan2(sin.y, cos.y),
                Mathf.Atan2(sin.z, cos.z)
            ) * Mathf.Rad2Deg;
        }
    }
}
