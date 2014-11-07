using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeWaffles.Parsing
{
	class Lexeme
	{
		public PartOfSpeechType Type;
		public string Value;

		public Lexeme(string value, PartOfSpeechType type)
		{
			Value = value;
			Type = type;
		}
	}
}
