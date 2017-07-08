using System;
using Xamarin.Forms;
using Particle;
using Ambient.Setup;

namespace Ambient
{
    public partial class AmbientPage : ContentPage
    {
        public AmbientPage()
        {
            InitializeComponent();
        }

		async void OnButtonClicked(object sender, EventArgs args)
		{
			// Login as Application Client to create a new User account
			//await ParticleCloud.SharedInstance.LoginWithUserAsync(App.clientid, App.clientsecret);

			// Create the new user account with the Application Client credentials
			//await ParticleCloud.SharedInstance.SignUpWithCustomerAsync("rwarederek@gmail.com", "test", App.productid);

			// Log in with user
			var response = await ParticleCloud.SharedInstance.LoginWithUserAsync(this.username.Text, this.password.Text);

			if (ParticleCloud.AccessToken != null && response)
			{
				//await Navigation.PopAsync();
				await DisplayAlert("Login ok : " + ParticleCloud.AccessToken.Expiration.ToString(), "Login Ok", "Cancel");

				// Get all devices and persist to memory
				//App.userdevices = await ParticleCloud.SharedInstance.GetDevicesAsync();

				// Show device manager page
				App.Current.MainPage = new DeviceManager();
			}
			else
				await DisplayAlert("Login Error", "Invalid Login", "Try Again");

		}
    }
}
