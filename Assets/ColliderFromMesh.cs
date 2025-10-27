using UnityEngine;

/// <summary>
/// This class creates a MeshCollider from a SkinnedMeshRenderer and updates it each frame.
/// </summary>
public class ColliderFromMesh : MonoBehaviour
{
    /// <value> Reference to the MeshCollider component. </value>
    private MeshCollider meshCollider;
    /// <value> Reference to the SkinnedMeshRenderer component. </value>
    private SkinnedMeshRenderer skinnedMeshRenderer;
    /// <value> Mesh used for the collider. </value>
    private Mesh colliderMesh;

    /// <summary>
    /// Initializes the components and sets up the MeshCollider.
    /// </summary>
    void Start()
    {
        // Get the SkinnedMeshRenderer from the object
        skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        if (skinnedMeshRenderer == null)
        {
            Debug.LogError("SkinnedMeshRenderer not found on the object.");
            return;
        }

        // Create a new mesh to hold the current skin position
        colliderMesh = new Mesh();

        // Add MeshCollider to the object
        meshCollider = gameObject.AddComponent<MeshCollider>();
    }

    /// <summary>
    /// Updates the MeshCollider with the current baked mesh each frame.
    /// </summary>
    void Update()
    {
        // Bake the current mesh into colliderMesh
        skinnedMeshRenderer.BakeMesh(colliderMesh);

        // Set the baked mesh as the sharedMesh in MeshCollider
        meshCollider.sharedMesh = colliderMesh;

        // Ensure the collider has the correct position and rotation
        meshCollider.transform.position = skinnedMeshRenderer.transform.position;
        meshCollider.transform.rotation = skinnedMeshRenderer.transform.rotation;
    }
}