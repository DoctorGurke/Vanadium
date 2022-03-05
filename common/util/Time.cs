namespace Vanadium;

public static class Time {

	public static float Delta { get; private set; }
	public static float Now { get; private set; }

	public static void Update(float delta, float now) {
		Delta = delta;
		Now = now;
	}
}
