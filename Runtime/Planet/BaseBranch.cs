using UnityEngine;

namespace PlanetEngine
{

    /// <summary>
    /// This abstract class defines a branch in the quad tree.
    /// </summary>

    internal abstract class BaseBranch : MonoBehaviour
    {
        /// <summary>
        /// The visibility of a branch enables or disables the renderer of the branch.
        /// It is used when a branch is split up in smaller branches.
        /// </summary>
        internal abstract bool Visible { get; set; }
    }
}