using OpenTK.Mathematics;

namespace Vanadium;

public class SceneObject
{
	private Model? _model;
	public Model? Model
	{
		get
		{
			return _model ?? Model.Primitives.Error;
		}
		set
		{
			_model = value;
			PostModelSet();
		}
	}

	public void SetModel( Model model )
	{
		Model = model;
	}

	public void SetModel( string path )
	{
		Model = Model.Load( path );
	}

	public bool Transparent { get; private set; }

	public Color RenderColor = Color.White;
	public float TintAmount = 1.0f;

	public Vector3 Position
	{
		get
		{
			return LocalTransform.Position;
		}
		set
		{
			LocalTransform.Position = value;
		}
	}
	public Rotation Rotation
	{
		get
		{
			return LocalTransform.Rotation;
		}
		set
		{
			LocalTransform.Rotation = value;
		}
	}
	public float Scale
	{
		get
		{
			return LocalTransform.Scale;
		}
		set
		{
			LocalTransform.Scale = value;
		}
	}
	public Transform LocalTransform;
	public Matrix4 GlobalTransform => Parent is null ? LocalTransform.TransformMatrix : LocalTransform.TransformMatrix * Parent.GlobalTransform;

	private SceneObject? _parent;
	public SceneObject? Parent
	{
		get
		{
			return _parent;
		}
		set
		{
			if ( value == this ) return;
			_parent = value;
			value?.Children.Add( this );
		}
	}
	public List<SceneObject> Children = new();

	public SceneObject()
	{
		Position = Vector3.Zero;
		Rotation = Rotation.Identity;
		Scale = 1.0f;

		OnSpawn();
	}

	private void PostModelSet()
	{
		if ( Model is null ) return;
		var transparent = false;
		foreach ( var mesh in Model.Meshes )
		{
			if ( mesh.Material.Transparent )
			{
				transparent = true;
				break;
			}
		}

		if ( transparent )
		{
			SceneWorld.AddTransparent( this );
		}
		else
		{
			SceneWorld.AddOpaque( this );
		}
	}

	public virtual void OnSpawn() { }

	public void Draw()
	{
		Model?.Draw( this );
		OnRender();
	}

	protected virtual void OnRender()
	{
		if ( Model is not null )
			DebugDraw.Box( Vector3.Zero * GlobalTransform, Model.RenderBounds, Color.Green );
	}
}
