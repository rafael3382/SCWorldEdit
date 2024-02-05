namespace System.Runtime.Versioning
{
	/// <summary>
	/// Annotates the custom guard field, property or method with an unsupported platform name and optional version.
	/// Multiple attributes can be applied to indicate guard for multiple unsupported platforms.
	/// </summary>
	/// <remarks>
	/// Callers can apply a <see cref="T:System.Runtime.Versioning.UnsupportedOSPlatformGuardAttribute" /> to a field, property or method
	/// and use that  field, property or method in a conditional or assert statements as a guard to safely call APIs unsupported on those platforms.
	///
	/// The type of the field or property should be boolean, the method return type should be boolean in order to be used as platform guard.
	/// </remarks>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = false)]
	internal sealed class UnsupportedOSPlatformGuardAttribute : OSPlatformAttribute
	{
		public UnsupportedOSPlatformGuardAttribute(string platformName)
			: base(platformName)
		{
		}
	}
}
