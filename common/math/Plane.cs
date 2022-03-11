namespace Vanadium;

/// <summary>
/// Represents a simple Plane in 3D space
/// </summary>
public struct Plane
{
	/// <summary>
	/// Distance from world origin to the nearest point on the plane
	/// </summary>
	public float distance;

	/// <summary>
	/// The normal of the plane
	/// </summary>
	public Vector3 normal;
}
