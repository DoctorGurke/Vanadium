namespace Vanadium.Renderer.Scene;

public class SceneObject
{
	
	public Material? MaterialOverride;
	public OpenTKMath.Matrix4 GlobalTransform => Parent is null ? LocalTransform.TransformMatrix : LocalTransform.TransformMatrix * Parent.GlobalTransform;
	public Color RenderColor = Color.White;
	public float TintAmount = 1.0f;

	public void SetMaterialOverride( string path )
	{
		MaterialOverride = Material.Load( path );
	}

	private Model? _model;
	public Model Model
	{
		get
		{
			return _model is null || _model.IsError ? Model.Primitives.Error : _model;
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

	private SceneObject? _parent;
	public SceneObject? Parent
	{
		get
		{
			return _parent;
		}
		set
		{
			// don't parent to itself
			if ( value == this ) return;

			// remove child ref from old parent list
			if ( _parent is not null )
				_parent.Children.Remove( this );

			// add new parent
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
		var transparent = false;

		if ( Model.Meshes is not null )
		{
			foreach ( var mesh in Model.Meshes )
			{
				if ( mesh.Material.Transparent )
				{
					transparent = true;
					break;
				}
			}
		}

		if ( transparent )
		{
			SceneWorld.Main?.AddTransparent( this );
		}
		else
		{
			SceneWorld.Main?.AddOpaque( this );
		}
	}

	public virtual void OnSpawn() { }

	public void Draw()
	{
		Model.Draw( DrawCommand.FromSceneObject( this ) );
		OnRender();
	}

	protected virtual void OnRender()
	{
		//if ( Model is not null )
		//DebugDraw.Box( Vector3.Zero * GlobalTransform, Model.RenderBounds, Color.Green );
	}
}
