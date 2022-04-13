﻿global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Diagnostics;

using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Vanadium;

public class Program
{
	public static void Main( string[] args )
	{
		Assert.ResourcePresent( Shader.Error );
		Assert.ResourcePresent( Material.Error );
		Assert.ResourcePresent( Model.Error );
		Assert.ResourcePresent( Texture.Error );

		// init the settings for our main window
		var nativeWindowSettings = new NativeWindowSettings()
		{
			Size = new Vector2i( 1280, 800 ),
			Title = "Vanadium",
			WindowState = WindowState.Normal,
			StartFocused = true,
			NumberOfSamples = 4
		};


		GLFW.WindowHint( WindowHintBool.SrgbCapable, true );

		// init and run our window type
		using ( var window = new Window( GameWindowSettings.Default, nativeWindowSettings ) )
		{
			window.CenterWindow();
			window.VSync = VSyncMode.On;
			window.Run();
		}
	}
}
