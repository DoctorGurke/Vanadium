using OpenTK.Mathematics;

namespace Vanadium;

// Taken from https://github.com/opentk/LearnOpenTK
// This is the camera class as it could be set up after the tutorials on the website.
// It is important to note there are a few ways you could have set up this camera.
// For example, you could have also managed the player input inside the camera class,
// and a lot of the properties could have been made into functions.

// TL;DR: This is just one of many ways in which we could have set up the camera.
// Check out the web version if you don't know why we are doing a specific thing or want to know more about the code.
public class Camera {
	public static Camera ActiveCamera { get; private set; }

	public Rotation Rotation { get; set; } = Rotation.Identity;
	public Vector3 Position { get; set; }

	// The field of view of the camera (radians)
	private float _fov = MathHelper.PiOver2;

	public Camera(Vector3 position, float aspectRatio) {
		ActiveCamera = this;
		Position = position;
		AspectRatio = aspectRatio;
	}

	// This is simply the aspect ratio of the viewport, used for the projection matrix.
	public float AspectRatio { private get; set; }

	// The field of view (FOV) is the vertical angle of the camera view.
	// This has been discussed more in depth in a previous tutorial,
	// but in this tutorial, you have also learned how we can use this to simulate a zoom feature.
	// We convert from degrees to radians as soon as the property is set to improve performance.
	public float Fov {
		get => MathHelper.RadiansToDegrees(_fov);
		set {
			var angle = MathHelper.Clamp(value, 1f, 90f);
			_fov = MathHelper.DegreesToRadians(angle);
		}
	}

	// Get the view matrix using the amazing LookAt function described more in depth on the web tutorials
	public Matrix4 GetViewMatrix() {
		var pos = Position;
		var target = Position + Rotation.Forward;
		var up = Rotation.Up;
		//Debug.WriteLine($"{pos} | {target} | {up}");
		return Matrix4.LookAt(pos, target, up);
	}

	// Get the projection matrix using the same method we have used up until this point
	public Matrix4 GetProjectionMatrix() {
		return Matrix4.CreatePerspectiveFieldOfView(_fov, AspectRatio, 0.01f, 100f);
	}
}
