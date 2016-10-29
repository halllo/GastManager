using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using JustObjectsPrototype.Universal;
using JustObjectsPrototype.Universal.JOP;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Data.Json;

namespace GastManagerLauncher
{
	sealed partial class
		App : Application
	{
		public App()
		{
			this.InitializeComponent();
		}

		protected override void OnLaunched(LaunchActivatedEventArgs e)
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




	[Title("Events"), Icon(Symbol.Bullets)]
	public class Event
	{
		[Editor(@readonly: true)]
		public string ClientName { get; set; }

		[Editor(@readonly: true)]
		public string Message { get; set; }

		[Icon(Symbol.Refresh)]
		public async static Task<List<Event>> Refresh()
		{
			var events = App.Prototype.Repository.OfType<Event>().ToList();
			foreach (var e in events) App.Prototype.Repository.Remove(e);

			var httpClient = new Windows.Web.Http.HttpClient();
			var downloaded = await httpClient.GetStringAsync(new Uri("http://mbusrelay.azurewebsites.net/api/mbus?" + DateTime.Now.Ticks));

			var messages = JsonValue.Parse(downloaded).GetArray()
				.Select(i => new Event
				{
					ClientName = i.GetObject().GetNamedString("UserName"),
					Message = i.GetObject().GetNamedString("Message")
				})
				.ToList();
			
			return messages;
		}
	}
}
