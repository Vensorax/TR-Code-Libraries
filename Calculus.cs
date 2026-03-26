// MIT License
// Copyright (c) 2026 Samuel Nchinda

using System;
using System.Runtime.CompilerServices;

namespace TeamRadiance.Science;

/// <summary>
/// Anyone likes calculus? The beautiful subject of math where curves all of a sudden go from confusing
/// to solvable, finding the area and slopes of irregular shapes by infinitely zooming in on said stuff
/// and finding the solution. That being said this is also when many math students begin to question the
/// meaning of life as a whole.
/// </summary>
public static class Calculus
{
    /// <summary>
    /// Finds the slope of a function at point x. For many people, this is when the limit definition
    /// was last used, and derivatives made their grand entrance.
    /// </summary>
    /// <param name="f">The function to differentiate.</param>
    /// <param name="x">The x value, or variable in general, you want to find the point at.</param>
    /// <returns>The derivative at the point.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Differentiate(Func<float, float> f, float x)
    {
        float h = 1e-4f;
        return (f(x + h) - f(x - h)) / (2f * h);
    }

    /// <summary>
    /// Finds the slope of a function at point x. For many people, this is when the limit definition
    /// was last used, and derivatives made their grand entrance.
    /// </summary>
    /// <param name="f">The function to differentiate.</param>
    /// <param name="x">The x value, or variable in general, you want to find the point at.</param>
    /// <returns>The derivative at the point.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Differentiate(Func<double, double> f, double x)
    {
        double h = 1e-7; // 🚀 FIX: Removed the 'f' and increased precision for doubles!
        return (f(x + h) - f(x - h)) / (2.0 * h);
    }

    /// <summary>
    /// Finds the area under a curve of a function between points a and b.
    /// </summary>
    /// <param name="f">The function to integrate.</param>
    /// <param name="a">The lower limit.</param>
    /// <param name="b">The upper limit.</param>
    /// <param name="intervals">How many slices you want to integrate. The higher the number, the more accurate the area.</param>
    /// <returns>The area under the curve.</returns>
    public static float Integrate(Func<float, float> f, float a, float b, int intervals = 1000)
    {
        float step = (b - a) / intervals;
        float integral = 0;
        for (int i = 1; i < intervals; i++)
        {
            float x = a + i * step;
            integral += f(x);
        }
        integral += (f(a) + f(b)) / 2f;
        return integral * step;
    }

    /// <summary>
    /// Finds the area under a curve of a function between points a and b.
    /// </summary>
    /// <param name="f">The function to integrate.</param>
    /// <param name="a">The lower limit.</param>
    /// <param name="b">The upper limit.</param>
    /// <param name="intervals">How many slices you want to integrate. The higher the number, the more accurate the area.</param>
    /// <returns>The area under the curve.</returns>
    public static double Integrate(Func<double, double> f, double a, double b, int intervals = 1000)
    {
        double step = (b - a) / intervals;
        double integral = 0;
        for (int i = 1; i < intervals; i++)
        {
            double x = a + i * step;
            integral += f(x);
        }
        integral += (f(a) + f(b)) / 2.0;
        return integral * step;
    }
}