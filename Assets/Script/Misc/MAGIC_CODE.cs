namespace System.Runtime.CompilerServices
{
    class IsExternalInit
    {

    }
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false)]
    public sealed class RequiredMemberAttribute : Attribute
    {

    }
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public sealed class CompilerFeatureRequiredAttribute : Attribute
    {
        public string FeatureName { get; }
        public CompilerFeatureRequiredAttribute(string featureName)
        {
            FeatureName = featureName;
        }

        public bool IsOptional { get; set; }
        public const string RefStructs = "RefStructs";
        public const string RequiredMembers = "RequiredMembers";
    }
}