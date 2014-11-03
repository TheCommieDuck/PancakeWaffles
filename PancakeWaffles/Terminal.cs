using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeWaffles
{
	static class Terminal
	{
		public static void Write(string output)
		{
			Console.Write(output);
		}

		public static void WriteLine(string output)
		{
			Console.WriteLine(output);
		}

		public static string Read()
		{
			return Console.ReadLine();
		}
	}
}
