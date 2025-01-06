using System;
using ReflectSettings.Attributes;

namespace ReflectSettings.EditableConfigs
{
	public class ButtonClick : NameAttribute
	{
		public ButtonClick(string name) : base(name)
		{
		}
	}
	public class GroupTag : NameAttribute
	{
		public GroupTag(string name) : base(name)
		{
		}
	}

}
