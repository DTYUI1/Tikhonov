using System;

namespace VersionedStackProject
{
	class Program
	{
		static void Main()
		{
			Console.WriteLine("Версии документа");

			VersionedStack<string> documentChanges = new VersionedStack<string>();

			int v1 = documentChanges.Push("Создан");
			Console.WriteLine($"Версия {v1}: {documentChanges.GetVersion(v1)}");

			int v2 = documentChanges.Push("Добавлена информация");
			Console.WriteLine($"Версия {v2}: {documentChanges.GetVersion(v2)}");

			int v3 = documentChanges.Push("Что-то ещё");
			Console.WriteLine($"Версия {v3}: {documentChanges.GetVersion(v3)}");

			Console.WriteLine("\n2 Pop данных :");

			string undoneChange = documentChanges.Pop();
			Console.WriteLine($": {undoneChange}\n");

			documentChanges.DisplayAllVersions();
			documentChanges.DisplayCurrentStack();
		}
	}
}