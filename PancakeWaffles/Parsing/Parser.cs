using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PancakeWaffles.Verbs;
using PancakeWaffles.Things;

namespace PancakeWaffles.Parsing
{
	class Parser
	{
		public static Dictionary<string, Verb> Verbs
		{
			get { return commands; }
		}

		public static HashSet<string> Prepositions
		{
			get { return prepositions; }
		}

		public static HashSet<string> Adjectives
		{
			get { return adjectives; }
		}

		public readonly static string[] Pronouns = new []{ "it", "him", "her", "them", "all", "everything", "me", "myself", "everyone", "everybody" };

		public readonly static string[] Conjunctions = new[] { "and", "," };

		public readonly static string[] CommandBreaks = new[] { "then", ".", "?", "!", ";" };

		public readonly static string[] IndiscriminateDeterminers = new[] { "a", "an"};

		public readonly static string[] Determiners = new[] { "the", "some", "every", "all" };

		public const string NoPreposition = "no_preposition";

		private static Dictionary<string, Verb> commands = new Dictionary<string, Verb>();

		private static HashSet<string> prepositions = new HashSet<string>(){ NoPreposition };

		private static HashSet<string> adjectives = new HashSet<string>();

		public static void RegisterVerb(Verb command)
		{
			command.Synonyms.ForEach(s => Verbs.Add(s, command));
			command.Prepositions.ForEach(p => Prepositions.Add(p));
		}

		public List<Command> Parse(string input)
		{
			//first, lowercase all words and split out punctuation
			List<Command> parsedCommands = new List<Command>();

			List<string> tokens = TokeniseInput(input);
			if(tokens == null)
			{
				return null;
			}

			//now consider every word for terminators to break into multiple commands. remove empty lists (e.g. 'go west.' gives 2 lists)
			List<List<string>> commands = SplitCommands(tokens);

			foreach(List<string> command in commands)
			{
				//delete trailing conjunctions ('and then')
				while (Conjunctions.Contains(command.LastOrDefault()))
					command.RemoveAt(command.Count - 1);
				command.RemoveAll(s => s == "");
				//first, check for invalid inputs
				foreach(string token in command)
				{
					if(!IsVocabulary(token))
					{
						//todo: invalid input found
						return null;
					}
				}
				//format is: [actor] verb [direct objects] [preposition] [indirect objects].
				//find a verb and save position. then save the first preposition
				string verb = command.FirstOrDefault(w => Verbs.ContainsKey(w));
				if (verb == null)
					return null; //invalid verb found, todo

				int verbLocation = command.IndexOf(verb);

				//now we have a verb, we find the first preposition
				string preposition = command.Skip(verbLocation+1).FirstOrDefault(w => Prepositions.Contains(w));
				int prepositionLocation = command.IndexOf(preposition);

				if (preposition == null)
				{
					preposition = NoPreposition;
					prepositionLocation = command.Count;
				}

				Verb verbObject = Verbs[verb];
				if(!verbObject.Prepositions.Contains(preposition))
				{
					//todo: invalid preposition (e.g. put axe from house)
					return null;
				}
				//so now we know our verb and preposition are valid (e.g. put axe on wall)
				//now identify the actor - if we can't get one, we know it's the player (i.e. null)
				//TODO: bother with this if we ever need it.

				//now consider the direct objects and the indirect objects
				List<List<string>> directObjects = IdentifyNouns(command, verbLocation, prepositionLocation);
				List<List<string>> indirectObjects = IdentifyNouns(command, prepositionLocation, command.Count);
				if (directObjects == null || indirectObjects == null)
					return null; //TODO: non-matching noun, OR we read an adjective wrong..

				//if the preposition follows the verb immediately, like 'look into box', then indirect objects are direct
				if (prepositionLocation - verbLocation == 1)
					directObjects = indirectObjects;

				parsedCommands.Add(new Command());
				
			}
			return parsedCommands;
		}

