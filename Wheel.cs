using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;

namespace IngameScript
{
    public class Wheel
    {
        private readonly IMyMotorSuspension wheel;
        private readonly bool reversePropulsion;

        public Wheel(IMyMotorSuspension wheel, bool reversePropulsion)
        {
            this.wheel = wheel;
            this.reversePropulsion = reversePropulsion;
        }

        public void StartCruise()
        {
            int propulsionCoefficient = 1;
            if (reversePropulsion)
            {
                propulsionCoefficient *= -1;
            }
            wheel.PropulsionOverride = propulsionCoefficient;
            wheel.SetValue("Speed Limit", 3.6f);
        }

        public void StopCruise()
        {
            wheel.PropulsionOverride = 0;
            // Cant set the speed limit using a reasonable interface, so we'll be using this.
            // May god help you if you want a speed limit >:)
            wheel.ApplyAction("ResetSpeed Limit");
        }
    }
}