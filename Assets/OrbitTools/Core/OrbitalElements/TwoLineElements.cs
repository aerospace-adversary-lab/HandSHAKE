//
// TwoLineElements.cs
//
// Copyright (c) 2021-2022 Michael F. Henry
// Version 07/2022
//
using System;
using System.Globalization;

// ////////////////////////////////////////////////////////////////////////
//
// Two-Line Element Data format
//
// [Reference: Dr. T.S. Kelso / www.celestrak.com]
//
// Two-line element data consists of three lines in the following format:
//
// AAAAAAAAAAAAAAAAAAAAAAAA
// 1 NNNNNU NNNNNAAA NNNNN.NNNNNNNN +.NNNNNNNN +NNNNN-N +NNNNN-N N NNNNN
// 2 NNNNN NNN.NNNN NNN.NNNN NNNNNNN NNN.NNNN NNN.NNNN NN.NNNNNNNNNNNNNN
//  
// Line 0 is a twenty-four-character name.
// 
// Lines 1 and 2 are the standard Two-Line Orbital Element Set Format 
// used by NORAD and NASA. The format description is:
//      
//     Line 1
//     Column    Description
//     01-01     Line Number of Element Data
//     03-07     Satellite Number
//     10-11     International Designator (Last two digits of launch year)
//     12-14     International Designator (Launch number of the year)
//     15-17     International Designator (Piece of launch)
//     19-20     Epoch Year (Last two digits of year)
//     21-32     Epoch (Julian Day and fractional portion of the day)
//     34-43     First Time Derivative of the Mean Motion
//               or Ballistic Coefficient (Depending on ephemeris type)
//     45-52     Second Time Derivative of Mean Motion (decimal point assumed;
//               blank if N/A)
//     54-61     BSTAR drag term if GP4 general perturbation theory was used.
//               Otherwise, radiation pressure coefficient.  (Decimal point assumed)
//     63-63     Ephemeris type
//     65-68     Element number
//     69-69     Check Sum (Modulo 10)
//               (Letters, blanks, periods, plus signs = 0; minus signs = 1)
//     Line 2
//     Column    Description
//     01-01     Line Number of Element Data
//     03-07     Satellite Number
//     09-16     Inclination [Degrees]
//     18-25     Right Ascension of the Ascending Node [Degrees]
//     27-33     Eccentricity (decimal point assumed)
//     35-42     Argument of Perigee [Degrees]
//     44-51     Mean Anomaly [Degrees]
//     53-63     Mean Motion [Revs per day]
//     64-68     Revolution number at epoch [Revs]
//     69-69     Check Sum (Modulo 10)
//        
//     All other columns are blank or fixed.
//          
// Example:
//      
// ISS(ZARYA)
// 1 25544U 98067A   16362.88986010  .00002353  00000-0  43073-4 0  9992
// 2 25544  51.6423 172.2304 0006777  22.6708 127.4688 15.53951055 35119
//
namespace Zeptomoby.OrbitTools
{
   /// <summary>
   /// This class encapsulates a single set of NORAD two-line orbital elements.
   /// </summary>
   public sealed class TwoLineElements : OrbitalElements
   {
      #region Column Offsets

      // Note: Column offsets are zero-based.

      // Line 1
      private const int TLE1_COL_SATNUM = 2;         private const int TLE1_LEN_SATNUM = 5;
      private const int TLE1_COL_INTLDESC_A = 9;     private const int TLE1_LEN_INTLDESC_A = 2;
      private const int TLE1_LEN_INTLDESC_B = 3;     private const int TLE1_LEN_INTLDESC_C = 3;     
      private const int TLE1_COL_EPOCH_A = 18;       private const int TLE1_LEN_EPOCH_A = 2;
      private const int TLE1_COL_EPOCH_B = 20;       private const int TLE1_LEN_EPOCH_B = 12;
      private const int TLE1_COL_MEANMOTIONDT = 33;  private const int TLE1_LEN_MEANMOTIONDT = 10;
      private const int TLE1_COL_BSTAR = 53;         private const int TLE1_LEN_BSTAR = 8;
      private const int TLE1_COL_ELNUM = 64;         private const int TLE1_LEN_ELNUM = 4;

      // Line 2
      private const int TLE2_COL_INCLINATION = 8;    private const int TLE2_LEN_INCLINATION = 8;
      private const int TLE2_COL_RAASCENDNODE = 17;  private const int TLE2_LEN_RAASCENDNODE = 8;
      private const int TLE2_COL_ECCENTRICITY = 26;  private const int TLE2_LEN_ECCENTRICITY = 7;
      private const int TLE2_COL_ARGPERIGEE = 34;    private const int TLE2_LEN_ARGPERIGEE = 8;
      private const int TLE2_COL_MEANANOMALY = 43;   private const int TLE2_LEN_MEANANOMALY = 8;
      private const int TLE2_COL_MEANMOTION = 52;    private const int TLE2_LEN_MEANMOTION = 11;
      private const int TLE2_COL_REVATEPOCH = 63;    private const int TLE2_LEN_REVATEPOCH = 5;

