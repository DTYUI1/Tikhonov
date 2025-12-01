class Program
{
    static void Main()
    {
        Console.WriteLine("Введите выражение:");
        string expr = Console.ReadLine();

        if (CheckExpression(expr))
        {
            Console.WriteLine("Выражение корректно");
        }
        else
        {
            Console.WriteLine("Выражение НЕ корректно");
        }
    }

    static bool CheckExpression(string expr)
    {
        if (string.IsNullOrEmpty(expr)) return false;

        int bracketCount = 0;
        foreach (char c in expr)
        {
            if (c == '(') bracketCount++;
            else if (c == ')') bracketCount--;

            if (bracketCount < 0) return false;
        }
        if (bracketCount != 0) return false;

        if (System.Text.RegularExpressions.Regex.IsMatch(expr[^1].ToString(), @"[+\-*/]"))
            return false;

        string pattern = @"([+\-*/]{2,})|(\.[0-9]*\.)|([+\-*/]\))";
        if (System.Text.RegularExpressions.Regex.IsMatch(expr, pattern))
            return false;

        if (System.Text.RegularExpressions.Regex.IsMatch(expr[0].ToString(), @"[*/)]"))
            return false;

        string validChars = @"^[0-9+\-*/().\s]+$";
        if (!System.Text.RegularExpressions.Regex.IsMatch(expr, validChars))
            return false;

        if (expr.Contains("()")) return false;

        return true;
    }
}
