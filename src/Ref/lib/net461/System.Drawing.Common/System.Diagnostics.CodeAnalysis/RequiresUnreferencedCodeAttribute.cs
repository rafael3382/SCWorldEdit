namespace System.Diagnostics.CodeAnalysis
{
	/// <summary>
	/// Indicates that the specified method requires dynamic access to code that is not referenced
	/// statically, for example through <see cref="N:System.Reflection" />.
	/// </summary>
	/// <remarks>
	/// This allows tools to understand which methods are unsafe to call when removing unreferenced
	/// code from an application.
	/// </remarks>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Constructor | AttributeTargets.Method, Inherited = false)]
	internal sealed class RequiresUnreferencedCodeAttribute : Attribute
	{
		/// <summary>
		/// Gets a message that contains information about the usage of unreferenced code.
		/// </summary>
		public string Message { get; }

		/// <summary>
		/// Gets or sets an optional URL that contains more information about the method,
		/// why it requries unreferenced code, and what options a consumer has to deal with it.
		/// </summary>
		public string Url { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="T:System.Diagnostics.CodeAnalysis.RequiresUnreferencedCodeAttribute" /> class
		/// with the specified message.
		/// </summary>
		/// <param name="message">
		/// A message that contains information about the usage of unreferenced code.
		/// </param>
		public RequiresUnreferencedCodeAttribute(string message)
		{
			Message = message;
		}
	}
}
