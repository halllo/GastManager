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
					Message = m.Element(xmlns_mbusrelay + "Message").Value
				}).ToList();

			var events = App.Prototype.Repository.OfType<Event>().ToList();
			events.ForEach(e => App.Prototype.Repository.Remove(e));
			messages.ForEach(m => App.Prototype.Repository.Add(m));
		}
	}
}
