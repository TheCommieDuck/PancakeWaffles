using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PancakeWaffles.Things;

namespace PancakeWaffles.Rooms
{
	class Room : Object
	{
		public Dictionary<Direction, Room> Transitions { get; private set; }

		public Object ContainedIn { get; private set; }

		public Room(string name, string description, Object container=null)
			:base(name, description)
		{
			IndefiniteArticle = "The";
			ContainedIn = container;
		}

		public string DisplayDescription
		{
			get
			{
				return String.Format("{0}\n-----\n\n{1}\n", DisplayName, Description);
			}
		}

		public string DisplayName
		{
			get
			{
				return IndefiniteArticle + " " + Name;
			}
		}
	}
}
