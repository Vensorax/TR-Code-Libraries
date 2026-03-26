// MIT License

// Copyright (c) 2026 Samuel Nchinda

using System;

namespace TeamRadiance.Science;

/// <summary>
/// A collection of scientifically-accurate mathematical and physical constants.
/// </summary>
public static class MathConstants
{
    /// <summary>
    /// The constant for Earth's gravity, 9.81 m/s^2. A beautiful foundation for many physics problems from
    /// basic kinematics to pulleys and more. Use this for help when building your own physics engine or anything else!
    /// </summary>
    public const double Gravity = 9.80665;

    /// <summary>
    /// TAU, or 2 * pi, is used to represent a circle's circumference to its radius.
    /// Used in modern trigonometry and graphics to represent a full 360-degree rotation in radians..
    /// </summary>
    public const double Tau = 2.0 * Math.PI;

    /// <summary>
    /// The vacuum permittivity (epsilon naught). A crucial constant in electromagnetism.
    /// </summary>
    public const double VacuumPermittivity = 8.8541878128e-12;

    /// <summary>
    /// Avogadro's Number, the mole! It's over 23 digits long, and it's used for stoichiometry problems.
    /// It's here for anyone who needs it for chemistry simulations or scientific projects. Maybe you can
    /// make a stoichiometry calculator?
    /// </summary>
    public const double Mol = 6.02214076e23;

    /// <summary>
    /// The Universal Gravitational Constant, used for calculating the gravitational force between two objects.
    /// If you plan to make a galaxy based game, you may need this! This number only works in this universe, so
    /// go crazy with it for other universes.
    /// </summary>
    public const double UniversalGravitationalConstant = 6.67430e-11;

    public const float Epsilon = 1e-06f;
}