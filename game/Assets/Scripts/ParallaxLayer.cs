using UnityEngine;

namespace Prpg.Game
{
    /// <summary>
    /// Hook for background layers that scroll slower than the camera. The far layer will be a zoomed-out painting
    /// moving at a small fraction of camera speed. No art yet, so nothing creates one; the view is a flat colour.
    /// </summary>
    public sealed class ParallaxLayer : MonoBehaviour
    {
        public float Factor = 0.1f;          // 0 = fixed to the screen, 1 = moves with the world
        public Vector2 Origin;               // where the layer sits when the camera is at the world origin

        /// <summary>Call after the camera moves each frame.</summary>
        public void Follow(Vector3 cameraPosition)
        {
            transform.position = new Vector3(Origin.x + cameraPosition.x * (1 - Factor), Origin.y + cameraPosition.y * (1 - Factor), 10);
        }
    }
}
