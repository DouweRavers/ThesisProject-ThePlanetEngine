using System.Collections.Generic;
using UnityEngine;

namespace PlanetEngine
{
    /// <summary>
    /// The branch node is part of the quad tree structure. Depending on the distance of the target.
    /// It will remain current detail level, divide in 4 higher detailed branches or shrink back to 
    /// a lower detailed branch.
    /// </summary>
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class QuadTreeBranchNode : MonoBehaviour
    {
        /// <summary>
        /// This property indicates if the current branch is visible.
        /// </summary>
        public bool Visible
        {
            get { return GetComponent<MeshRenderer>().enabled; }
            set
            {
                GetComponent<MeshRenderer>().enabled = value;
                if (ocean != null) ocean.Visible = value;
            }
        }
        /// <summary>
        /// The number divisions until this branch.
        /// </summary>
        public int QuadDepth { get { return _quadDepth; } }
        int _quadDepth;
        /// <summary>
        /// Is the branch currently divided in child branches.
        /// </summary>
        public bool Divided { get { return _divided; } }
        bool _divided = false;
        /// <summary>
        /// The bounds of the planeMesh on the unit sphere.
        /// </summary>
        public Bounds Bounds { get { return _bounds; } }
        Bounds _bounds;
        /// <summary>
        /// The base texture which other textures are generated from.
        /// </summary>
        public Texture2D BaseTexture { get { return _baseTexture; } }
        Texture2D _baseTexture;

        // Reference to the associate ocean mesh
        OceanNode ocean;

        #region Branching Process
        /// <summary>
        /// When the parent branch is active the Update method on this branch is called.
        /// </summary>
        public void UpdateQuadTree()
        {
            Planet planet = GetComponentInParent<Planet>();
            // The distance between the centre of the branch and the target.
            float targetDistance = Vector3.Distance(planet.Target.position, transform.position);
            float expandDistance = GetComponent<MeshFilter>().sharedMesh.bounds.size.magnitude * 0.6f;
            float shrinkDistance = GetComponent<MeshFilter>().sharedMesh.bounds.size.magnitude * 1f;
            // the branch is currently split => in check if has to shrink or update child branches.
            // the branch is not split and thus active(=non divided and updating) => check if has to fold back up.
            if (_divided)
            {
                if (targetDistance > shrinkDistance) QuadTreeShrink();
                else QuadTreeUpdateChildBranches();
            }
            else if (targetDistance < expandDistance) QuadTreeExpand();
        }

        // Expands the branch into 4 smaller branches
        void QuadTreeExpand()
        {
            Planet planet = GetComponentInParent<Planet>();
            // If the max branching level is achieved the planet switches to a terrain.
            if (_quadDepth == planet.Data.MaxDepth)
            {
                return;
            }
            // Create new child branches and disable own visuals.
            CreateChildQuads();
            _divided = true;
            Visible = false;
        }

        // Destroys child branches and enables own visuals.
        void QuadTreeShrink()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                QuadTreeBranchNode branch = transform.GetChild(i).GetComponent<QuadTreeBranchNode>();
                if (branch != null) Destroy(branch.gameObject);
            }
            Visible = true;
            _divided = false;
        }

        // Triggers the child branches to also update.
        void QuadTreeUpdateChildBranches()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                QuadTreeBranchNode branch = transform.GetChild(i).GetComponent<QuadTreeBranchNode>();
                if (branch != null) branch.UpdateQuadTree();
            }
        }

        // creates four new branches under the current branch.
        void CreateChildQuads()
        {
            Planet planet = GetComponentInParent<Planet>();
            // Create four new quads.
            for (int i = 0; i < 4; i++)
            {
                // Create the branch object.
                GameObject ChildQuadObject = new GameObject(this.name + "." + i);
                ChildQuadObject.transform.SetParent(transform);
                ChildQuadObject.transform.localPosition = Vector3.zero;
                ChildQuadObject.transform.localEulerAngles = Vector3.zero;

                // Define which zone of the current branch this new branch will represent.
                Rect zone = Rect.zero;
                if (i == 0) zone = new Rect(0, 0, 0.5f, 0.5f);
                else if (i == 1) zone = new Rect(0, 0.5f, 0.5f, 0.5f);
                else if (i == 2) zone = new Rect(0.5f, 0, 0.5f, 0.5f);
                else zone = new Rect(0.5f, 0.5f, 0.5f, 0.5f);
                ChildQuadObject.AddComponent<QuadTreeBranchNode>().CreateBranch(this, zone);
            }
            // Add the new renderers to the LOD system.
            LODGroup lodGroup = planet.GetComponent<LODGroup>();
            LOD[] lods = lodGroup.GetLODs();
            List<Renderer> quadRenderers = new List<Renderer>(lods[0].renderers);
            quadRenderers.AddRange(GetComponentsInChildren<Renderer>());
            lods[0].renderers = quadRenderers.ToArray();
            lodGroup.SetLODs(lods);
        }
        #endregion

        #region Branch Creation methods
        /// <summary>
        /// Creates a branch with no parent branch.
        /// </summary>
        /// <param name="side">Indicates which side of the cube is generated. Used for basetexture.</param>
        public void CreateBranch(CubeSides side)
        {
            // Is the first branch in a tree so depth is zero.
            _quadDepth = 0;
            // Generate the base texture for a non parent branch.
            _baseTexture = ProceduralTexture.GetBaseTexture(128, 128, side);
            // Generate a plane which represents a single side on a unit cube.
            Vector3 offset = Vector3.up * 0.5f;
            float size = Mathf.Sqrt(0.5f);
            CreateBranch(offset, size);
        }

        /// <summary>
        /// Creates a branch with based on parent branch.
        /// </summary>
        /// <param name="parent">Reference to the parent node.</param>
        /// <param name="zone">In 2D surface space, which zone the new quad is.</param>
        public void CreateBranch(QuadTreeBranchNode parent, Rect zone)
        {
            // Increase the depth by one based on parents depth. 
            _quadDepth = parent.QuadDepth + 1;
            // Generate the base texture from the texture of the parent.
            _baseTexture = ProceduralTexture.GetBaseTexture(parent.BaseTexture, zone);
            // Generate a plane which is a part of a parent plane on the unit cube.
            Vector3 offset = parent.Bounds.center +
                2f * parent.Bounds.extents.x * new Vector3(zone.x - 0.25f, 0, zone.y - 0.25f);
            float size = parent.Bounds.size.magnitude / 4f;
            CreateBranch(offset, size);
        }

        // Takes an offset and size for a plane on the unit cube and generates the mesh accordingly.
        void CreateBranch(Vector3 offset, float size)
        {
            Planet planet = GetComponentInParent<Planet>();
            // Generate a plane on a unit cube according to a certain size and offset.
            Mesh planeMesh = MeshTool.GenerateUnitQuadMesh();
            planeMesh = MeshTool.NormalizeAndAmplify(planeMesh, size);
            planeMesh = MeshTool.OffsetMesh(planeMesh, offset);
            MeshTool.SubdivideGPU(planeMesh, 5);
            // Store the position on the unit cube
            _bounds = planeMesh.bounds;

            // Generate the mesh surface of the branch
            GetComponent<MeshFilter>().mesh = ProceduralMesh.GetBranchMesh(planeMesh, transform);
            GetComponent<MeshRenderer>().material = ProceduralMaterial.GetLandMaterial(planet.Data, _baseTexture);
            if (planet.Data.HasOcean) CreateOcean(planeMesh);
        }

        // Creates a ocean object 
        void CreateOcean(Mesh planeMesh)
        {
            GameObject oceanObject = new GameObject(name + ": Ocean");
            oceanObject.transform.SetParent(transform);
            oceanObject.transform.localPosition = Vector3.zero;
            oceanObject.transform.localEulerAngles = Vector3.zero;
            ocean = oceanObject.AddComponent<OceanNode>();
            ocean.CreateOcean(planeMesh, _baseTexture);
        }
        #endregion

    }
}