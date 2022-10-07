using System;
using System.Collections.Generic;
using System.Text;
namespace UnityEngine
{
    //
    // Summary:
    //     Representation of 3D vectors and points.
    public struct Vector3 : IEquatable<Vector3>
    {
        public const float kEpsilon = 1E-05F;
        public const float kEpsilonNormalSqrt = 1E-15F;
        //
        // Summary:
        //     X component of the vector.
        public float x;
        //
        // Summary:
        //     Y component of the vector.
        public float y;
        //
        // Summary:
        //     Z component of the vector.
        public float z;

        //
        // Summary:
        //     Creates a new vector with given x, y components and sets z to zero.
        //
        // Parameters:
        //   x:
        //
        //   y:
        public Vector3(float x, float y)
        {
            this.x = x;
            this.y = y;
            z = 0;
        }
        //
        // Summary:
        //     Creates a new vector with given x, y, z components.
        //
        // Parameters:
        //   x:
        //
        //   y:
        //
        //   z:
        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public float this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return x;
                    case 1:
                        return y;
                    case 2:
                        return z;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        x = value;
                        break;
                    case 1:
                        y = value;
                        break;
                    case 2:
                        z = value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        //
        // Summary:
        //     Shorthand for writing Vector3(0, 0, 0).
        public static Vector3 zero { get; }
        //
        // Summary:
        //     Shorthand for writing Vector3(1, 1, 1).
        public static Vector3 one { get; } = new Vector3(1, 1, 1);
        //
        // Summary:
        //     Shorthand for writing Vector3(0, 0, 1).
        public static Vector3 forward { get; } = new Vector3(0, 0, 1);
        //
        // Summary:
        //     Shorthand for writing Vector3(0, 0, -1).
        public static Vector3 back { get; } = new Vector3(0, 0, -1);
        //
        // Summary:
        //     Shorthand for writing Vector3(1, 0, 0).
        public static Vector3 right { get; } = new Vector3(1, 0, 0);
        //
        // Summary:
        //     Shorthand for writing Vector3(0, -1, 0).
        public static Vector3 down { get; }
        //
        // Summary:
        //     Shorthand for writing Vector3(-1, 0, 0).
        public static Vector3 left { get; }
        //
        // Summary:
        //     Shorthand for writing Vector3(float.PositiveInfinity, float.PositiveInfinity,
        //     float.PositiveInfinity).
        public static Vector3 positiveInfinity { get; }
        //
        // Summary:
        //     Shorthand for writing Vector3(0, 1, 0).
        public static Vector3 up { get; }
        //
        // Summary:
        //     Shorthand for writing Vector3(float.NegativeInfinity, float.NegativeInfinity,
        //     float.NegativeInfinity).
        public static Vector3 negativeInfinity { get; }
        [Obsolete("Use Vector3.forward instead.")]
        public static Vector3 fwd { get; }
        //
        // Summary:
        //     Returns the squared length of this vector (Read Only).
        public float sqrMagnitude { get => 0; }
        //
        // Summary:
        //     Returns this vector with a magnitude of 1 (Read Only).
        public Vector3 normalized { get => this; }
        //
        // Summary:
        //     Returns the length of this vector (Read Only).
        public float magnitude { get => 0; }

