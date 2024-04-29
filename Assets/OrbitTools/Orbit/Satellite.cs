//
// Satellite.cs
//
// Copyright (c) 2014-2021 Michael F. Henry
// Version 09/2021
//
using System;

namespace Zeptomoby.OrbitTools
{
   /// <summary>
   /// Class to encapsulate a satellite.
   /// </summary>
   public class Satellite
   {
      #region Properties

      /// <summary>
      /// The satellite name.
      /// </summary>
      public string Name { get; private set; }

      /// <summary>
      /// Information related to the satellite's orbit.
      /// </summary>
      public Orbit Orbit { get; private set; }

      #endregion

      #region Construction

      /// <summary>
      /// Standard constructor.
      /// </summary>
      /// <param name="tle">TLE data.</param>
      /// <param name="name">Optional satellite name.</param>
      public Satellite(TwoLineElements tle, string name = null)
      {
         Orbit = new Orbit(tle);
         Name = name ?? Orbit.SatName;
      }

      /// <summary>
      /// Constructor accepting an Orbit object.
      /// </summary>
      /// <param name="orbit">The satellite's orbit.</param>
      /// <param name="name">Optional satellite name.</param>
      public Satellite(Orbit orbit, string name = null)
      {
         Orbit = orbit;
         Name = name ?? Orbit.SatName;
      }

      #endregion

      /// <summary>
      /// Returns the ECI position of the satellite.
      /// </summary>
      /// <param name="utc">The time (UTC) of position calculation.</param>
      /// <returns>The ECI location of the satellite at the given time.</returns>
      public EciTime PositionEci(DateTime utc)
      {
         return Orbit.PositionEci(utc);
      }

      /// <summary>
      /// Returns the ECI position of the satellite.
      /// </summary>
      /// <param name="mpe">The time of position calculation, in minutes-past-epoch.</param>
      /// <returns>The ECI location of the satellite at the given time.</returns>
      public EciTime PositionEci(double mpe)
      {
         return Orbit.PositionEci(mpe);
      }
   }
}
