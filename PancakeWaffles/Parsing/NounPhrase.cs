using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeWaffles.Parsing
{
	//a noun phrase isn't an in-game object; it's a noun string + a bunch of adjectives
	class NounPhrase
	{
		public List<string> Adjectives { get; set; }

		public string Noun { get; set; }

        public bool IsIndiscriminateDeterminer { get; set; }

        public string Determiner { get; set; }

		public override string ToString()
		{
			return (!IsIndiscriminateDeterminer ? Determiner : "") + string.Join(" ", Adjectives) + " " + Noun;
		}
	}
}
