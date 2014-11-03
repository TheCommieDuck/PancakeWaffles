using PancakeWaffles.Rooms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeWaffles
{
	static class Registry
	{
		public static Dictionary<string, Object> Objects { get; private set; }

		public static Region DefaultRegion { get; set; }

		public static Room StartLocation { get; set; }

		public static bool RegisterObject(Object obj)
		{
			if(obj is Room)
			{
				Room room = (Room)obj;
				Object container = room.ContainedIn;
				if (container != null)
					((Region)container).AddRoom(room);
				else
				{
					if (DefaultRegion != null)
						DefaultRegion.AddRoom(room);
				}
			}

			if (Objects.ContainsKey(obj.Name))
				return false;
			Objects.Add(obj.Name, obj);
			return true;
		}

		static Registry()
		{
			Objects = new Dictionary<string, Object>();
		}
	}
}
