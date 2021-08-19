using System;
using EnumNameGenerator;
using MyEnumNamespace;

namespace EnumNameGeneratorSample
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(MyEnum.Value1.GetName());
        }
    }
}

namespace MyEnumNamespace
{
    [GenerateEnumNames]
    public enum MyEnum
    {
        Value1 = 1,
        Value2,
        Value3,
        Value4
    }
}