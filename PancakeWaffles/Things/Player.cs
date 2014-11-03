using PancakeWaffles.Rooms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeWaffles.Things
{
	class Player : Thing
	{
		public override Object Location
		{
			get
			{
				return base.Location;
			}

			set
			{
				Room room = value as Room;
 				if(room == null)
					throw new ArgumentException("Cannot set player location to a region or thing.");
				Terminal.WriteLine(room.DisplayDescription);
			}
		}

		public Player()
			:base("player", "It's you, the player.")
		{

		}
	}
}
