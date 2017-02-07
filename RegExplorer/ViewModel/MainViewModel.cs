using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Linq;
using System.Windows;
using System;

namespace RegExplorer.ViewModel
{
	/// <summary>
	/// This class contains properties that the main View can data bind to.
	/// <para>
	/// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
	/// </para>
	/// <para>
	/// You can also use Blend to data bind with the tool's support.
	/// </para>
	/// <para>
	/// See http://www.galasoft.ch/mvvm
	/// </para>
	/// </summary>
	public class MainViewModel : ViewModelBase
	{
		#region Fields

		private string _regularExpression;
		//private string _searchText;
		private FlowDocument _searchTextDocument;
		private int _currentMatchIndex;

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the MainViewModel class.
		/// </summary>
		public MainViewModel()
		{
			if (IsInDesignMode)
			{
				// Code runs in Blend --> create design time data.
			}
			else
			{
				_regularExpression = string.Empty;
				//_searchText = string.Empty;
				_searchTextDocument = new FlowDocument();
				_currentMatchIndex = 0;
				Matches = new ObservableCollection<MatchInfo>();
				UpdateSearchHighlighting();

				OpenFileCommand = new RelayCommand(OpenFile);
				RunSearchCommand = new RelayCommand(RunSearch);
				FirstMatchCommand = new RelayCommand(() => CurrentMatchIndex = 0);
				PreviousMatchCommand = new RelayCommand(() => CurrentMatchIndex--);
				NextMatchCommand = new RelayCommand(() => CurrentMatchIndex++);
				LastMatchCommand = new RelayCommand(() => CurrentMatchIndex = -1);
			}
		}

		#endregion

		#region Properties

		public string Name
		{
			get
			{
				return "RegExplorer";
			}
		}

		public string RegularExpression
		{
			get
			{
				return _regularExpression;
			}
			set
			{
				if (_regularExpression != value)
				{
					_regularExpression = value;
					RaisePropertyChanged(() => RegularExpression);
					UpdateSearchHighlighting();
				}
			}
		}

		//public string SearchText
		//{
		//	get
		//	{
		//		return _searchText;
		//	}
		//	set
		//	{
		//		if (_searchText != value)
		//		{
		//			_searchText = value;
		//			RaisePropertyChanged(() => SearchText);
		//			UpdateSearchHighlighting();
		//		}
		//	}
		//}

		public int CurrentMatchIndex
		{
			get
			{
				return _currentMatchIndex;
			}
			set
			{
				if (_currentMatchIndex != value)
				{
					ChangeMatchHighlighting(_currentMatchIndex, Colors.Yellow);

					// Don't use the % operator, as there might not be any matches.
					// 0 % 0 = ????
					if (value < 0)
					{
						if (Matches.Count == 0)
						{
							value = 0;
						}
						else
						{
							value = Matches.Count - 1;
						}
					}
					else if (value >= Matches.Count)
					{
						value = 0;
					}

					_currentMatchIndex = value;
					RaisePropertyChanged(() => CurrentMatchIndex);
					BringCurrentMatchIntoView();

					ChangeMatchHighlighting(_currentMatchIndex, Colors.Red);
				}
			}
		}

		public FlowDocument SearchTextDocument
		{
			get
			{
				return _searchTextDocument;
			}
			set
			{
				if (_searchTextDocument != value)
				{
					_searchTextDocument = value;
					RaisePropertyChanged(() => SearchTextDocument);
				}
			}
		}

		public ICommand OpenFileCommand { get; private set; }

		public ICommand RunSearchCommand { get; private set; }

		public ICommand FirstMatchCommand { get; private set; }

		public ICommand PreviousMatchCommand { get; private set; }

		public ICommand NextMatchCommand { get; private set; }

		public ICommand LastMatchCommand { get; private set; }

		public ObservableCollection<MatchInfo> Matches { get; private set; }

		#endregion

		#region Methods

