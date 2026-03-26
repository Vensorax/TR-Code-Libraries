// MIT License
// Copyright (c) 2026 Samuel Nchinda

using System;
using System.Runtime.CompilerServices;
using static TeamRadiance.Science.Trig;
using static TeamRadiance.Science.Calculus;

namespace TeamRadiance.Science;

/// <summary>
/// The physics class comes packed with many commonly used physics formulas that you may need in order to conduct
/// simulations, predictions and more. Team Radiance being a team of undergraduate and high school engineers/physics
/// students means that there is no shortage of important formulas in this class!
/// </summary>
public static class Physics
{
    private const double Deg2Rad = Math.PI / 180.0;

    /// <summary>
    /// Calculates the magnitude of force using Newton's second law of motion.
    /// F = ma
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double CalculateForceMagnitude(double mass, double acceleration) => mass * acceleration;

    /// <summary>
    /// Calculates the final velocity of an object given a constant acceleration.
    /// v_f = v_i + a * t
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double CalculateFinalVelocity(double initialVelocity, double acceleration, double time) => initialVelocity + acceleration * time;

    /// <summary>
    /// Calculates the instantaneous velocity of an object at a given time using its position function.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double CalculateInstantVelocity(Func<double, double> positionFunction, double time) => Differentiate(positionFunction, time);

    /// <summary>
    /// Calculates the total displacement of an object from its velocity function over a period of time.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double CalculateDisplacement(Func<double, double> velocityFunction, double startTime, double endTime) => Integrate(velocityFunction, startTime, endTime);

    /// <summary>
    /// Calculates the final position of an object under constant acceleration.
    /// x_f = x_i + v_i * t + 0.5 * a * t^2
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double CalculateFinalPosition(double initialPosition, double initialVelocity, double acceleration, double time) => 
        initialPosition + initialVelocity * time + 0.5 * (acceleration * time * time);

    /// <summary>
    /// Calculates the total horizontal distance a projectile travels.
    /// R = (v^2 * sin(2*theta)) / g
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double CalculateProjectileRange(double initialVelocity, double angleInDegrees)
    {
        double angleInRadians = Trig.NormalizeDegrees((float)angleInDegrees) * Deg2Rad;
        return initialVelocity * initialVelocity * Math.Sin(2.0 * angleInRadians) / MathConstants.Gravity;
    }

    /// <summary>
    /// Calculates the work done by a force on an object.
    /// W = F * d * cos(theta)
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double CalculateWork(double force, double distance, double angleInDegrees = 0)
    {
        double angleInRadians = Trig.NormalizeDegrees((float)angleInDegrees) * Deg2Rad;
        return force * distance * Math.Cos(angleInRadians);
    }

    /// <summary>
    /// Calculates the magnitude of the drag force on an object.
    /// F_d = 0.5 * p * C_d * A * v^2
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double CalculateDragForce(double airDensity, double dragCoefficient, double crossSectionalArea, double velocity) => 
        0.5 * airDensity * dragCoefficient * crossSectionalArea * velocity * velocity;
}