using UnityEngine;

namespace ECM2
{
    public static class MathLib
    {
        /// <summary>
        /// Returns Value mapped from one range into another.
        /// </summary>

        public static float Remap(float inA, float inB, float outA, float outB, float value)
        {
            float t = Mathf.InverseLerp(inA, inB, value);

            return Mathf.Lerp(outA, outB, t);
        }
        
        /// <summary>
        /// Return the square of the given value.
        /// </summary>

        public static float Square(float value)
        {
            return value * value;
        }

        /// <summary>
        /// Returns the direction adjusted to be tangent to a specified surface normal relatively to given up axis.
        /// </summary>
        
        public static Vector3 GetTangent(Vector3 direction, Vector3 normal, Vector3 up)
        {
            Vector3 right = direction.perpendicularTo(up);

            return normal.perpendicularTo(right);
        }

        /// <summary>
        /// Projects a given point onto the plane defined by plane origin and plane normal.
        /// </summary>

        public static Vector3 ProjectPointOnPlane(Vector3 point, Vector3 planeOrigin, Vector3 planeNormal)
        {
            Vector3 toPoint = point - planeOrigin;
            Vector3 toPointProjected = Vector3.Project(toPoint, planeNormal);

            return point - toPointProjected;
        }
        
        /// <summary>
        /// Clamps given angle within min - max range.
        /// </summary>
        
        public static float ClampAngle(float a, float min, float max)
        {
            while (max < min)
                max += 360.0f;

            while (a > max)
                a -= 360.0f;

            while (a < min)
                a += 360.0f;

            return a > max ? a - (max + min) * 0.5f < 180.0f ? max : min : a;
        }
        
        /// <summary>
        /// Returns Angle in the range (0, 360)
        /// </summary>

        public static float ClampAngle(float angle)
        {
            // returns angle in the range (-360, 360)

            angle = angle % 360.0f;

            if (angle < 0.0f)
            {
                // shift to (0, 360) range

                angle += 360.0f;
            }

            return angle;
        }
        
        /// <summary>
        /// Return angle in range -180 to 180
        /// </summary>

        public static float NormalizeAngle(float angle)
        {
            // returns angle in the range (0, 360)

            angle = ClampAngle(angle);

            if (angle > 180.0f)
            {
                // shift to (-180,180)

                angle -= 360.0f;
            }

            return angle;
        }

        /// <summary>
        /// Clamps the given angle into 0 - 360 degrees range.
        /// </summary>
        
        private static float Clamp0360(float eulerAngles)
        {
            float result = eulerAngles - Mathf.CeilToInt(eulerAngles / 360f) * 360f;
            if (result < 0) result += 360f;

            return result;
        }

        /// <summary>
        /// Returns a new rotation angle (interpolated) clamped in the range (0.0f , 360.0f)
        /// </summary>

        public static float FixedTurn(float current, float target, float maxDegreesDelta)
        {
            if (maxDegreesDelta == 0.0f)
                return Clamp0360(current);

            if (maxDegreesDelta >= 360.0f)
                return Clamp0360(target);

            float result = Clamp0360(current);
            current = result;
            target = Clamp0360(target);

            if (current > target)
            {
                if (current - target < 180.0f)
                    result -= Mathf.Min(current - target, Mathf.Abs(maxDegreesDelta));
                else
                    result += Mathf.Min(target + 360.0f - current, Mathf.Abs(maxDegreesDelta));
            }
            else
            {
                if (target - current < 180.0f)
                    result += Mathf.Min(target - current, Mathf.Abs(maxDegreesDelta));
                else
                    result -= Mathf.Min(current + 360.0f - target, Mathf.Abs(maxDegreesDelta));
            }

            return Clamp0360(result);
        }
        
        /// <summary>
        /// Frame Rate Independent Damping.
        /// Source: https://www.rorydriscoll.com/2016/03/07/frame-rate-independent-damping-using-lerp/
        /// </summary>
        
        public static float Damp(float a, float b, float lambda, float dt)
        {
            return Mathf.Lerp(a, b, 1.0f - Mathf.Exp(-lambda * dt));
        }
        
        /// <summary>
        /// Frame Rate Independent Damping.
        /// Source: https://www.rorydriscoll.com/2016/03/07/frame-rate-independent-damping-using-lerp/
        /// </summary>
        
        public static Vector3 Damp(Vector3 a, Vector3 b, float lambda, float dt)
        {
            return Vector3.Lerp(a, b, 1.0f - Mathf.Exp(-lambda * dt));
        }
    }
}