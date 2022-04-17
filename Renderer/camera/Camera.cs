using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Vanadium;

public abstract class Camera
{
	public static Camera? ActiveCamera { get; private set; }
	internal CameraSetup setup;

	public Vector3 Position
	{
		get => setup.Position;
		set => setup.Position = value;
	}

	public Rotation Rotation
	{
		get => setup.Rotation;
		set => setup.Rotation = value;
	}

	public float FieldOfView
	{
		get => setup.FieldOfView;
		set => setup.FieldOfView = value;
	}

	public float ZNear
	{
		get => setup.ZNear;
		set => setup.ZNear = value;
	}

	public float ZFar
	{
		get => setup.ZFar;
		set => setup.ZFar = value;
	}

	public Camera()
	{
		ActiveCamera = this;
		Activate();
	}

	public virtual void Activate() { }

	public virtual void Update() { }

	public static void BuildActiveCamera()
	{
		ActiveCamera?.Update();
		ActiveCamera?.BuildView( ref ActiveCamera.setup );
	}

	public virtual void BuildView( ref CameraSetup setup )
	{
		if ( setup.FieldOfView == 0 ) setup.FieldOfView = 75;

		if ( setup.ZNear == 0 ) setup.ZNear = 0.3f;
		if ( setup.ZFar == 0 ) setup.ZFar = 1000.0f;
	}

	public virtual void BuildInput( KeyboardState keyboard, MouseState mouse ) { }

	public OpenTKMath.Matrix4 ViewMatrix => OpenTKMath.Matrix4.LookAt( Position, Position + Rotation.Forward, Rotation.Up );
	public OpenTKMath.Matrix4 ProjectionMatrix => OpenTKMath.Matrix4.CreatePerspectiveFieldOfView( FieldOfView.DegreeToRadian(), Screen.AspectRatio, ZNear, ZFar );
}
