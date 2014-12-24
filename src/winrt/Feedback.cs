using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.UI.Popups;

namespace RateMyApp
{
    public sealed class FeedbackHelper
    {
		// Constants
		private const string LaunchCountKey = "RATE_MY_APP_LAUNCH_COUNT";
		private const string ReviewedKey = "RATE_MY_APP_REVIEWED";
		private const string LastLaunchDateKey = "RATE_MY_APP_LAST_LAUNCH_DATE";

		// Members
		private int firstCount;
		private int secondCount;
		private int launchCount = 0;
		private bool reviewed = false;
		private DateTime? lastLaunchDate = null;
		private string packageFamilyName;

		// Properties
		public string ApplicationName { get; set; }
		public string DialogTitle { get; set; }
		public string DialogMessage { get; set; }
		public string DialogOkButtonContent { get; set; }
		public string DialogCancelButtonContent { get; set; }

		public FeedbackHelper(string packageFamilyName, int firstCount, int secondCount)
		{
			this.packageFamilyName = packageFamilyName;
			this.firstCount = firstCount;
			this.secondCount = secondCount;

			LoadStateFromStorage();
			LoadDefaultResources();
		}

		private void LoadStateFromStorage()
		{
			var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
			
			if(localSettings.Values.ContainsKey(LaunchCountKey))
			{
				launchCount = (int)localSettings.Values[LaunchCountKey];
			}

			if(localSettings.Values.ContainsKey(ReviewedKey))
			{
				reviewed = (bool)localSettings.Values[ReviewedKey];
			}

			if(localSettings.Values.ContainsKey(LastLaunchDateKey))
			{
				long ticks = (long)localSettings.Values[LastLaunchDateKey];
				lastLaunchDate = new DateTime(ticks);
			}
		}

		private void LoadDefaultResources()
		{
			ResourceLoader rl = ResourceLoader.GetForCurrentView("RateMyApp/Resources");

			this.DialogTitle = rl.GetString("DialogTitle");
			this.DialogMessage = rl.GetString("DialogMessage");
			this.DialogOkButtonContent = rl.GetString("DialogOK");
			this.DialogCancelButtonContent = rl.GetString("DialogCancel");
		}

		public IAsyncAction Start()
		{
			return StartAsync().AsAsyncAction();
		}

		public static void Reset()
		{
			var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
			localSettings.Values[LaunchCountKey] = 0;
			localSettings.Values[ReviewedKey] = false;
			localSettings.Values.Remove(LastLaunchDateKey);
		}

		private async Task StartAsync()
		{
			// Increment launchCount and saves it with the current date as last launch date
			launchCount++;

			var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
			localSettings.Values[LaunchCountKey] = launchCount;
			localSettings.Values[LastLaunchDateKey] = DateTime.Now.Ticks;

			// Show the rating prompt if we hit a launch count without being reviewed
			if(!reviewed)
			{
				if(launchCount == firstCount || launchCount == secondCount)
				{
					MessageDialog md = new MessageDialog(
						DialogMessage,
						String.Format(DialogTitle, ApplicationName));

					UICommand okCommand = new UICommand(DialogOkButtonContent, OnUserAcceptsToReview, 0);
					UICommand cancelCommand = new UICommand(DialogCancelButtonContent, null, 1);
					md.Commands.Add(okCommand);
					md.Commands.Add(cancelCommand);
					md.DefaultCommandIndex = 0;
					md.CancelCommandIndex = 1;

					await md.ShowAsync();
				}
			}
		}

		private async void OnUserAcceptsToReview(IUICommand command)
		{
			// Set the reviewed flag
			var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
			localSettings.Values[ReviewedKey] = true;


			var storeURI = "ms-windows-store:reviewapp?appid=" + Windows.ApplicationModel.Store.CurrentApp.AppId.ToString();
			//var storeURI = new Uri("ms-windows-store:PDP?PFN=" + packageFamilyName);
			await Windows.System.Launcher.LaunchUriAsync(new Uri(storeURI));
		}
    }
}
