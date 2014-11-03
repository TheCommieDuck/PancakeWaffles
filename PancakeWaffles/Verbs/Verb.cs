using PancakeWaffles.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeWaffles.Verbs
{
	class Verb
	{
		public List<string> Synonyms { get; private set; }

		public Action<Object[]> Action { get; private set; }

		public List<string> Prepositions { get; private set; }

		public Verb(Action<Object[]> action, string[] synonyms, string[] prepositions=null, bool optionalNoPreposition=false)
		{
			Synonyms = new List<string>(synonyms);
			Prepositions = prepositions == null ? new List<string>(){ Parser.NoPreposition } : new List<string>(prepositions);
			if (optionalNoPreposition)
				Prepositions.Add(Parser.NoPreposition);
			Action = action;
			Parser.RegisterVerb(this);
		}
	}
}
