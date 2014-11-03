using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeWaffles.Things
{
	class Thing : Object
	{
		public virtual Object Location { get; set; }
		
		public Thing(string name, string description)
			: base(name, description)
		{
		}
	}
}
