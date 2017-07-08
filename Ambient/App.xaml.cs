using Xamarin.Forms;
using System.Collections.Generic;
using Ambient.Models;

namespace Ambient
{
    public partial class App : Application
    {
		// TODO: Secure this!
		public static string clientid = "ambient-app-5560";
		public static string clientsecret = "1547588cb4aa59d0fabd3f0d498f62230e46d516";
		public static string productid = "688";

		public static string token = "d3578a522ed4bd4ee8ab4c71cca90075094f4308";
		public static string username = "Derek@runtimeware.com";
		public static string password = "";

		public static WirelessAPConfig wirelessconfig = new WirelessAPConfig();

		public static Particle.ParticleAccessToken accesstoken = null;

		public static List<Particle.ParticleDevice> userdevices;

		public App()
		{
			InitializeComponent();

			MainPage = new AmbientPage();
			//MainPage = new SetupAmbientDevicePage();
		}

		protected override void OnStart()
		{
			// Handle when your app starts
		}

		protected override void OnSleep()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume()
		{
			// Handle when your app resumes
		}
    }
}
