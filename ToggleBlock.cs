using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
    public class ToggleBlock
    {
        private IMyFunctionalBlock block;
        private bool invertToggle;

        public ToggleBlock(IMyFunctionalBlock block, bool invertToggle)
        {
            this.invertToggle = invertToggle;
            this.block = block;
        }

        public void Enable()
        {
            block.Enabled = !invertToggle;
        }

        public void Disable()
        {
            block.Enabled = invertToggle;
        }
    }
}