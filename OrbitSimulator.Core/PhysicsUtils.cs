using System;

namespace OrbitSimulator.Core
{
    /// <summary>
    /// Provides physical constants and utility functions
    /// </summary>
    public static class PhysicsUtils
    {

        #region Angle Constants
        /// <summary>
        /// A full turn of circle
        /// </summary>
        public const double TwoPi = Math.PI * 2;

        /// <summary>
        /// A quarter turn of a circle
        /// </summary>
        public const double HalfPi = Math.PI * 0.5;

        public const double Pi = Math.PI;
        #endregion

        #region Atmosphere Heights

        /// <summary>
        /// The height of the boundary of the atmosphere
        /// </summary>
        /// <remarks>Measured in kilometres</remarks>
        public const double AtmosphereEdge = 80;

        /// <summary>
        /// Internationally recognised, if somewhat arbitary, boundary for space. Not used by NASA
        /// </summary>
        public const double KarmanLine = 100;

        /// <summary>
        /// Upper bound of the stratosphere - the second layer of the atmosphere
        /// </summary>
        /// <remarks>Measured in kilometres</remarks>
        public const double StratosphereEdge = 50;

        /// <summary>
        /// Upper bound of the troposphere - the lowest layer of the atmosphere
        /// </summary>
        /// <remarks>Measured in kilometres</remarks>
        public const double TroposphereEdge = 10;

        #endregion

        /// <summary>
        /// Standard gravity
        /// </summary>
        public const double g0 = 9.81;

        /// <summary>
        /// Radius of the Earth in kilometres
        /// </summary>
        public const double EarthRadius = 6.371e3;

        /// <summary>
        /// The tangential velocity that the rocket already has due to being at the Earth's equator
        /// </summary>
        /// <remarks>In km/hr</remarks>
        public const double TangentialVelocityEquator = 1674.4;

        /// <summary>
        /// Gets the tangential velocity gained
        /// </summary>
        /// <param name="latitude">The latitude being considered - the defualt is 0 (the equator)</param>
        /// <returns>The tangential velocity in km/hr</returns>
        public static double GetTangentialVelocity(double latitude = 0)
        {
            return Math.Cos(ConvertDegreesToRadians(latitude)) * TangentialVelocityEquator;
        }

        /// <summary>
        /// Returns the speed required to maintain orbit around Earth at a certain height
        /// </summary>
        /// <param name="height">The height above the surface of the Earth in kilometres</param>
        /// <returns>The speed in km/hr required to stay in orbit</returns>
        public static double GetRequiredOrbitalSpeed(double height)
        {
            double earthRadiusMetres = ConvertKmToMetres(EarthRadius);
            double numerator = g0 * earthRadiusMetres * earthRadiusMetres;
            double denominator = earthRadiusMetres + ConvertKmToMetres(height);
            return ConvertMpsToKmph(Math.Sqrt(numerator / denominator));
        }

        /// <summary>
        /// Returns the speed required to escape Earth's gravitational field
        /// </summary>
        /// <param name="height">The height above the Earth's surface in kilometre</param>
        /// <returns>The escape velocity in km/hr</returns>
        public static double GetEscapeVelocity(double height = 0)
        {
            return Math.Sqrt(2) * GetRequiredOrbitalSpeed(height);
        }

        public static double ConvertRadiansToDegrees(double radAngle)
        {
            return radAngle * 180 / Pi;
        }

        public static double ConvertDegreesToRadians(double degAngle)
        {
            return degAngle * Pi / 180;
        }


        /// <summary>
        /// Converts an angle with the horizontal to an angle with the vertical
        /// </summary>
        /// <param name="theta">The angle with the horizontal axis, in radians</param>
        /// <returns>The equivalent angle from the vertical in the range -PI to PI</returns>
        public static double ConvertAngleToVertical(double theta)
        {
            theta = SimplifyAngle(theta);
            double phi;
            if (theta >= -HalfPi)
            { //Most of the cases
                phi = HalfPi - theta;
            }
            else
            { //-Pi < theta < -Pi/2
                phi = -(3 * HalfPi - theta);
            }
            return phi;
        }

        /// <summary>
        /// Simplify the angle to the range of -Pi to Pi
        /// </summary>
        /// <param name="theta">The angle to be simplified</param>
        /// <returns>The equivalent angle in the range of -<see cref="Pi"/> to <see cref="Pi"/></returns>
        public static double SimplifyAngle(double theta)
        {
            theta = theta % TwoPi;
            if (theta > Pi)
            { //Value should be negative
                theta = theta - TwoPi;
            }
            else if (theta < -Pi)
            { //The value should be positive
                theta = theta + TwoPi;
            }
            return theta;
        }

        /// <summary>
        /// Converts metres per second to kilometres per hour
        /// </summary>
        public static double ConvertMpsToKmph(double mps)
        {
            return mps * 3.6;
        }

        /// <summary>
        /// Convert kilometres per hour to metres per second
        /// </summary>
        public static double ConvertKmphToMps(double kmph)
        {
            return kmph / 3.6;
        }

        /// <summary>
        /// Convert metres to kilometres
        /// </summary>
        public static double ConvertMetresToKm(double m)
        {
            return 0.001 * m;
        }

        public static double ConvertKmToMetres(double km)
        {
            return 1000 * km;
        }
    }
}
