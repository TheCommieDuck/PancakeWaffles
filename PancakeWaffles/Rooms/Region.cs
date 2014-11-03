using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeWaffles.Rooms
{
	class Region : Object
	{
		public HashSet<Room> Rooms { get; private set; }

		public Region(string name, string description=null)
			:base(name, description)
		{
			Rooms = new HashSet<Room>();
		}

		public void AddRoom(Room room)
		{
			Rooms.Add(room);
		}
	}
}
