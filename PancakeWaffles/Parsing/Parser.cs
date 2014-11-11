using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using PancakeWaffles.Verbs;

namespace PancakeWaffles.Parsing
{
	class Parser
	{
		public static Dictionary<string, List<Verb>> Verbs
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

		private static Dictionary<string, List<Verb>> commands = new Dictionary<string, List<Verb>>();

		private static HashSet<string> prepositions = new HashSet<string>(){ NoPreposition };

		private static HashSet<string> adjectives = new HashSet<string>();

		private static HashSet<string> commandPatterns = new HashSet<string>();

		public static void RegisterVerb(Verb command)
		{
			command.Synonyms.ForEach(s => 
			{
				if (Verbs.ContainsKey(s))
					Verbs[s].Add(command);
				else
					Verbs.Add(s, new List<Verb>() { command });
			});
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

        public List<List<Lexeme>> TokeniseInput(string input)
        {
            StringBuilder newInput = new StringBuilder();
            List<string> words = new List<string>();
            List<List<Lexeme>> output = new List<List<Lexeme>>();

            //first, split away the punctuation (,, ., etc)
            foreach (char symbol in input)
                newInput.Append(char.IsPunctuation(symbol) ? (" " + symbol + " ") : new string(new[] { symbol }));

            words = newInput.ToString().Split(' ').Select(t => t.ToLower()).ToList();

            List<List<string>> commands = SplitCommands(words, CommandBreaks);

            foreach (List<string> singleCommand in commands)
            {

                while (Conjunctions.Contains(singleCommand.Last()) || singleCommand.Last() == "")
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

		public Parser()
		{
			//plan for parsing - register a list of rules (e.g. verb, verb object)
			string actor = "^(Actor )?";
			string verb = "(Verb)";
			string prep = "( Preposition)?";
			string innerNoun = "(?<innerNounPhrase>(Determiner )?((Adjective )*(Pronoun|Actor|Noun)))";
			string nounPhrase = "( (?<nounPhrase>"+innerNoun+"( Preposition "+innerNoun+")?))?";
			string conjuction = "( Conjunction \\k<innerNounPhrase>)*$";

			//[Actor] Verb [Preposition] [([Determiner] [Adjective]* Pronoun/Actor/Noun)] [Preposition nounphrase] [conjunction nounphrase]
			commandPatterns.Add(actor+verb+prep+nounPhrase+conjuction);
		}

		public List<CommandPhrase> Parse(string input)
		{
			//first, lowercase all words and split out punctuation
			List<CommandPhrase> parsedCommands = new List<CommandPhrase>();

			List<List<Lexeme>> tokens = TokeniseInput(input);
			if (tokens == null)
			{
				Console.WriteLine("Invalid Input");
				return null;
			}

			Regex regex = new Regex(commandPatterns.Aggregate((built, pattern) => String.Join("|", built+"$", "^"+pattern)));

			foreach(List<Lexeme> command in tokens)
			{
				//match lexemes with pattern matching
				string builtCommand = command.Aggregate("", (built, lex) => built + lex.Type.ToString() + " ").Trim();
				
				//find the first verb (since, e.g. tell bob to eat a pie will have 2 verbs)
				Lexeme verbLex = command.FirstOrDefault(l => l.Type == PartOfSpeechType.Verb);
				if(verbLex == null)
				{
					//TODO SOME ERROR ABOUT NO VERB
				}
				List<Verb> matchingVerbs = Verbs[verbLex.Value];
				
				//iterate all verb forms we could possibly have



				/*//don't care if they don't match up entirely - e.g. allow throw tasty yellow brick for bob
				Match match = regex.Match(builtCommand);
				if(!match.Success)
				{
					Console.WriteLine("Invalid regex");
					//we didn't recognise that structure..
					return null;
				}

				//then have a sanity check afterwards to see whether it's random words put together
				//everything has a verb, we know that for sure.
                Lexeme verbLex = command.First(l => l.Type == PartOfSpeechType.Verb);
				Verb verb = Verbs[verbLex.Value];

				//if we have no actor, then the actor is the player
				//TODO
				Lexeme actorLex = command.FirstOrDefault(l => l.Type == PartOfSpeechType.Actor);
                string actor = (actorLex != null ? actorLex.Value : "player_placeholder");

				Lexeme prepLex = command.FirstOrDefault(l => l.Type == PartOfSpeechType.Preposition);
				string prep = (prepLex != null ? prepLex.Value : NoPreposition);

				if(!verb.Prepositions.Contains(prep))
				{
					//nonmatching preposition
					Terminal.WriteLine("Prep doesn't match");
					return null;
				}
				
				//so if we have a preposition, then it's going to be verb prep, verb prep object, or verb object prep object
                //verb object - only dobj
                //verb prep object - no dobj, only iobj - which are actually dobj
                //verb object prep object - dobj, iobj
                IEnumerable<Lexeme> directObjects = command.Skip(command.IndexOf(verbLex) + 1).TakeWhile(l => !l.Equals(prepLex));
                IEnumerable<Lexeme> indirectObjects = prep == NoPreposition ? new List<Lexeme>() :
                    command.Skip(command.IndexOf(prepLex));

                //if we have verb prep object, we switch
                if (directObjects.Count() == 0 && indirectObjects.Count() > 0)
                {
                    directObjects = indirectObjects;
                    indirectObjects = new List<Lexeme>();
                }

                CommandPhrase commandObject = new CommandPhrase(verb, prep) 
                { DirectObjects = IdentifyNounPhrases(directObjects.ToList()),
                  IndirectObjects = IdentifyNounPhrases(indirectObjects.ToList())
                };
                parsedCommands.Add(commandObject);*/
			}
            return parsedCommands;
		}

        public List<NounPhrase> IdentifyNounPhrases(List<Lexeme> objects)
        {
            //no nouns
            if (objects.Count == 0)
                return new List<NounPhrase>();

            List<NounPhrase> phrases = new List<NounPhrase>();

            //structure of a nounphrase: noun | adjective noun | article noun | article adjective noun | nounphrase conjunction nounphrase
            //so split on conjunctions
            List<List<Lexeme>> separatedObjectLexemes = new List<List<Lexeme>>();
            List<Lexeme> currentObject = new List<Lexeme>();
            foreach(Lexeme lex in objects)
            {
                if(lex.Type == PartOfSpeechType.Conjunction)
                {
                    separatedObjectLexemes.Add(currentObject);
                    currentObject = new List<Lexeme>();
                }
                else
                    currentObject.Add(lex);
            }
            separatedObjectLexemes.Add(currentObject);

            foreach (List<Lexeme> objectPhrase in separatedObjectLexemes)
            {
                NounPhrase noun = new NounPhrase()
                {
                    Adjectives = objectPhrase.Where(l => l.Type == PartOfSpeechType.Adjective).Select(l => l.Value).ToList(),
                    Noun = objectPhrase.First(l => l.Type == PartOfSpeechType.Noun || l.Type == PartOfSpeechType.Pronoun
                        || l.Type == PartOfSpeechType.Actor).Value
                };
                Lexeme detLex = objectPhrase.FirstOrDefault(l => l.Type == PartOfSpeechType.Determiner);
                //use 'a' for blank ones - e.g. throw brick becomes throw a brick
                noun.Determiner = detLex == null ? "a" : detLex.Value;
                noun.IsIndiscriminateDeterminer = IndiscriminateDeterminers.Contains(noun.Determiner);
                phrases.Add(noun);
            }
            return phrases;
        }

		public List<List<string>> SplitCommands(List<string> tokens, IEnumerable<string> breaks)
		{
			List<List<string>> chunkList = new List<List<string>>();
			List<string> currentChunk = new List<string>();
			foreach(string tok in tokens)
			{
				if (breaks.Contains(tok))
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