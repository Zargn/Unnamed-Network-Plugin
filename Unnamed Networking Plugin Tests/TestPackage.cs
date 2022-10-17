using Unnamed_Networking_Plugin;

namespace Unnamed_Networking_Plugin_Tests;

[Serializable]
public class TestPackage : Package
{
    public TestData TestData { get; init; }

    public TestPackage(TestData testData)
    {
        TestData = testData;
    }
}

[Serializable]
public class TestData
{
    public string Text { get; init; }
    public int Integer { get; init; }
    public float FloatingPointInteger { get; set; }

    public TestData(string text, int integer, float floatingPointInteger)
    {
        this.Text = text;
        this.Integer = integer;
        this.FloatingPointInteger = floatingPointInteger;
    }

    public override string ToString()
    {
        return $"text: {Text}, integer: {Integer}, floatingPointInteger: {FloatingPointInteger}";
    }

    public override bool Equals(object? obj)
    {
        if (obj is TestData other)
        {
            return other.Text == Text && other.Integer == Integer && other.FloatingPointInteger == FloatingPointInteger;
        }

        return false;
    }
}