using PancakeWaffles.Verbs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeWaffles.Parsing
{
	class CommandPhrase
	{
		public Verb Verb { get; set; }

		public List<NounPhrase> DirectObjects { get; set; }

		public List<NounPhrase> IndirectObjects { get; set; }

		public string Preposition { get; set; }

        public CommandPhrase(Verb verb, string prep=Parser.NoPreposition)
        {
            Verb = verb;
            Preposition = prep;
            DirectObjects = new List<NounPhrase>();
            IndirectObjects = new List<NounPhrase>();
        }

		public string ToPrettyString()
		{
			return String.Format(@"
Verb: {0}
Preposition: {1}
Direct Objects: 
{2}
Indirect Objects:
{3}", Verb, Preposition == Parser.NoPreposition ? "No Preposition" : Preposition, string.Join(" ", DirectObjects), string.Join(" ", IndirectObjects));
		}
	}
}
