namespace System.Runtime.Versioning
{
	/// <summary>
	/// Marks APIs that were removed in a given operating system version.
	/// </summary>
	/// <remarks>
	/// Primarily used by OS bindings to indicate APIs that are only available in
	/// earlier versions.
	/// </remarks>
	[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Interface, AllowMultiple = true, Inherited = false)]
	internal sealed class UnsupportedOSPlatformAttribute : OSPlatformAttribute
	{
		public UnsupportedOSPlatformAttribute(string platformName)
			: base(platformName)
		{
		}
	}
}
