using System;
using OrbitSimulator.Core;
using Xamarin.Forms;

namespace OrbitSimulator
{
    /// <summary>
    /// Class for calculating the colour of the sky
    /// </summary>
    internal static class SkyColourHelper
    {
        public static Color StartingColour = Color.DeepSkyBlue;
        static readonly double upperAtmosphereLumDiff = 0.25; //The upper atmosphere is 0.25 lumonisity, where 0.5 is normal and 1 is black
        static readonly double spaceLumDiff = 0.48; //Close to (0.02) completely black


        public static Color GetColor(double height)
        {
            double luminosity_diff;
            if (height < PhysicsUtils.TroposphereEdge)
            {
                luminosity_diff = Math.Round(
                                             height / PhysicsUtils.TroposphereEdge
                                             * upperAtmosphereLumDiff,
                                             3, MidpointRounding.AwayFromZero);
            }
            else if (height < PhysicsUtils.AtmosphereEdge)
            {
                luminosity_diff = upperAtmosphereLumDiff + Math.Round(
                                                     (height - PhysicsUtils.TroposphereEdge) / PhysicsUtils.AtmosphereEdge
                                                      * (spaceLumDiff - upperAtmosphereLumDiff),
                                                      3, MidpointRounding.AwayFromZero);
            }
            else
            {
                luminosity_diff = spaceLumDiff;
            }
            return StartingColour.AddLuminosity(-luminosity_diff); //Negative since it is getting darker
        }
    }
}