      #endregion

      private static double Parse(string str) { return double.Parse(str, CultureInfo.InvariantCulture); }
      private static int ParseInt(string str) { return int.Parse(str, CultureInfo.InvariantCulture);    }

      #region Construction

      /// <summary>
      /// Standard constructor.
      /// </summary>
      /// <param name="name">The satellite name.</param>
      /// <param name="line1">Line 1 of the orbital elements.</param>
      /// <param name="line2">Line 2 of the orbital elements.</param>
      public TwoLineElements(string name, string line1, string line2)
      {
         SatelliteName = name;

         NoradIdStr = line1.Substring(TLE1_COL_SATNUM, TLE1_LEN_SATNUM);
         IntlDesignatorStr = line1.Substring(TLE1_COL_INTLDESC_A,
                                             TLE1_LEN_INTLDESC_A +
                                             TLE1_LEN_INTLDESC_B +
                                             TLE1_LEN_INTLDESC_C).
                                             Replace(" ", string.Empty);

         int epochYear   = ParseInt(line1.Substring(TLE1_COL_EPOCH_A, TLE1_LEN_EPOCH_A));
         double epochDay = Parse   (line1.Substring(TLE1_COL_EPOCH_B, TLE1_LEN_EPOCH_B));

         epochYear = (epochYear < 57) ? (epochYear + 2000) : (epochYear + 1900);
         Epoch = new Julian(epochYear, epochDay);

         string dragStr = (line1[TLE1_COL_MEANMOTIONDT] == '-') ? "-0" : "0";

         dragStr += line1.Substring(TLE1_COL_MEANMOTIONDT + 1, TLE1_LEN_MEANMOTIONDT);
         MeanMotionDt = Parse(dragStr);

         // Decimal point assumed; exponential notation
         BStar = Parse(ExpToDecimal(line1.Substring(TLE1_COL_BSTAR, TLE1_LEN_BSTAR)));

         string setNumStr = line1.Substring(TLE1_COL_ELNUM, TLE1_LEN_ELNUM).TrimStart();

         if (string.IsNullOrEmpty(setNumStr)) { setNumStr = "0"; }

         SetNumber = ParseInt(setNumStr);

         // Eccentricity: decimal point is assumed
         Eccentricity   = Parse("0." + line2.Substring(TLE2_COL_ECCENTRICITY, TLE2_LEN_ECCENTRICITY));
         InclinationDeg = Parse(line2.Substring(TLE2_COL_INCLINATION,  TLE2_LEN_INCLINATION ));
         RAANodeDeg     = Parse(line2.Substring(TLE2_COL_RAASCENDNODE, TLE2_LEN_RAASCENDNODE));
         ArgPerigeeDeg  = Parse(line2.Substring(TLE2_COL_ARGPERIGEE,   TLE2_LEN_ARGPERIGEE  ));
         MeanAnomalyDeg = Parse(line2.Substring(TLE2_COL_MEANANOMALY,  TLE2_LEN_MEANANOMALY ));
         MeanMotion     = Parse(line2.Substring(TLE2_COL_MEANMOTION,   TLE2_LEN_MEANMOTION  ));

         InclinationRad = InclinationDeg.ToRadians();
         RAANodeRad     = RAANodeDeg.ToRadians();
         ArgPerigeeRad  = ArgPerigeeDeg.ToRadians();
         MeanAnomalyRad = MeanAnomalyDeg.ToRadians();

         string revStr = line2.Substring(TLE2_COL_REVATEPOCH, TLE2_LEN_REVATEPOCH).TrimStart();

         if (string.IsNullOrEmpty(revStr)) { revStr = "0"; }

         RevAtEpoch = ParseInt(revStr);
      }

      #endregion

      #region Utility

      /// <summary>
      /// Converts TLE-style exponential notation of the form 
      ///       [ |+|-]00000[ |+|-]0
      /// to decimal notation. Assumes implied decimal point to the left
      /// of the first number in the string, i.e., 
      ///       " 12345-3" =  0.00012345
      ///       "-23429-5" = -0.0000023429   
      ///       " 40436+1" =  4.0436
      /// No sign character implies a positive value, i.e.,
      ///       " 00000 0" =  0.00000
      ///       " 31416 1" =  3.1416
      /// </summary>
      private static string ExpToDecimal(string str)
      {
         const int COL_SIGN = 0;
         const int LEN_SIGN = 1;

         const int COL_MANTISSA = 1;
         const int LEN_MANTISSA = 5;

         const int COL_EXPONENT = 6;
         const int LEN_EXPONENT = 2;

         string sign     = str.Substring(COL_SIGN, LEN_SIGN);
         string mantissa = str.Substring(COL_MANTISSA, LEN_MANTISSA);
         string exponent = str.Substring(COL_EXPONENT, LEN_EXPONENT).TrimStart();

         double val = Parse(sign + "0." + mantissa + "e" + exponent);
         int sigDigits = mantissa.Length + Math.Abs(ParseInt(exponent));

         return val.ToString("F" + sigDigits, CultureInfo.InvariantCulture);
      }

      #endregion
   }
}