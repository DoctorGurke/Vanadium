namespace Vanadium;

public static class Time {

	public static float Delta { get; private set; }

	public static void Update(float delta) {
		Delta = delta;
	}
}
