using System;
using System.Collections.Generic;

namespace VersionedStackProject
{
    public class VersionedStack<T>
    {
        private Stack<T> stack;
        private Dictionary<int, T> versions;
        private int versionCounter;

        public VersionedStack()
        {
            stack = new Stack<T>();
            versions = new Dictionary<int, T>();
            versionCounter = 0;
        }

        public int Push(T item)
        {
            stack.Push(item);
            versionCounter++;
            versions[versionCounter] = item;
            return versionCounter;
        }

        public T Pop()
        {
            if (stack.Count == 0) throw new Exception("Stek is empty");

            var item = stack.Pop();
            versionCounter++;
            versions[versionCounter] = item;
            return item;
        }

        public T GetVersion(int version)
        {
            if (versions.TryGetValue(version, out var item)) { return item; }
            throw new Exception($"Version {version} not found");
        }

        public void DisplayAllVersions()
        {
            Console.WriteLine("Версии:");
            foreach (var version in versions)
            {
                Console.WriteLine($"Версия {version.Key}: {version.Value}");
            }
        }

        public void DisplayCurrentStack()
        {
            Console.WriteLine("Текущий стек:");
            foreach (var item in stack)
            {
                Console.WriteLine($"  {item}");
            }
        }
    }
}