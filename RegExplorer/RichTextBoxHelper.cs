using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace RegExplorer
{
	public class RichTextBoxHelper : DependencyObject
	{
		public static readonly DependencyProperty FlowDocumentProperty = DependencyProperty.RegisterAttached(
			"FlowDocument",
			typeof(FlowDocument),
			typeof(RichTextBoxHelper),
			new FrameworkPropertyMetadata
			{
				BindsTwoWayByDefault = true,
				PropertyChangedCallback = FlowDocumentPropertyChanged
			});

		public static FlowDocument GetFlowDocument(DependencyObject obj)
		{
			return (FlowDocument)obj.GetValue(FlowDocumentProperty);
		}

		public static void SetFlowDocument(DependencyObject obj, FlowDocument value)
		{
			obj.SetValue(FlowDocumentProperty, value);
		}

		private static void FlowDocumentPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			var richTextBox = (RichTextBox)obj;
			richTextBox.Document = GetFlowDocument(obj);
		}
	}
}
