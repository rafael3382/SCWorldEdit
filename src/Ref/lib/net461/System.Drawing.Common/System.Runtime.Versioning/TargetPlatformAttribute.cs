namespace System.Runtime.Versioning
{
	/// <summary>
	/// Records the platform that the project targeted.
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
	internal sealed class TargetPlatformAttribute : OSPlatformAttribute
	{
		public TargetPlatformAttribute(string platformName)
			: base(platformName)
		{
		}
	}
}
