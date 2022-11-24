namespace ForwardingClientExample.CommandSystem;

public static class CommandExtractor
{
    public static string GetCommandName(string inputString)
    {
        string command;
        // List<string> arguments;
        
        for (int i = 0; i < inputString.Length; i++)
        {
            if (inputString[i] == ' ')
            {
                command = inputString.Substring(0, i);
                return command;
            }
        }

        return inputString;
    }

    public static List<string> GetArguments(string inputString)
    {
        int startIndex = 0;

        List<string> arguments = new();

        for (int i = 0; i < inputString.Length; i++)
        {
            if (inputString[i] != '*')
            {
                continue;
            }

            if (startIndex == 0)
            {
                startIndex = i + 1;
                continue;
            }

            arguments.Add(inputString.Substring(startIndex, i - startIndex));
            startIndex = 0;
        }

        return arguments;
    }
}