		public List<string> TokeniseInput(string input)
		{
			StringBuilder newInput = new StringBuilder();
			List<string> tokens = new List<string>();
			List<string> output = new List<string>();

			//first, split away the punctuation (,, ., etc)
			foreach (char symbol in input)
			{
				if (char.IsPunctuation(symbol))
					newInput.Append(" " + symbol + " ");
				else
					newInput.Append(symbol);
			}


			tokens = newInput.ToString().Split(' ').Select(t => t.ToLower()).ToList();
			List<string> originalInput = new List<string>(tokens);

			//while we have tokens to consider, consider a string of every token. check if it's a valid word; if it is, then remove those tokens
			//and check if the remaining words form token(s). otherwise, remove the last word and try again.
			//if at any point we have no tokens left in our consideration string, then it's not a valid configuration and we throw an error.
			while(tokens.Count > 0)
			{
				List<string> consider = new List<string>(tokens);
				while (consider.Count > 0)
				{
					//if it's a valid token, then add it to our output
					if (IsVocabulary(String.Join(String.Empty, consider)))
					{
						output.Add(String.Join(String.Empty, consider));
						tokens.RemoveRange(0, consider.Count);
						break;
					}
					consider.RemoveAt(consider.Count - 1);
				}

				//if our string for consideration is empty, then it was totaly invalid
				if(consider.Count == 0)
				{
					return null;
				}
			}

			return tokens;
		}

		public List<List<string>> IdentifyNouns(List<string> command, int verbLocation, int prepLocation)
		{
			//a noun can have the following form:
			//noun. brick. bricks.
			//article noun. a brick. 
			//quantifier/ambiguous-determiner noun. the brick/bricks. some bricks. 3 bricks. all bricks.
			//any of the above + adjectives noun. the red brick. some red bricks. 3 blue bricks. Cannot have anything between adjectives
			//and the noun except more adjectives.
			List<string> substr = command.Skip(verbLocation + 1).Take(prepLocation - verbLocation - 1).ToList();
			if (substr.Count == 0)
				return new List<List<string>>(); //does this ever happen? use on wall..
			List<List<string>> nouns = new List<List<string>>();
			List<string> currentNoun = new List<string>();
			foreach(string word in substr)
			{
				if (Registry.Objects.ContainsKey(word))
				{
					currentNoun.Add(word);
					nouns.Add(currentNoun);
					currentNoun = new List<string>();
				}
				else if (Adjectives.Contains(word))
					currentNoun.Add(word);
				else
					continue;
			}

			if(currentNoun.Count > 0)
			{
				//we have adjectives, but no matching noun..
				return null;
			}

			return nouns;
		}

		public List<List<string>> SplitCommands(List<string> tokens)
		{
			List<List<string>> chunkList = new List<List<string>>();
			List<string> currentChunk = new List<string>();
			foreach(string tok in tokens)
			{
				if (CommandBreaks.Contains(tok))
				{
					chunkList.Add(currentChunk);
					currentChunk = new List<string>();
					continue;
				}
				currentChunk.Add(tok);
			}
			chunkList.Add(currentChunk);
			return chunkList.Where(l => l.Count > 0).ToList();
		}

		public bool IsVocabulary(string word)
		{
			return (Registry.Objects.ContainsKey(word) || Adjectives.Contains(word) || Prepositions.Contains(word)
						|| Conjunctions.Contains(word) || Pronouns.Contains(word) || Verbs.ContainsKey(word) ||
						Determiners.Contains(word) || IndiscriminateDeterminers.Contains(word));
		}

		/*public List<string> GetFullyExpressedCommand(string input)
		{
			foreach(string tok in input.Split(' '))
			{
				string token = tok.ToLower();
				//todo: if the word is invalid and we don't recognise it, give up
				if(Pronouns.Contains(token))
				{
					//for the current room, consider 
				}
			}
			return null;
		}*/

		public void PerformUnknownCommand(string input)
		{
			Terminal.WriteLine("Sorry, I don't know what " + input + " means.");
		}
	}
}
