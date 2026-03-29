namespace Tools.Discovery;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class ToolGroupAttribute(string group) : Attribute
{
    public string Group { get; } = group;
}
