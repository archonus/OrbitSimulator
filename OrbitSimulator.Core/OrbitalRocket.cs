using System;
using static OrbitSimulator.Core.PhysicsUtils;

namespace OrbitSimulator.Core
{
    /// <summary>
    /// Not complete
    /// </summary>
    class OrbitalRocket : IdealSingleStageRocket
    {

        readonly Coord EarthCentre = new Coord(0, -EarthRadius);
        protected double GravityDirection
        {
            get
            {
                VectorPolar v = Coord.VectorBetween(EarthCentre, Position);
                return v.Direction;
            }
        }
        public OrbitalRocket(RocketEngine engine, double payloadMass, double fuel, double structureMass) : base(engine, payloadMass, fuel, structureMass)
        {
            throw new NotImplementedException("This class is not complete");
        }
        protected override VectorPolar CalculateVelocityChange(double initial_mass, double final_mass)
        {
            var dv = Ve * Math.Log(initial_mass / final_mass); //The magnitude of the change in velocity from rocket equation
            var dV = new VectorPolar(dv, Thrust.Direction);
            VectorPolar gEffect = new VectorPolar(-g0 * (initial_mass - final_mass) / Mdot, GravityDirection); //The effect of gravity
            return dV + gEffect;
        }
    }
}