        //
        // Summary:
        //     Returns the angle in degrees between from and to.
        //
        // Parameters:
        //   from:
        //     The vector from which the angular difference is measured.
        //
        //   to:
        //     The vector to which the angular difference is measured.
        //
        // Returns:
        //     The angle in degrees between the two vectors.
        public static float Angle(Vector3 from, Vector3 to) => 0;
        [Obsolete("Use Vector3.Angle instead. AngleBetween uses radians instead of degrees and was deprecated for this reason")]
        public static float AngleBetween(Vector3 from, Vector3 to) => 0;
        //
        // Summary:
        //     Returns a copy of vector with its magnitude clamped to maxLength.
        //
        // Parameters:
        //   vector:
        //
        //   maxLength:
        public static Vector3 ClampMagnitude(Vector3 vector, float maxLength) => vector;
        //
        // Summary:
        //     Cross Product of two vectors.
        //
        // Parameters:
        //   lhs:
        //
        //   rhs:
        public static Vector3 Cross(Vector3 lhs, Vector3 rhs) => lhs;
        //
        // Summary:
        //     Returns the distance between a and b.
        //
        // Parameters:
        //   a:
        //
        //   b:
        public static float Distance(Vector3 a, Vector3 b) => 0;
        //
        // Summary:
        //     Dot Product of two vectors.
        //
        // Parameters:
        //   lhs:
        //
        //   rhs:
        public static float Dot(Vector3 lhs, Vector3 rhs) => 0;
        [Obsolete("Use Vector3.ProjectOnPlane instead.")]
        public static Vector3 Exclude(Vector3 excludeThis, Vector3 fromThat) => default;
        //
        // Summary:
        //     Linearly interpolates between two vectors.
        //
        // Parameters:
        //   a:
        //
        //   b:
        //
        //   t:
        public static Vector3 Lerp(Vector3 a, Vector3 b, float t) => default;
        //
        // Summary:
        //     Linearly interpolates between two vectors.
        //
        // Parameters:
        //   a:
        //
        //   b:
        //
        //   t:
        public static Vector3 LerpUnclamped(Vector3 a, Vector3 b, float t) => default;
        public static float Magnitude(Vector3 vector) => 0;
        //
        // Summary:
        //     Returns a vector that is made from the largest components of two vectors.
        //
        // Parameters:
        //   lhs:
        //
        //   rhs:
        public static Vector3 Max(Vector3 lhs, Vector3 rhs) => default;
        //
        // Summary:
        //     Returns a vector that is made from the smallest components of two vectors.
        //
        // Parameters:
        //   lhs:
        //
        //   rhs:
        public static Vector3 Min(Vector3 lhs, Vector3 rhs) => default;
        //
        // Summary:
        //     Calculate a position between the points specified by current and target, moving
        //     no farther than the distance specified by maxDistanceDelta.
        //
        // Parameters:
        //   current:
        //     The position to move from.
        //
        //   target:
        //     The position to move towards.
        //
        //   maxDistanceDelta:
        //     Distance to move current per call.
        //
        // Returns:
        //     The new position.
        public static Vector3 MoveTowards(Vector3 current, Vector3 target, float maxDistanceDelta) => default;
        //
        // Summary:
        //     Makes this vector have a magnitude of 1.
        //
        // Parameters:
        //   value:
        public static Vector3 Normalize(Vector3 value) => default;
        public static void OrthoNormalize(ref Vector3 normal, ref Vector3 tangent, ref Vector3 binormal) { }
        public static void OrthoNormalize(ref Vector3 normal, ref Vector3 tangent) { }
        //
        // Summary:
        //     Projects a vector onto another vector.
        //
        // Parameters:
        //   vector:
        //
        //   onNormal:
        public static Vector3 Project(Vector3 vector, Vector3 onNormal) => default;
        //
        // Summary:
        //     Projects a vector onto a plane defined by a normal orthogonal to the plane.
        //
        // Parameters:
        //   planeNormal:
        //     The direction from the vector towards the plane.
        //
        //   vector:
        //     The location of the vector above the plane.
        //
        // Returns:
        //     The location of the vector on the plane.
        public static Vector3 ProjectOnPlane(Vector3 vector, Vector3 planeNormal)=>default;
        //
        // Summary:
        //     Reflects a vector off the plane defined by a normal.
        //
        // Parameters:
        //   inDirection:
        //
        //   inNormal:
        public static Vector3 Reflect(Vector3 inDirection, Vector3 inNormal) => default;
        //
        // Summary:
        //     Rotates a vector current towards target.
        //
        // Parameters:
        //   current:
        //     The vector being managed.
        //
        //   target:
        //     The vector.
        //
        //   maxRadiansDelta:
        //     The distance between the two vectors in radians.
        //
        //   maxMagnitudeDelta:
        //     The length of the radian.
        //
        // Returns:
        //     The location that RotateTowards generates.
        public static Vector3 RotateTowards(Vector3 current, Vector3 target, float maxRadiansDelta, float maxMagnitudeDelta) => default;
        //
        // Summary:
        //     Multiplies two vectors component-wise.
        //
        // Parameters:
        //   a:
        //
        //   b:
        public static Vector3 Scale(Vector3 a, Vector3 b) => default;
        //
        // Summary:
        //     Returns the signed angle in degrees between from and to.
        //
        // Parameters:
        //   from:
        //     The vector from which the angular difference is measured.
        //
        //   to:
        //     The vector to which the angular difference is measured.
        //
        //   axis:
        //     A vector around which the other vectors are rotated.
        public static float SignedAngle(Vector3 from, Vector3 to, Vector3 axis) => 0;
        //
        // Summary:
        //     Spherically interpolates between two vectors.
        //
        // Parameters:
        //   a:
        //
        //   b:
        //
        //   t:
        public static Vector3 Slerp(Vector3 a, Vector3 b, float t) => default;
        //
        // Summary:
        //     Spherically interpolates between two vectors.
        //
        // Parameters:
        //   a:
        //
        //   b:
        //
        //   t:
        public bool Equals(Vector3 other) => false;
        //
        // Summary:
        //     Returns true if the given vector is exactly equal to this vector.
        //
        // Parameters:
        //   other:
        public override bool Equals(object other) => false;
        public void Normalize() { }
        //
        // Summary:
        //     Multiplies every component of this vector by the same component of scale.
        //
        // Parameters:
        //   scale:
        public void Scale(Vector3 scale) { }
        //
        // Summary:
        //     Set x, y and z components of an existing Vector3.
        //
        // Parameters:
        //   newX:
        //
        //   newY:
        //
        //   newZ:
        public void Set(float newX, float newY, float newZ) { }
        //
        // Summary:
        //     Returns a nicely formatted string for this vector.
        //
        // Parameters:
        //   format:
        public string ToString(string format)
        {
            return $"({x},{y},{z})";
        }
        //
        // Summary:
        //     Returns a nicely formatted string for this vector.
        //
        // Parameters:
        //   format:
        public override string ToString()
        {
            return $"({x},{y},{z})";
        }

        public static Vector3 operator +(Vector3 a, Vector3 b)
        {
            return a;
        }
        public static Vector3 operator -(Vector3 a)
        {
            return a;
        }
        public static Vector3 operator -(Vector3 a, Vector3 b)
        {
            return a;
        }
        public static Vector3 operator *(Vector3 a, float d)
        {
            return a;
        }
        public static Vector3 operator *(float d, Vector3 a)
        {
            return a;
        }
        public static Vector3 operator /(Vector3 a, float d)
        {
            return a;
        }
        public static bool operator ==(Vector3 lhs, Vector3 rhs)
        {
            return false;
        }
        public static bool operator !=(Vector3 lhs, Vector3 rhs)
        {
            return true;
        }
    }
}