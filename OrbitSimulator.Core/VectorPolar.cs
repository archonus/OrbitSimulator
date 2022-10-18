using System;
using static OrbitSimulator.Core.PhysicsUtils;

namespace OrbitSimulator.Core
{
    /// <summary>
    /// Represents a two dimensional vector in the polar form
    /// </summary>
    public struct VectorPolar
    {

        /// <summary>
        /// The magnitude of the vector along the direction
        /// </summary>
        public double Magnitude => r;

        /// <summary>
        /// Whether the vector is pointing downwards
        /// </summary>
        public bool IsDownwards => Direction < -HalfPi || Direction > HalfPi;

        /// <summary>
        /// The angle, in radians, from the vertical axis (up). 
        /// </summary>
        /// <remarks>Negative to the left and positive to the right. Ranges from -Pi to Pi</remarks>
        public double Direction => phi;

        readonly double r; //Magnitude of the vector
        readonly double phi; //Angle with VERTICAL axis

        /// <summary>
        /// Horizontal component
        /// </summary>
        public double X => r * Math.Sin(phi);

        /// <summary>
        /// Vertical Component
        /// </summary>
        public double Y => r * Math.Cos(phi);

        /// <summary>
        /// Creates a polar vector with magnitude and direction
        /// </summary>
        /// <param name="r">The magnitude of the vector</param>
        /// <param name="phi">The angle with the vertical, in radians</param>
        public VectorPolar(double r, double phi)
        {
            double simplified_phi;
            double simplified_r;
            //Make sure the value of phi is in range of -Pi to Pi
            simplified_phi = SimplifyAngle(phi);
            if (r == 0) //Zero vector
            {   //Chosen to mean that phi is also zero, although it doesn't really matter
                simplified_phi = simplified_r = 0;
            }
            else if (r < 0)
            { //Make r positive by changing direction
                simplified_r = -r;
                if (simplified_phi > 0)
                { //The angle was originally positive
                    simplified_phi = simplified_phi - Pi;
                }
                else
                { //The angle was originally negative
                    simplified_phi = simplified_phi + Pi;
                }
            }
            else
            { //r is positive, so simply assign
                simplified_r = r;
            }
            this.r = simplified_r;
            this.phi = simplified_phi;
        }

        public override string ToString()
        {
            return $"VectorPolar(r:{Magnitude:0.##}, phi:{Direction:0.##}, x:{X:0.##}, y:{Y:0.##})";
        }

        /// <summary>
        /// Converts a Cartesian representation of a vector into a polar vector
        /// </summary>
        /// <param name="x">The horizontal component</param>
        /// <param name="y">The vertical component</param>
        /// <returns>A VectorPolar</returns>
        public static VectorPolar FromCartesian(double x, double y)
        {
            double r = Math.Sqrt(x * x + y * y);
            double phi = Math.Atan2(x, y); //The arguments of atan2 are reversed to get the vertical angle rather than horizontal
            return new VectorPolar(r, phi);
        }

        #region Operators

        /// <summary>
        /// Scalar multiplication
        /// </summary>
        /// <param name="v">The vector to be scaled</param>
        /// <param name="scalar">The scaling factor</param>
        /// <returns>A vector with the same direction and scaled magnitude</returns>
        public static VectorPolar operator *(VectorPolar v, double scalar)
        {
            return new VectorPolar(scalar * v.Magnitude, v.Direction);
        }

        /// <summary>
        /// Scalar multiplication
        /// </summary>
        /// <param name="scalar">The scaling factor</param>
        /// <param name="v">The vector to be scaled</param>
        /// <returns>A vector with the same direction and scaled magnitude</returns>
        public static VectorPolar operator *(double scalar, VectorPolar v)
        {
            return new VectorPolar(scalar * v.Magnitude, v.Direction);
        }

        /// <summary>
        /// Adds two vectors together
        /// </summary>
        public static VectorPolar operator +(VectorPolar v1, VectorPolar v2)
        {
            var x = v1.X + v2.X;
            var y = v1.Y + v2.Y;
            return FromCartesian(x, y);
        }

        /// <summary>
        /// Translates a point by a vector
        /// </summary>
        /// <param name="v">The translation vector</param>
        /// <param name="p">The point</param>
        /// <returns>The translated Coord</returns>
        public static Coord operator +(VectorPolar v, Coord p)
        {
            return new Coord(v.X + p.X, v.Y + p.Y);
        }

        #endregion

    }
}
