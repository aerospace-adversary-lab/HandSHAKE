//
// OrbitalElements.cs
//
// Copyright (c) 2021 Michael F. Henry
// Version 07/2021
//

namespace Zeptomoby.OrbitTools
{
   public abstract class OrbitalElements
   {
      public double InclinationDeg { get; protected set; }
      public double InclinationRad { get; protected set; }
      public double Eccentricity   { get; protected set; }
      public double RAANodeDeg     { get; protected set; }
      public double RAANodeRad     { get; protected set; }
      public double ArgPerigeeDeg  { get; protected set; }
      public double ArgPerigeeRad  { get; protected set; }
      public double MeanAnomalyDeg { get; protected set; }
      public double MeanAnomalyRad { get; protected set; }
      public double BStar          { get; protected set; }
      public double MeanMotion     { get; protected set; }
      public double MeanMotionDt   { get; protected set; }

      public int RevAtEpoch { get; protected set; }
      public int SetNumber  { get; protected set; }

      public string SatelliteName     { get; protected set; }
      public string NoradIdStr        { get; protected set; }
      public string IntlDesignatorStr { get; protected set; }

      public Julian Epoch { get; protected set; }
   }
}
