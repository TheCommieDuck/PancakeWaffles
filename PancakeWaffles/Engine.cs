using PancakeWaffles.Parsing;
using PancakeWaffles.Rooms;
using PancakeWaffles.Things;
using PancakeWaffles.Verbs;
using System.Collections.Generic;

namespace PancakeWaffles
{
	class Engine
	{
		public static Region DefaultRegion { get; set; }

		private Queue<CommandPhrase> currentCommands;

		static void Main(string[] args)
		{
			Region world = new Region("world");
			Engine.DefaultRegion = world;

			Room lab = new Room(
				name: "Lab", 
				description: "The lab seems deserted; all the surfaces are covered in dust and grime."
				);

			Room hall = new Room(
				"hall",
				"Light streams in from outside the hall through the windows."
				);

			Verb open = new Verb(
				null, new[]{"open"});

			Verb throwVerb = new Verb(
				null, new[]{"throw"}, new[]{"at", "under", "over"}, true);

			Thing door = new Thing("door", "it's a door.");
			Thing sandwich = new Thing("chicken sandwich", null);
			Thing chicken = new Thing("chicken", null);
			Registry.StartLocation = lab;

			Engine engine = new Engine();

			engine.Start();
			
		}

		public void Start()
		{
			//note this contains parsed commands that may or may not actually work. They get checked on the fly.
			currentCommands = new Queue<CommandPhrase>();

			Player player = new Player();
			Parser parser = new Parser();
			player.Location = Registry.StartLocation;
			while (true)
			{
				Terminal.Write(">");
				string input = Terminal.Read();
				List<CommandPhrase> commandPhrases = parser.Parse(input);

				if(commandPhrases == null)
				{
					//do something about invalid input
					continue;
				}

				commandPhrases.ForEach(c => currentCommands.Enqueue(c));

				//now pump messages into the actual game loop
				while(commandPhrases.Count > 0)
				{
					CommandPhrase currentCommand = currentCommands.Dequeue();
					//if the current command is a meta-command, don't run through a turn.
					//though meta commands must not have objects, so.
					if(currentCommand.Verb.IsMetaCommand)
					{
						//do something..
					}
					else
					{

						RunGameTurn();
					}
				}
			}
		}

		public void RunGameTurn()
		{

		}
	}
}
