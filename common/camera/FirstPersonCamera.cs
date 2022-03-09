using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Vanadium;

public class FirstPersonCamera : Camera {

	private bool _firstMove = true;

	private Vector2 _lastPos;
	private float targetPitch = 0;
	private float targetYaw = 0;

	private float TargetFOV = 90;

	public override void Update() {
		FieldOfView = TargetFOV;
	}

	public void ResetLastPosition(Vector2 lastpos) {
		_lastPos = lastpos;
	}

	public override void BuildInput(KeyboardState keyboard, MouseState mouse) {
		var fast = keyboard.IsKeyDown(Keys.LeftShift);
		var slow = keyboard.IsKeyDown(Keys.LeftAlt);

		float cameraSpeed = slow ? 0.5f : fast ? 10 : 2.5f;
		float cameraSensitivity = 0.1f;

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
			Position += Vector3.Up * cameraSpeed * Time.Delta; // Up
		}
		if(keyboard.IsKeyDown(Keys.LeftControl)) {
			Position -= Vector3.Up * cameraSpeed * Time.Delta; // Down
		}

		if(mouse.IsButtonDown(MouseButton.Middle)) {
			TargetFOV = TargetFOV.LerpTo(20, Time.Delta * 7f);
		} else {
			TargetFOV = TargetFOV.LerpTo(90, Time.Delta * 7f);
		}

		if(_firstMove) {
			_lastPos = new Vector2(mouse.X, mouse.Y);
			_firstMove = false;
		} else {
			// calculate the mouse delta
			var deltaX = mouse.X - _lastPos.X;
			var deltaY = mouse.Y - _lastPos.Y;
			_lastPos = new Vector2(mouse.X, mouse.Y);

			targetYaw -= deltaX * cameraSensitivity;
			targetPitch -= deltaY * cameraSensitivity;
			
			targetPitch = targetPitch.Clamp(90, -90);

			Rotation = Rotation.Identity.RotateAroundAxis(Vector3.Up, targetYaw).RotateAroundAxis(Vector3.Right, targetPitch);
		}
	}
}
