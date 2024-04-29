//
// Orbit.cs
//
// Copyright (c) 2005-2021 Michael F. Henry
// Version 09/2021
//
using System;

namespace Zeptomoby.OrbitTools
{
   /// <summary>
   /// This class accepts a single satellite's NORAD two-line element
   /// set and provides information regarding the satellite's orbit 
   /// such as period, axis length, ECI coordinates, velocity, etc.
   /// </summary>
   public class Orbit
   {
      #region Properties

      public OrbitalElements Elements { get; set; }

      public Julian   Epoch     { get { return Elements.Epoch; } }
      public DateTime EpochTime { get { return Epoch.ToTime(); } }

      private NoradBase NoradModel { get; set; }

      // "Recovered" from the input orbital elements
      public double SemiMajorRec  { get; private set; }
      public double SemiMinorRec  { get; private set; }
      public double MajorRec      { get { return 2.0 * SemiMajorRec; } }
      public double MinorRec      { get { return 2.0 * SemiMinorRec; } }
      public double MeanMotionRec { get; private set; }  // radians per min
      public double PerigeeKmRec  { get; private set; }
      public double ApogeeKmRec   { get; private set; }

      public string SatName       { get { return Elements.SatelliteName;      } }
      public string SatNameLong   { get { return SatName + " #" + SatNoradId; } }
      public string SatNoradId    { get { return Elements.NoradIdStr;         } }
      public string SatDesignator { get { return Elements.IntlDesignatorStr;  } }

      /// <summary>
      /// The orbital period.
      /// </summary>
      public TimeSpan Period 
      {
         get 
         { 
            if (m_Period == null)
            {
               // Calculate the period using the recovered mean motion
               if (MeanMotionRec == 0)
               {
                  m_Period = new TimeSpan(0, 0, 0);
               }
               else
               {
                  double secs  = (Globals.TwoPi / MeanMotionRec) * 60.0;
                  int    msecs = (int)((secs - (int)secs) * 1000);

                  m_Period = new TimeSpan(0, 0, 0, (int)secs, msecs);
               }
            }

            return m_Period.Value;
         }
      }
      private TimeSpan? m_Period;

      #endregion

      #region Construction

      /// <summary>
      /// Standard constructor.
      /// </summary>
      /// <param name="elements">Orbital elements.</param>
      public Orbit(OrbitalElements elements)
      {
         Elements = elements;

         // Recover the original mean motion and semi-major axis from the orbital elements.
         double mm     = Elements.MeanMotion * Globals.TwoPi / Globals.MinPerDay; // rads per min
         double a1     = Math.Pow(Globals.Xke / mm, 2.0 / 3.0);
         double e      = Elements.Eccentricity;
         double i      = Elements.InclinationRad;
         double temp   = (1.5 * Globals.Ck2 * (3.0 * Globals.Sqr(Math.Cos(i)) - 1.0) / 
                         Math.Pow(1.0 - e * e, 1.5));   
         double delta1 = temp / (a1 * a1);
         double a0     = a1 * 
                        (1.0 - delta1 * 
                        ((1.0 / 3.0) + delta1 * 
                        (1.0 + 134.0 / 81.0 * delta1)));

         double delta0 = temp / (a0 * a0);

         MeanMotionRec = mm / (1.0 + delta0);
         SemiMajorRec  = a0 / (1.0 - delta0);
         SemiMinorRec  = SemiMajorRec * Math.Sqrt(1.0 - (e * e));
         PerigeeKmRec  = Globals.Xkmper * (SemiMajorRec * (1.0 - e) - Globals.Ae);
         ApogeeKmRec   = Globals.Xkmper * (SemiMajorRec * (1.0 + e) - Globals.Ae);

         if (Period.TotalMinutes >= 225.0)
         {
            // SDP4 - period >= 225 minutes.
            NoradModel = new NoradSDP4(this);
         }
         else
         {
            // SGP4 - period < 225 minutes
            NoradModel = new NoradSGP4(this);
         }
      }

      #endregion

      #region Position

      /// <summary>
      /// Calculate satellite ECI position/velocity for a given time.
      /// </summary>
      /// <param name="mpe">Target time, in minutes-past-epoch.</param>
      /// <returns>Kilometer-based position/velocity ECI coordinates.</returns>
      public EciTime PositionEci(double mpe)
      {
         EciTime eci = NoradModel.GetPosition(mpe);

         // Convert ECI vector units from AU to kilometers
         double radiusAe = Globals.Xkmper / Globals.Ae;

         eci.ScalePosVector(radiusAe);                               // km
         eci.ScaleVelVector(radiusAe * (Globals.MinPerDay / 86400)); // km/sec

         return eci;
      }

      /// <summary>
      /// Calculate ECI position/velocity for a given time.
      /// </summary>
      /// <param name="utc">Target time (UTC).</param>
      /// <returns>Kilometer-based position/velocity ECI coordinates.</returns>
      public EciTime PositionEci(DateTime utc)
      {
         return PositionEci(TPlusEpoch(utc).TotalMinutes);
      }

      #endregion

      // ///////////////////////////////////////////////////////////////////////////
      // Returns elapsed time from epoch to given time.
      // Note: "Predicted" TLEs can have epochs in the future.
      public TimeSpan TPlusEpoch(DateTime utc) 
      {
         return (utc - EpochTime);
      }

      // ///////////////////////////////////////////////////////////////////////////
      // Returns elapsed time from epoch to current time.
      // Note: "Predicted" TLEs can have epochs in the future.
      public TimeSpan TPlusEpoch()
      {
         return TPlusEpoch(DateTime.UtcNow);
      }
   }
}