		private void OpenFile()
		{
			var dlg = new OpenFileDialog()
			{
				Multiselect = false
			};
			var result = dlg.ShowDialog();
			if (result.GetValueOrDefault(false))
			{
				using (var reader = new StreamReader(dlg.OpenFile()))
				{
					SearchTextDocument = new FlowDocument();
					SearchTextDocument.Blocks.Clear();
					SearchTextDocument.Blocks.Add(new Paragraph(new Run(reader.ReadToEnd())));
					RaisePropertyChanged(() => SearchTextDocument);
				}
			}
		}

		private void RunSearch()
		{
			UpdateSearchHighlighting();
		}

		private void UpdateSearchHighlighting()
		{
			var text = new TextRange(SearchTextDocument.ContentStart, SearchTextDocument.ContentEnd).Text;
			SearchTextDocument.Blocks.Clear();
			//SearchTextDocument.Blocks.Add(new Paragraph(new Run(text)));

			var para = new Paragraph();
			var lastMatchEnd = 0;
			
			if (!string.IsNullOrWhiteSpace(_regularExpression))
			{
				Matches.Clear();

				try
				{
					var matches = Regex.Matches(text, _regularExpression ?? string.Empty, RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace);

					foreach (var match in matches.Cast<Match>())
					{
						var startIndex = match.Index;
						var length = match.Length;

						// Add the un-highlighted text that wasn't matched.
						var lastMatchLength = match.Index - lastMatchEnd;
						para.Inlines.Add(CreateInline(text.Substring(lastMatchEnd, lastMatchLength), Colors.Transparent));

						lastMatchEnd = startIndex + length;

						//var lastMatchLineNumber = text.Take(lastMatchEnd).Count(ch => ch == '\n') + 1;

						// Add the highlighted match text.
						var matchLineNumber = text.Take(match.Index).Count(ch => ch == '\n') + 1;

						var matchedText = CreateInline(text.Substring(startIndex, length), Colors.Yellow);
						para.Inlines.Add(matchedText);

						Matches.Add(new MatchInfo()
						{
							LineNumber = matchLineNumber,
							ColumnNumber = match.Index - ((match.Index == text.Length) ? match.Index : text.LastIndexOf('\n', match.Index)),
							Match = match,
							Text = matchedText
						});
					}
				}
				catch (Exception)
				{
					// Probably a badly-formatted regular expression.
				}
			}

			// Add any remaining text.
			para.Inlines.Add(new Run(text.Substring(lastMatchEnd)));

			SearchTextDocument.Blocks.Add(para);

			_currentMatchIndex = 0;
			BringCurrentMatchIntoView();

			RaisePropertyChanged(() => SearchTextDocument);
		}

		private Inline CreateInline(string text, Color backgroundColor)
		{
			var span = new Span();
			span.Background = new SolidColorBrush(backgroundColor);

			span.Inlines.Add(new Run(text));

			// TODO: Add the ability to show the current line number.
			//var lines = text.Split('\n');
			//foreach (var line in lines)
			//{
			//	span.Inlines.Add(new Run(string.Format("{0}:\t", lineNumber))
			//	{
			//		Background = new SolidColorBrush(Colors.Transparent),
			//		Foreground = new SolidColorBrush(Colors.Blue),
			//		FontWeight = FontWeights.Bold
			//	});
			//	span.Inlines.Add(new Run(string.Format("{0}\n", line)));
			//	lineNumber++;
			//}

			return span;
		}

		private void BringCurrentMatchIntoView()
		{
			BringMatchIntoView(_currentMatchIndex);
		}

		private void BringMatchIntoView(int matchIndex)
		{
			DoEvents();
			if ((0 <= matchIndex) && (matchIndex < Matches.Count))
			{
				Matches[matchIndex].Text.BringIntoView();
			}
		}

		private static void DoEvents()
		{
			Application.Current.Dispatcher.Invoke(
				System.Windows.Threading.DispatcherPriority.Background,
				new Action(delegate { }));
		}

		private void ChangeMatchHighlighting(int matchIndex, Color backgroundColor)
		{
			if ((0 <= matchIndex) && (matchIndex < Matches.Count))
			{
				//Matches[matchIndex].Text.ApplyPropertyValue(TextElement.BackgroundProperty, new SolidColorBrush(backgroundColor));
				Matches[matchIndex].Text.Background = new SolidColorBrush(backgroundColor);
			}
		}

		#endregion
	}
}