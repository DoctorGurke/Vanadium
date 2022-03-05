namespace Vanadium;

public class Material {
	public enum MaterialParamType {
		Float,
		Integer,
		Vector2,
		Vector3,
		Vector4,
		Matrix4,
		Sampler2D
	}

	public Dictionary<string, MaterialParamType> MaterialParameters = new();
}
