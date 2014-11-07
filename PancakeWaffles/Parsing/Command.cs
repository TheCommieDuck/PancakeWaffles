using PancakeWaffles.Verbs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeWaffles.Parsing
{
	class Command
	{
		public Verb Verb { get; set; }

		public List<NounPhrase> DirectObjects { get; set; }

		public List<NounPhrase> IndirectObjects { get; set; }

		public string Preposition { get; set; }

        public Command(Verb verb, string prep=Parser.NoPreposition)
        {
            Verb = verb;
            Preposition = prep;
            DirectObjects = new List<NounPhrase>();
            IndirectObjects = new List<NounPhrase>();
        }
	}
}
