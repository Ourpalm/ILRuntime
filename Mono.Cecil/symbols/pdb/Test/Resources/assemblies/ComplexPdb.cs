using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static System.Console;
using Foo1 = System.Console;
using Foo2 = System.Reflection;

namespace ComplexPdb
{
    public class Program
    {
        private static void Foo()
        {
            const string s = "const string";
            const int f = 1;
            const object o = null;
            const string u = null;
            WriteLine(s);
            WriteLine(2);
        }

        private static void Bar(int i)
        {
            int result;
            if (i < 0)
            {
                int s = 1;
                result = s;
            }
            else
            {
                const int b = 12;
                const decimal c = 74;
                int s = -1;
                result = s + b + (int)c;
                if (result > b)
                {
                    int u = result * b;
                    Console.WriteLine(u);
                }
            }
            WriteLine(result);
        }

        public async Task TestAsync()
        {
            WriteLine("Hello1");
            await Task.Delay(100);

            for (int i = 0; i < 4; ++i)
            {
                WriteLine("Hello2");
                await Task.Delay(100);
                WriteLine("Hello3");
            }
        }

        public IEnumerable<string> TestIterator()
        {
            WriteLine("Hello1");
            yield return "Hello1";

            for (int i = 0; i < 4; ++i)
            {
                WriteLine("Hello1");
                yield return "Hello2";
                WriteLine("Hello3");
            }
        }
    }
}
