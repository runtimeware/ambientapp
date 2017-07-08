using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using System.Threading.Tasks;

using System.Linq;

using Xamarin.Forms;

using Ambient.Setup;

namespace Ambient
{
    public partial class SetupAmbientAppDevice : ContentPage
    {
		private List<Models.SoftAPScanAP> ssidlist;

		public string password = "";

		public SetupAmbientAppDevice()
		{
			InitializeComponent();

			this.WirelessAPList.SetBinding(ListView.ItemsSourceProperty, new Binding("."));

			this.WirelessAPList.IsPullToRefreshEnabled = true;
		}

		protected override async void OnAppearing()
		{
			await this.RefreshWirelessAPList();

			if (this.ssidlist != null)
				this.WirelessAPList.ItemsSource = this.ssidlist.Select(x => x.SSID);
		}

		public async Task RefreshWirelessAPList()
		{
			this.SetViewState(true);

			try
			{
				this.ssidlist = await SoftAPConfig.GetScanAPsAsync();

				this.WirelessAPList.ItemsSource = this.ssidlist.Select(x => x.SSID);
			}
			catch
			{
				await DisplayAlert("Refresh error", "Could not get wireless networks - are you connected to the Ambient device?", "OK");
			}

			this.SetViewState(false);
		}

		public async void SetupWirelessAP(object sender, EventArgs e)
		{
			this.SetViewState(true);

			App.wirelessconfig.Password = this.PasswordEntry.Text;

			var publicKey = await SoftAPConfig.GetPublicKeyAsync();

			var configureresult = await SoftAPConfig.SetConfigureAPAsync(0, ssidlist.Where(l => l.SSID == App.wirelessconfig.SSID).First(), App.wirelessconfig.Password, publicKey);

			if (configureresult == 0)
				await SoftAPConfig.SetConnectAPAsync(0);

			this.SetViewState(false);
		}

		public void SetViewState(bool bWaiting)
		{
			this.actIndicator.IsVisible = bWaiting;
			this.actIndicator.IsRunning = bWaiting;

			this.WirelessAPList.IsEnabled = !bWaiting;
			this.PasswordEntry.IsEnabled = !bWaiting;
			this.SetupAP.IsEnabled = !bWaiting;
		}

		public void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
		{
			if (e.SelectedItem == null) return; // has been set to null, do not 'process' tapped event
			DisplayAlert("Tapped", e.SelectedItem + " row was tapped", "OK");
			((ListView)sender).SelectedItem = null; // de-select the row

			App.wirelessconfig.SSID = e.SelectedItem.ToString();
		}

		public async void OnRefresh(object sender, EventArgs e)
		{
			var list = (ListView)sender;
			Exception error = null;
			try
			{
				await RefreshWirelessAPList();
			}
			catch (Exception ex)
			{
				error = ex;
			}
			finally
			{
				list.EndRefresh();
			}

			if (error != null)
			{
				await DisplayAlert("Refresh Error", "Couldn't refresh data (" + error.Message + ")", "OK");
			}
		}


	}
}
