using Unnamed_Networking_Plugin;

namespace Unnamed_Networking_Plugin_Tests;

[System.Serializable]
public class TestPackage : Package
{
    public TestData TestData { get; init; }

    public TestPackage(TestData testData)
    {
        TestData = testData;
    }
}

[System.Serializable]
public class TestData
{
    public string text { get; init; }
    public int integer { get; init; }
    public float floatingPointInteger { get; set; }

    public TestData(string text, int integer, float floatingPointInteger)
    {
        this.text = text;
        this.integer = integer;
        this.floatingPointInteger = floatingPointInteger;
    }

    public override string ToString()
    {
        return $"text: {text}, integer: {integer}, floatingPointInteger: {floatingPointInteger}";
    }

    public override bool Equals(object? obj)
    {
        if (obj is TestData other)
        {
            // Console.WriteLine($"1: {other.text}, 2: {text}");
            // Console.WriteLine($"1: {other.integer}, 2: {integer}");
            // Console.WriteLine($"1: {other.floatingPointInteger}, 2: {floatingPointInteger}");
            return other.text == text && other.integer == integer && other.floatingPointInteger == floatingPointInteger;
        }

        return false;
    }
}