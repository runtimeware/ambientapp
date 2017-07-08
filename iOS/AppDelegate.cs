using System;
using System.Collections.Generic;
using System.Linq;

using System.Net;

using Foundation;
using UIKit;

namespace Ambient.iOS
{
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            global::Xamarin.Forms.Forms.Init();

            LoadApplication(new App());

            return base.FinishedLaunching(app, options);
        }

		public override void WillEnterForeground(UIApplication application)
		{
			// TODO: Figure out why CaptiveNetwork isn't working in iOS 9+
			Console.WriteLine("Determining whether current wifi network is a setup-compatible network");

			string[] interfaces;
			SystemConfiguration.CaptiveNetwork.TryGetSupportedInterfaces(out interfaces);

			if (interfaces != null)
			{
				foreach (var iif in interfaces)
				{
					NSDictionary nsinfo;
					SystemConfiguration.CaptiveNetwork.TryCopyCurrentNetworkInfo(iif, out nsinfo);

					if (nsinfo != null)
						Console.WriteLine(iif + " :" + nsinfo.ValueForKey(new NSString("SSID")).ToString());
				}
			}
		}
    }

}
