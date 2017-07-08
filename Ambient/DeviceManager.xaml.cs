using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Particle;

using Xamarin.Forms;

namespace Ambient
{
    public partial class DeviceManager : ContentPage
    {
        public DeviceManager()
        {
            InitializeComponent();
			this.DeviceList.IsPullToRefreshEnabled = true;
		}

		protected override async void OnAppearing()
		{
			await this.RefreshDeviceList();
		}

		public void SetupNewDevice(object sender, EventArgs e)
		{
			App.Current.MainPage = new SetupAmbientAppDevice();
		}

		public async Task RefreshDeviceList()
		{
			this.SetViewState(true);

			try
			{
				App.userdevices = await ParticleCloud.SharedInstance.GetDevicesAsync();

				this.DeviceList.ItemsSource = App.userdevices;
			}
			catch
			{
				await DisplayAlert("Refresh error", "Could not get device list - are you connected to the internet?", "OK");
			}

			this.SetViewState(false);
		}

		public void SetViewState(bool bWaiting)
		{
			this.actIndicator.IsVisible = bWaiting;
			this.actIndicator.IsRunning = bWaiting;
		}

		public void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
		{
			if (e.SelectedItem == null) return; // has been set to null, do not 'process' tapped event
			DisplayAlert("Tapped", e.SelectedItem + " row was tapped", "OK");
			((ListView)sender).SelectedItem = null; // de-select the row

		}

		public async void OnRefresh(object sender, EventArgs e)
		{
			var list = (ListView)sender;
			Exception error = null;
			try
			{
				await this.RefreshDeviceList();
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
