using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ReflectSettings.Attributes
{
	[AttributeUsage(AttributeTargets.Property)]
	public class PredefinedValuesAttribute : Attribute
	{
		public IList<object> Values { get; }

		public PredefinedValuesAttribute(params object[] predefinedValues)
		{
			if (predefinedValues == null)
				Values = new List<object> {null};
			else
				Values = predefinedValues.ToList();
		}


		public PredefinedValuesAttribute()
		{
			Values = new List<object>();
		}
	}

	
	[AttributeUsage(AttributeTargets.Property)]
	public class ConfigTitle : Attribute
	{
		public string ConfigTitleName { get; }

		public ConfigTitle(params object[] title)
		{
			if (title == null || title.Length == 0)
				ConfigTitleName = null;
			else
				ConfigTitleName = title[0] as string;
		}
		public ConfigTitle()
		{
			ConfigTitleName = null;
		}
	}

	
}
