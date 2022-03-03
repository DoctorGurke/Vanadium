global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Diagnostics;

using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Mathematics;

namespace Vanadium;

public class Program {
    public static void Main(string[] args) {
		// init the settings for our main window
		var nativeWindowSettings = new NativeWindowSettings() {
			Size = new Vector2i(1920, 1080),
			Title = "Vanadium"
		};

		// init and run our window type
		using(var window = new Window(GameWindowSettings.Default, nativeWindowSettings)) {
			window.Run();
		}
	}
}
