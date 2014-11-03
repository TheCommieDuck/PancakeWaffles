using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PancakeWaffles
{
	class Object
	{
		#region Properties
		
		

		public string Name { get; protected set; }

		public string Description { get; protected set; }

		public string PluralName
		{
			get
			{
				return pluralName ?? Name;
			}

			set
			{
				pluralName = value;
				HasPluralName = (value != null);
			}
		}

		public bool HasPluralName { get; protected set; }

		public bool HasSpecialIndefiniteArticle { get; protected set; }

		public string IndefiniteArticle
		{
			get
			{
				return indefiniteArticle ?? "a";
			}

			set
			{
				indefiniteArticle = value;
				HasSpecialIndefiniteArticle = (value != null);
			}
		}

		protected string pluralName = null;

		protected string indefiniteArticle = null;
		#endregion

		public Object(string name, string desc)
		{
			this.Name = name;
			this.Description = desc;
			Registry.RegisterObject(this);
		}
	}
}
