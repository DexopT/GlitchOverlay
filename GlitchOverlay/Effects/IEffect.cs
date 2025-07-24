using GlitchOverlay.Core;
using GlitchOverlay.Models;

namespace GlitchOverlay.Effects
{
    /// <summary>
    /// Interface for all glitch effects
    /// </summary>
    public interface IEffect
    {
        /// <summary>
        /// Gets the name of the effect
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets whether the effect is currently enabled
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// Updates the effect configuration
        /// </summary>
        void UpdateConfiguration(GlitchConfiguration config);

        /// <summary>
        /// Applies the effect to the graphics engine
        /// </summary>
        void Apply(GraphicsEngine graphics, double deltaTime);

        /// <summary>
        /// Resets the effect state
        /// </summary>
        void Reset();
    }
}
