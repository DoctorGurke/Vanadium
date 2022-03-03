using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Vanadium;

public class Camera {
	public static Camera ActiveCamera { get; private set; }
	internal CameraSetup setup;

	public Vector3 Position {
		get => setup.Position;
		set => setup.Position = value;
	}

	public Rotation Rotation {
		get => setup.Rotation;
		set => setup.Rotation = value;
	}

	public float FieldOfView {
		get => setup.FieldOfView;
		set => setup.FieldOfView = value;
	}

	public float ZNear {
		get => setup.ZNear;
		set => setup.ZNear = value;
	}

	public float ZFar {
		get => setup.ZFar;
		set => setup.ZFar = value;
	}

	public Camera() {
		ActiveCamera = this;
		Activate();
	}

	public virtual void Activate() {
		Position = Vector3.Zero;
		Rotation = Rotation.Identity;
	}

	public static void BuildActiveCamera() {
		ActiveCamera.BuildView(ref ActiveCamera.setup);
	}

	public virtual void BuildView(ref CameraSetup setup) {
		if(setup.FieldOfView == 0) setup.FieldOfView = 90;

		if(setup.ZNear == 0) setup.ZNear = 0.001f;
		if(setup.ZFar == 0) setup.ZFar = 1000.0f;
	}

	private bool _firstMove = true;

	private Vector2 _lastPos;

	public virtual void BuildInput(KeyboardState keyboard, MouseState mouse) {
		const float cameraSpeed = 1.5f;

		if(keyboard.IsKeyDown(Keys.W)) {
			Position += Rotation.Forward * cameraSpeed * Time.Delta; // Forward
		}

		if(keyboard.IsKeyDown(Keys.S)) {
			Position -= Rotation.Forward * cameraSpeed * Time.Delta; // Backwards
		}
		if(keyboard.IsKeyDown(Keys.A)) {
			Position -= Rotation.Right * cameraSpeed * Time.Delta; // Left
		}
		if(keyboard.IsKeyDown(Keys.D)) {
			Position += Rotation.Right * cameraSpeed * Time.Delta; // Right
		}
		if(keyboard.IsKeyDown(Keys.Space)) {
			Position += Rotation.Up * cameraSpeed * Time.Delta; // Up
		}
		if(keyboard.IsKeyDown(Keys.LeftControl)) {
			Position -= Rotation.Up * cameraSpeed * Time.Delta; // Down
		}

		if(_firstMove) {
			_lastPos = new Vector2(mouse.X, mouse.Y);
			_firstMove = false;
		} else {
			// Calculate the offset of the mouse position
			var deltaX = mouse.X - _lastPos.X;
			var deltaY = mouse.Y - _lastPos.Y;
			_lastPos = new Vector2(mouse.X, mouse.Y);

			Rotation = Rotation.RotateAroundAxis(Vector3.Up, -deltaX * 0.1f);
			Rotation = Rotation.RotateAroundAxis(Vector3.Right, -deltaY * 0.1f);
		}
	}

	public Matrix4 ViewMatrix => Matrix4.LookAt(Position, Position + Rotation.Forward, Rotation.Up);
	public Matrix4 ProjectionMatrix => Matrix4.CreatePerspectiveFieldOfView(FieldOfView.DegreeToRadian(), Screen.AspectRatio, ZNear, ZFar);
}
