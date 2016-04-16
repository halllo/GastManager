using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media.Capture;
using Windows.Media.SpeechRecognition;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace ListenApp
{
	public sealed partial class MainPage : Page
	{
		public MainPage()
		{
			this.InitializeComponent();
			Einträge = new ObservableCollection<Eintrag>();
			EinträgeListBox.SelectionChanged += (s, e) =>
			{
				if (EinträgeListBox.SelectedItem != null)
				{
					EinträgeListBox.SelectedItem = null;
				}
			};

			this.DataContext = this;
		}

		public ObservableCollection<Eintrag> Einträge { get; private set; }

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);

			Einträge.Add(new Eintrag { Text = "Hello.", Subtle = true });
			Einträge.Add(new Eintrag { Text = "Press 'listen' and keep talking until 'bye'.", Subtle = true });

			Init(SpeechRecognizer.SystemSpeechLanguage);
		}

		SpeechRecognizer _SpeechRecognizer;
		private async void Init(Windows.Globalization.Language language)
		{
			ListenButton.IsEnabled = false;
			bool permissionGained = await AudioCapturePermissions.RequestMicrophonePermission();
			if (!permissionGained)
			{
				MessageDialog("Permission to access capture resources was not given by the user, reset the application setting in Settings->Privacy->Microphone.");
			}

			var recognizer = new SpeechRecognizer(language);
			var topicConstraint = new SpeechRecognitionTopicConstraint(SpeechRecognitionScenario.Dictation, "Development");
			recognizer.Constraints.Add(topicConstraint);
			var compilationResult = await recognizer.CompileConstraintsAsync();

			_SpeechRecognizer = recognizer;
			ListenButton.IsEnabled = true;
		}

		private async void MessageDialog(string message)
		{
			var messageDialog = new Windows.UI.Popups.MessageDialog(message);
			await messageDialog.ShowAsync();
		}

		private async void Listen_Click(object sender, RoutedEventArgs e)
		{
			GermanButton.IsEnabled = ListenButton.IsEnabled = false;

			await SpeekLoop();

			GermanButton.IsEnabled = ListenButton.IsEnabled = true;
		}

		private async Task SpeekLoop()
		{
			string heard = null;
			do
			{

				var result = await _SpeechRecognizer.RecognizeAsync();
				heard = result.Text;

				Einträge.Add(new Eintrag { Text = heard });
				ScrollToBottom(EinträgeListBox);

			} while (!(heard.ToLower().Contains("the end") || heard.ToLower().Contains("bye")));

			Einträge.Add(new Eintrag { Text = "Press 'listen' and keep talking until 'bye'.", Subtle = true });
			ScrollToBottom(EinträgeListBox);
		}

		private void GermanChanged(object sender, RoutedEventArgs e)
		{
			Init(GermanButton.IsChecked == true ? SpeechRecognizer.SupportedGrammarLanguages.First(l => l.LanguageTag == "de-DE")
												: SpeechRecognizer.SystemSpeechLanguage);
		}

		private static void ScrollToBottom(ListBox listBox)
		{
			if (VisualTreeHelper.GetChildrenCount(listBox) > 0)
			{
				Border border = (Border)VisualTreeHelper.GetChild(listBox, 0);
				ScrollViewer scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(border, 0);
				scrollViewer.UpdateLayout();
				scrollViewer.ScrollToVerticalOffset(scrollViewer.ScrollableHeight);
			}
		}
	}

	public class Eintrag
	{
		public string Text { get; set; }
		public bool Subtle { get; set; }
		public SolidColorBrush Color
		{
			get { return Subtle ? new SolidColorBrush(Colors.Gray) : new SolidColorBrush(Colors.Black); }
		}
		public double Size
		{
			get { return Subtle ? 15 : 20; }
		}
	}


	public class AudioCapturePermissions
	{
		private static int NoCaptureDevicesHResult = -1072845856;

		public async static Task<bool> RequestMicrophonePermission()
		{
			try
			{
				MediaCaptureInitializationSettings settings = new MediaCaptureInitializationSettings();
				settings.StreamingCaptureMode = StreamingCaptureMode.Audio;
				settings.MediaCategory = MediaCategory.Speech;
				MediaCapture capture = new MediaCapture();

				await capture.InitializeAsync(settings);
			}
			catch (UnauthorizedAccessException)
			{
				return false;
			}
			catch (Exception exception)
			{
				if (exception.HResult == NoCaptureDevicesHResult)
				{
					var messageDialog = new Windows.UI.Popups.MessageDialog("No Audio Capture devices are present on this system.");
					await messageDialog.ShowAsync();
					return false;
				}
				else
				{
					throw;
				}
			}
			return true;
		}
	}
}
