namespace OrbitSimulator.Core
{
    /// <summary>
    /// Represents a point on the xy plane
    /// </summary>
    public struct Coord
    {
        public readonly double X;
        public readonly double Y;
        public Coord(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }

        public override string ToString()
        {
            return $"({X},{Y})";
        }

        /// <summary>
        /// Returns the vector that is between O and A
        /// </summary>
        /// <param name="O">The origin point, from which the vector starts</param>
        /// <param name="A">The end point</param>
        /// <returns>A vector that represents the translation of O to A</returns>
        public static VectorPolar VectorBetween(Coord O, Coord A)
        {
            return VectorPolar.FromCartesian(A.X - O.X, A.Y - O.Y);
        }

        public static Coord operator +(Coord a, Coord b)
        {
            return new Coord(a.X + b.X, a.Y + b.Y);
        }

        /// <summary>
        /// Translates a point by a vector
        /// </summary>
        /// <param name="a">The point</param>
        /// <param name="v">The translation vector</param>
        /// <returns>The translated point</returns>
        public static Coord operator +(Coord a, VectorPolar v)
        {
            return new Coord(a.X + v.X, a.Y + v.Y);
        }
    }
}
