using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace RegExplorer
{
	public class RichTextBoxHelper : DependencyObject
	{
		private static ConditionalWeakTable<RichTextBox, SupplementalProperties> _trackedControls = new ConditionalWeakTable<RichTextBox, SupplementalProperties>();

		private class SupplementalProperties
		{
			public bool IsInitialized { get; set; } = false;
		}

		public static readonly DependencyProperty FlowDocumentProperty = DependencyProperty.RegisterAttached(
			"FlowDocument",
			typeof(FlowDocument),
			typeof(RichTextBoxHelper),
			new FrameworkPropertyMetadata()
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

			if (!_trackedControls.GetOrCreateValue(richTextBox).IsInitialized)
			{
				richTextBox.TextChanged += RichTextBox_TextChanged;
				_trackedControls.GetOrCreateValue(richTextBox).IsInitialized = true;
			}
		}

		private static void RichTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			SetFlowDocument((sender as RichTextBox), (sender as RichTextBox).Document);
		}
	}
}
