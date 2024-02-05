namespace System.Diagnostics.CodeAnalysis
{
	/// <summary>Specifies that null is allowed as an input even if the corresponding type disallows it.</summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, Inherited = false)]
	internal sealed class AllowNullAttribute : Attribute
	{
	}
}
