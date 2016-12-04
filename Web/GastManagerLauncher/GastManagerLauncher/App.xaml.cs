using System;
using System.Linq;
using System.Collections.Generic;
using JustObjectsPrototype.Universal;
using JustObjectsPrototype.Universal.JOP;
using Windows.ApplicationModel.Activation;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using Windows.UI.Popups;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using Windows.Graphics.Imaging;
using System.IO;
using System.Collections.ObjectModel;

namespace GastManagerLauncher
{
	sealed partial class
		App : Application
	{
		public App()
		{
			this.InitializeComponent();
		}

		protected async override void OnLaunched(LaunchActivatedEventArgs e)
		{
			Prototype = Show.Prototype(
				With.Remembered(new List<object>
				{
					new Gast(1),
					new Gast(2),
					new Gast(3),
					new Gast(4),
					new Gast(5),
					new Gast(6),
				})
				.AndViewOf<Gast>()
				.AndViewOf<Event>()
				.AndViewOf<Bild>()
				.AndOpen<Gast>()
				);
		}

		public static Prototype Prototype;
	}











	[Title("Gäste"), Icon(Symbol.People)]
	public class Gast
	{
		public Gast(int gastIndex)
		{
			Name = "Gast " + gastIndex;
			Url = "https://labs.neokc.de/gast?id=" + Hash(gastIndex + " on " + DateTime.Today.ToString("ddMMyyyy"));
		}

		[Editor(@readonly: true)]
		public string Name { get; set; }

		[Editor(@readonly: true)]
		public string Url { get; set; }

		[Title("Konfigurieren"), Icon(Symbol.ContactInfo)]
		public async void Open()
		{
			await Windows.System.Launcher.LaunchUriAsync(new Uri(Url));
		}

		static string Hash(string input)
		{
			var buffer = CryptographicBuffer.ConvertStringToBinary(input, BinaryStringEncoding.Utf8);
			var hashAlgorithm = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha1);
			var hashBytes = hashAlgorithm.HashData(buffer).ToArray();
			return string.Join("", hashBytes.Select(b => b.ToString("x2")).ToArray()).ToUpper();
		}
	}




	[Title("Events"), Icon(Symbol.Bullets), CustomView("EventListItem")]
	public class Event
	{
		[Editor(@readonly: true)]
		public string ClientName { get; set; }

		[Editor(@readonly: true)]
		public string Message { get; set; }

		[Icon(Symbol.Refresh)]
		public async static void Refresh()
		{
			var httpClient = new Windows.Web.Http.HttpClient();
			httpClient.DefaultRequestHeaders.Add("Accept", "application/xml");
			var downloaded = await httpClient.GetStringAsync(new Uri("https://mbusrelay.azurewebsites.net/api/mbus?" + DateTime.Now.Ticks));

			XNamespace xmlns_mbusrelay = "http://schemas.datacontract.org/2004/07/MBus.Relay";
			var messages = XDocument.Parse(downloaded)
				.Descendants(xmlns_mbusrelay + "RecentMessages.RecentMessage")
				.Select(m => new Event
				{
					ClientName = m.Element(xmlns_mbusrelay + "UserName").Value,
					Message = string.Concat(m.Element(xmlns_mbusrelay + "Message").Value.Take(100))
				}).ToList();

			var events = App.Prototype.Repository.OfType<Event>().ToList();
			events.ForEach(e => App.Prototype.Repository.Remove(e));
			messages.ForEach(m => App.Prototype.Repository.Add(m));
		}
	}




	[Title("Bilder"), Icon(Symbol.RotateCamera), CustomView("BildListItem")]
	public class Bild
	{
		[Editor(hide: true)]
		public string Filename { get; set; }
		[Editor(@readonly: true)]
		public int Size { get; set; }
		[CustomView("ImageDisplay")]
		public BitmapImage Image { get; set; }
		[Editor(hide: true)]
		public string ImageAsBase64 { get; set; }

		[Icon(Symbol.OpenFile), Title("Hinzufügen"), WithProgressBar]
		public async static Task<List<Bild>> Add()
		{
			var neueBilder = new List<Bild>();

			var picker = new FileOpenPicker();
			picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
			picker.FileTypeFilter.Add(".jpg");
			picker.FileTypeFilter.Add(".png");
			var picked = await picker.PickMultipleFilesAsync();
			foreach (var file in picked)
			{
				using (var opened = await file.OpenReadAsync())
				{
					var image = await BitmapDecoder.CreateAsync(opened);
					var pixels = await ImageToScaledPixels(image, newWidth: 1024);
					var base64 = Convert.ToBase64String(pixels);
					var pixelsAgain = Convert.FromBase64String(base64);
					var convertedImage = await ImageFromBytes(pixels);

					neueBilder.Add(new Bild
					{
						Filename = file.Name,
						Size = pixels.Length,
						ImageAsBase64 = base64,
						Image = convertedImage
					});
				}
			}
			return neueBilder;
		}

		private async static Task<byte[]> ImageToScaledPixels(BitmapDecoder decoder, uint newWidth)
		{
			using (var encoderStream = new InMemoryRandomAccessStream())
			{
				var encoder = await BitmapEncoder.CreateForTranscodingAsync(encoderStream, decoder);
				var scaleFactor = decoder.PixelWidth / (double)newWidth;
				uint newHeight = (uint)(decoder.PixelHeight / scaleFactor);
				encoder.BitmapTransform.ScaledWidth = newWidth;
				encoder.BitmapTransform.ScaledHeight = newHeight;

				await encoder.FlushAsync();
				byte[] pixels = new byte[encoderStream.Size];
				await encoderStream.ReadAsync(pixels.AsBuffer(), (uint)pixels.Length, InputStreamOptions.None);

				return pixels;
			}
		}

		private async static Task<BitmapImage> ImageFromBytes(byte[] pixels)
		{
			BitmapImage image = new BitmapImage();
			using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
			{
				await stream.WriteAsync(pixels.AsBuffer());
				stream.Seek(0);
				await image.SetSourceAsync(stream);
			}
			return image;
		}

		[Icon(Symbol.Clear), Title("Leeren"), WithProgressBar]
		public static void Clear(ObservableCollection<Bild> bilder)
		{
			bilder.Clear();
		}

		[Icon(Symbol.SlideShow), Title("Zeigen"), WithProgressBar]
		public static async void Publish(ObservableCollection<Bild> bilder)
		{
			await Show.Message($"Alle {bilder.Count} Bilder nochmal veröffentlichen?", async () =>
			{
				foreach (var bild in bilder)
				{
					await EmitOnMBus($"<bild{bild.Size.ToString().PadLeft(12, '0')}>{bild.ImageAsBase64}");
				}
			});
			await Show.Message("Bestätigen?", () => EmitOnMBus("Bilder zeigen bestätigt"));
		}

		private static async Task EmitOnMBus(string content)
		{
			var sender = "GastManagerLauncher";

			var httpClient = new HttpClient();
			var response = await httpClient.PostAsync(
				new Uri("https://mbusrelay.azurewebsites.net/api/mbus"),
				new StringContent("{sender:'" + sender + "',content:'" + content + "'}", Encoding.UTF8, "application/json"));

			if (!response.IsSuccessStatusCode)
			{
				await new MessageDialog(response.StatusCode.ToString() + "\n" + await response.Content.ReadAsStringAsync()).ShowAsync();
			}
		}
	}
}
