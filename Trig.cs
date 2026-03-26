// MIT License

// Copyright (c) 2026 Samuel Nchinda

using System;

namespace TeamRadiance.Science;

/// <summary>
/// 
/// </summary>
public static class Trig
{
    /// <summary>
    /// Cosecant, 1 over sine.
    /// </summary>
    /// <param name="x"></param>
    /// <returns>The value of cosecant x.</returns>
    public static float Csc(this float x) => 1 / MathF.Sin(x);

    /// <summary>
    /// Secant, 1 over x.
    /// </summary>
    /// <param name="x">The x value to parse through.</param>
    /// <returns>The value of 1 over cosine.</returns>
    public static float Sec(this float x) => 1 / MathF.Cos(x);

    /// <summary>
    /// Returns 1 over cotangent.
    /// </summary>
    /// <param name="x">The x value to parse through.</param>
    /// <returns>The value of 1 over cotangent.</returns>
    public static float Cot(this float x) => 1 / MathF.Tan(x);

    // Documentation isn't finished!

    /// <summary>
    /// 
    /// </summary>
    /// <param name="degrees"></param>
    /// <returns></returns>
    public static float DegreesToRadians(this float degrees) => degrees * MathF.PI / 180f;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="radians"></param>
    /// <returns></returns>
    public static float RadiansToDegrees(this float radians) => radians * 180f / MathF.PI;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="radians"></param>
    /// <returns></returns>
    public static float WrapAngleRadians(this float radians) => (radians % (2 * MathF.PI) + 2 * MathF.PI) % (2 * MathF.PI);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="degrees"></param>
    /// <returns></returns>
    public static float WrapAngleDegrees(this float degrees) => (degrees % 360f + 360f) % 360f;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="radius"></param>
    /// <param name="angleRadians"></param>
    /// <returns></returns>
    public static (float x, float y) PolarToCartesian(this float radius, float angleRadians) => (radius * MathF.Cos(angleRadians), radius * MathF.Sin(angleRadians));

    /// <summary>
    /// 
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public static (float radius, float angleRadians) CartesianToPolar(this float x, float y)
    {
        float radius = MathF.Sqrt(x * x + y * y);
        float angleRadians = MathF.Atan2(y, x);
        return (radius, angleRadians);
    }

    /// <summary>
    /// Normalizes an angle to be between -PI and PI.
    /// </summary>
    public static float NormalizeRadians(this float radians) 
        => WrapAngleRadians(radians + MathF.PI) - MathF.PI;

    /// <summary>
    /// Normalizes an angle to be between -180 and 180 degrees.
    /// </summary>
    public static float NormalizeDegrees(this float degrees) 
        => WrapAngleDegrees(degrees + 180f) - 180f;
}
