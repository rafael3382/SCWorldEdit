namespace System.Runtime.Versioning
{
	/// <summary>
	/// Base type for all platform-specific API attributes.
	/// </summary>
	internal abstract class OSPlatformAttribute : Attribute
	{
		public string PlatformName { get; }

		private protected OSPlatformAttribute(string platformName)
		{
			PlatformName = platformName;
		}
	}
}
