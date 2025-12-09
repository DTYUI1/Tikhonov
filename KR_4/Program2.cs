// See https://aka.ms/new-console-template for more information
//Console.WriteLine("Hello, World!");


class Program
{
    static void Main()
    {
        Console.WriteLine("Создание объекта");
        object obj = new object();
        int gen = GC.GetGeneration(obj); 
        long memory = GC.GetTotalMemory(false);  
        Console.WriteLine(memory.ToString()); // Занятая память до вызова сборщика мусора

        GC.Collect(0); // Убираем нулевое поколение
        memory = GC.GetTotalMemory(false);
        Console.WriteLine(memory.ToString());
        
        GC.Collect(1); // Уюирает первое поколение
        Console.WriteLine(obj.GetType().Name);
        memory = GC.GetTotalMemory(false);
        Console.WriteLine(memory.ToString());

        obj = null;
        GC.Collect(2); // Убираем второе поколение

        GC.WaitForPendingFinalizers();
        //Console.WriteLine(obj.GetType().Name); - выведет ошибку, т.к сборщик отчистит нуловый object


        memory = GC.GetTotalMemory(false); 
        Console.WriteLine(memory.ToString()); // Занятая память после вызова сборщика мусора


        // Выводе можно заметить, что после каждого вызова сборщика мусора, всё меньше и меньше используется памяти, что и логично
    }



}


