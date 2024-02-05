namespace System.Diagnostics.CodeAnalysis
{
	/// <summary>Specifies that null is disallowed as an input even if the corresponding type allows it.</summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, Inherited = false)]
	internal sealed class DisallowNullAttribute : Attribute
	{
	}
}
