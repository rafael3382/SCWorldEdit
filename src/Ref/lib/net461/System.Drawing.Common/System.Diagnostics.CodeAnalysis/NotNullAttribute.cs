namespace System.Diagnostics.CodeAnalysis
{
	/// <summary>Specifies that an output will not be null even if the corresponding type allows it. Specifies that an input argument was not null when the call returns.</summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.ReturnValue, Inherited = false)]
	internal sealed class NotNullAttribute : Attribute
	{
	}
}
