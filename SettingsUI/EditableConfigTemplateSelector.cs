using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ReflectSettings.Attributes;
using ReflectSettings.EditableConfigs;

namespace FrontendDemo
{
	class EditableConfigTemplateSelector : DataTemplateSelector
	{
		public static EditableConfigTemplateSelector Instance { get; } = new EditableConfigTemplateSelector();

		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			var element = container as FrameworkElement;

			switch (item)
			{
				case EditableSecureString editableSecureString:
					return element?.TryFindResource("EditableSecureStringTemplate") as DataTemplate;
				case EditableString editableString:
					if (editableString.HasPredefinedValues)
						return element?.TryFindResource("EditableStringComboboxTemplate") as DataTemplate;
					else
						return DataTemplateByName(editableString, element);
				case IEditableKeyValuePair _:
					return element?.TryFindResource("EditableKeyValuePairTemplate") as DataTemplate;
				case IReadOnlyEditableCollection _:
					return element?.TryFindResource("ReadOnlyEditableCollectionTemplate") as DataTemplate;
				case IEditableCollection _:
					return element?.TryFindResource("EditableCollectionTemplate") as DataTemplate;
				// 新增的自定义的类型，需要这么去判断
				case EditableComplex<ButtonClick> _:
					return element?.TryFindResource("EditableJustButtonTemplate") as DataTemplate;
				case IEditableComplex editableComplex:
					if (editableComplex.HasPredefinedValues)
						return element?.TryFindResource("EditableComplexComboboxTemplate") as DataTemplate;
					else
						return element?.TryFindResource("EditableComplexTemplate") as DataTemplate;
				case IEditableEnum _:
					return element?.TryFindResource("EditableEnumTemplate") as DataTemplate;
				default:
					return DataTemplateByName(item, element);
			}
		}


		private static DataTemplate DataTemplateByName(object item, FrameworkElement element)
		{
			var type = item.GetType();
			var name = type.Name;
			var expectedKey = $"{name}Template";

			return element.TryFindResource(expectedKey) as DataTemplate;
		}
	}
}
