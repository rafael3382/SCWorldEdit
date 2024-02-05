namespace System.Runtime.Versioning
{
	/// <summary>
	/// Records the operating system (and minimum version) that supports an API. Multiple attributes can be
	/// applied to indicate support on multiple operating systems.
	/// </summary>
	/// <remarks>
	/// Callers can apply a <see cref="T:System.Runtime.Versioning.SupportedOSPlatformAttribute" />
	/// or use guards to prevent calls to APIs on unsupported operating systems.
	///
	/// A given platform should only be specified once.
	/// </remarks>
	[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Interface, AllowMultiple = true, Inherited = false)]
	internal sealed class SupportedOSPlatformAttribute : OSPlatformAttribute
	{
		public SupportedOSPlatformAttribute(string platformName)
			: base(platformName)
		{
		}
	}
}
