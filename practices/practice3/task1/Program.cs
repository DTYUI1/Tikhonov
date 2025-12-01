using System;

class Program
{
	static void Main()
	{
		Console.WriteLine("Введите строку:");
		string s = Console.ReadLine();

		string compressed = code(s);
		Console.WriteLine("Сжатая строка: " + compressed);

		string decompressed = decode(compressed);
		Console.WriteLine("Восстановленная строка: " + decompressed);
	}

	static string code(string s)
	{
		string res = "";
		char last = ' ';
		int last_counter = 1;

		for (int i = 0; i < s.Length; i++)
		{
			if (s[i] != last)
			{
				last = s[i];
				if (last_counter != 1)
				{
					res += last_counter;
				}
				res += s[i];
				last_counter = 1;
			}
			else
			{
				last_counter++;
			}
		}

		if (last_counter > 1)
		{
			res += last_counter;
		}

		return res;
	}

	static string decode(string s)
	{
		string res = "";
		char currentChar = ' ';
		string numStr = "";

		for (int i = 0; i < s.Length; i++)
		{
			if (char.IsLetter(s[i]))
			{
				if (currentChar != ' ' && numStr != "")
				{
					int count = int.Parse(numStr);
					for (int j = 0; j < count; j++)
					{
						res += currentChar;
					}
					numStr = "";
				}
				else if (currentChar != ' ')
				{
					res += currentChar;
				}

				currentChar = s[i];
			}
			else if (char.IsDigit(s[i]))
			{
				numStr += s[i];
			}
		}

		if (currentChar != ' ')
		{
			if (numStr != "")
			{
				int count = int.Parse(numStr);
				for (int j = 0; j < count; j++)
				{
					res += currentChar;
				}
			}
			else
			{
				res += currentChar;
			}
		}

		return res;
	}
}