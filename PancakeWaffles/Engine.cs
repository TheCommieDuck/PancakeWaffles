using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PancakeWaffles.Parsing;
using PancakeWaffles.Rooms;
using PancakeWaffles.Things;
using PancakeWaffles.Verbs;

namespace PancakeWaffles
{
	class Engine
	{
		public static Region DefaultRegion { get; set; }

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
			Player player = new Player();
			Parser parser = new Parser();
			player.Location = Registry.StartLocation;
			while (true)
			{
				Terminal.Write(">");
				string input = Terminal.Read();
				parser.Parse(input);
			}
		}
	}
}
