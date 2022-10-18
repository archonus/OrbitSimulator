using System;
using static OrbitSimulator.Core.PhysicsUtils;
namespace OrbitSimulator.Core
{
    public struct RocketEngine
    {
        public readonly double Mass;
        public readonly double SpecificImpulse;
        public readonly double MassFlowRate;
        private double gimbalAngle;

        public double MaxThrust { get => SpecificImpulse * MassFlowRate * PhysicsUtils.g0; }
        public double GimbalAngle
        {
            get => gimbalAngle;
            set
            {
                var angle = value % TwoPi;
                if (-HalfPi < angle && angle < HalfPi)
                {
                    gimbalAngle = angle;
                }
                else
                {
                    throw new ArgumentOutOfRangeException("The direction of thrust must be up the rocket's axis.");
                }
            }
        }

        public RocketEngine(double mass, double specificImpulse, double massFlowRate)
        {
            this.Mass = mass;
            this.SpecificImpulse = specificImpulse;
            this.MassFlowRate = massFlowRate;
            this.gimbalAngle = 0;
        }

        public override string ToString()
        {
            return $"Mass:{Mass}, Specific Impulse:{SpecificImpulse}, mdot:{MassFlowRate}";
        }
    }
}
