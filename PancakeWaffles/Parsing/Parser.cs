using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PancakeWaffles.Verbs;
using PancakeWaffles.Things;
using System.Text.RegularExpressions;

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

		private static HashSet<string> commandPatterns = new HashSet<string>();

		public static void RegisterVerb(Verb command)
		{
			command.Synonyms.ForEach(s => Verbs.Add(s, command));
			command.Prepositions.ForEach(p => Prepositions.Add(p));
		}

		public static void RegisterCommandPattern(string regex)
		{
			commandPatterns.Add(regex);
		}
		
		public PartOfSpeechType GetPartOfSpeech(string part)
		{
			if (Verbs.Keys.Contains(part))
				return PartOfSpeechType.Verb;
			if (Prepositions.Contains(part))
				return PartOfSpeechType.Preposition;
			if (Adjectives.Contains(part))
				return PartOfSpeechType.Adjective;
			if (Pronouns.Contains(part))
				return PartOfSpeechType.Pronoun;
			if (Conjunctions.Contains(part))
				return PartOfSpeechType.Conjunction;
			if (Determiners.Contains(part) || IndiscriminateDeterminers.Contains(part))
				return PartOfSpeechType.Determiner;
			if (Registry.Objects.Keys.Contains(part))
				return PartOfSpeechType.Noun;
			else
				return PartOfSpeechType.Error;
		}

		public Parser()
		{
			string actor = "(Actor )?";
			string verb = "(Verb)";
			string prep = "( Preposition)?";
			string innerNoun = "(?<innerNounPhrase>(Determiner )?((Adjective )*(Pronoun|Actor|Noun)))";
			string nounPhrase = "( (?<nounPhrase>"+innerNoun+"( Preposition "+innerNoun+")))?";
			string conjuction = "( Conjunction \\k<nounPhrase>)*";

			commandPatterns.Add(actor+verb+prep+nounPhrase+conjuction);
		}

		public List<Command> Parse(string input)
		{
			//first, lowercase all words and split out punctuation
			List<Command> parsedCommands = new List<Command>();

			List<List<Lexeme>> tokens = TokeniseInput(input);
			if (tokens == null)
			{
				Console.WriteLine("Invalid Input");
				return null;
			}
			else
			{
				tokens.ForEach(c => Console.WriteLine(c));
			}

			Regex regex = new Regex(commandPatterns.Aggregate("^", (built, pattern) => String.Join("|", built+"$", "^"+pattern), v => v+"$"));
			foreach(List<Lexeme> command in tokens)
			{
				//match lexemes with pattern matching
				string builtCommand = command.Aggregate("", (built, lex) => built + lex.Type.ToString() + " ").Trim();
				Match match = regex.Match(builtCommand);
				if(!match.Success)
				{
					Console.WriteLine("Invalid regex");
					//we didn't recognise that structure..
					return null;
				}
			}
			
			//plan for parsing - register a list of rules (e.g. verb, verb object)
			
			//don't care if they don't match up entirely - e.g. allow throw tasty yellow brick for bob
			//then have a sanity check afterwards to see whether it's random words put together

			/*if(tokens == null)
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
			return parsedCommands;*/
			return null;
		}

		public List<List<Lexeme>> TokeniseInput(string input)
		{
			StringBuilder newInput = new StringBuilder();
			List<string> words = new List<string>();
			List<List<Lexeme>> output = new List<List<Lexeme>>();

			//first, split away the punctuation (,, ., etc)
			foreach (char symbol in input)
					newInput.Append(char.IsPunctuation(symbol) ? (" " + symbol + " ") : new string(new []{symbol}));

			words = newInput.ToString().Split(' ').Select(t => t.ToLower()).ToList();

			List<List<string>> commands = SplitCommands(words);

			foreach (List<string> singleCommand in commands)
			{
				
				while(Conjunctions.Contains(singleCommand.Last()) || singleCommand.Last() == "")
					singleCommand.RemoveAt(singleCommand.Count - 1);
				List<Lexeme> singleCommandOutput = new List<Lexeme>();
				List<string> originalInput = new List<string>(singleCommand);

				while (singleCommand.Count > 0)
				{
					List<string> consider = new List<string>(singleCommand);

					while (consider.Count > 0)
					{
						string joinedConsider = String.Join(" ", consider).Trim();
						//if it's a valid token, then add it to our output
						if (IsVocabulary(joinedConsider))
						{
							singleCommandOutput.Add(new Lexeme(joinedConsider, GetPartOfSpeech(joinedConsider)));
							singleCommand.RemoveRange(0, consider.Count);
							break;
						}
						consider.RemoveAt(consider.Count - 1);
					}

					//if our string for consideration is empty, then it was invalid input
					if (consider.Count == 0)
					{
						return null;
					}
				}
				output.Add(singleCommandOutput);
			}
			return output;
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

		public void PerformUnknownCommand(string input)
		{
			Terminal.WriteLine("Sorry, I don't know what " + input + " means.");
		}
	}
}
