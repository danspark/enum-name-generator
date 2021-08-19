using System;

namespace EnumNameGenerator
{
    [AttributeUsage(AttributeTargets.Enum)]
    public class GenerateEnumNamesAttribute : Attribute { }
}