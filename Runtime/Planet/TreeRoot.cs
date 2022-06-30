using UnityEngine;

namespace PlanetEngine
{
    /// <summary>
    /// The Root for the quad tree. This Node will create the first branches of the entire sphere.
    /// </summary>
    internal class TreeRoot : MonoBehaviour
    {
        void Update()
        {
            // Update all child roots only. This way branches that should update, update.
            foreach (Transform child in transform)
            {
                child.GetComponent<Branch>().UpdateQuadTree();
            }
        }

        /// <summary>
        /// Creates a branch for every side of a cube.
        /// </summary>
        public void CreateRootBranches()
        {
            Planet planet = GetComponentInParent<Planet>();
            for (int i = 0; i < 6; i++)
            {
                // Create the branch object.
                GameObject rootBranchObject = new GameObject(planet.name + " - Branch: " + i);
                rootBranchObject.tag = "PlanetEngine";
                rootBranchObject.transform.SetParent(transform);
                rootBranchObject.transform.localPosition = Vector3.zero;

                // Depending on which side the right properties are set.
                Rect zone = Rect.zero;
                CubeSides side = CubeSides.FRONT;
                switch (i)
                {
                    case 0:
                        rootBranchObject.transform.eulerAngles = new Vector3(0, 180, 0);
                        zone = new Rect(1f / 4, 2f / 3, 1f / 4, 1f / 3);
                        side = CubeSides.TOP;
                        break;
                    case 1:
                        rootBranchObject.transform.eulerAngles = new Vector3(180, 180, 0);
                        zone = new Rect(1f / 4, 0, 1f / 4, 1f / 3);
                        side = CubeSides.BOTTOM;
                        break;
                    case 2:
                        rootBranchObject.transform.eulerAngles = new Vector3(-90, 180, 0);
                        zone = new Rect(1f / 4, 1f / 3, 1f / 4, 1f / 3);
                        side = CubeSides.FRONT;
                        break;
                    case 3:
                        rootBranchObject.transform.eulerAngles = new Vector3(-90, 0, 0);
                        zone = new Rect(3f / 4, 1f / 3, 1f / 4, 1f / 3);
                        side = CubeSides.BACK;
                        break;
                    case 4:
                        rootBranchObject.transform.eulerAngles = new Vector3(-90, 0, -90);
                        zone = new Rect(0, 1f / 3, 1f / 4, 1f / 3);
                        side = CubeSides.LEFT;
                        break;
                    case 5:
                        rootBranchObject.transform.eulerAngles = new Vector3(-90, 0, 90);
                        zone = new Rect(2f / 4, 1f / 3, 1f / 4, 1f / 3);
                        side = CubeSides.RIGHT;
                        break;
                }
                rootBranchObject.AddComponent<Branch>().CreateBranch(side);
            }
        }

        /// <summary>
        /// Destroys the root branches and all branches under them.
        /// </summary>
        public void RemoveRootBranches()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }
    }
}