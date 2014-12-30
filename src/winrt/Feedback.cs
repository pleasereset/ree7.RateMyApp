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
		private int firstCount;						// Number of app launches before showing the message
		private int secondCount;					// Number of app launches before showing the message a second time
		private int launchCount = 0;
		private bool reviewed = false;
		private DateTime? lastLaunchDate = null;
		private string packageFamilyName;

		// Properties
		/// <summary>
		/// The application name that will be displayed in the popup message.
		/// A default value is provided by the library.
		/// </summary>
		public string ApplicationName { get; set; }

		/// <summary>
		/// Pop-up dialog title.
		/// A default value is provided by the library.
		/// </summary>
		public string DialogTitle { get; set; }
		
		/// <summary>
		/// Pop-up dialog content.
		/// A default value is provided by the library.
		/// </summary>
		public string DialogMessage { get; set; }
		
		/// <summary>
		/// Pop-up dialog OK button content.
		/// A default value is provided by the library.
		/// </summary>
		public string DialogOkButtonContent { get; set; }

		/// <summary>
		/// Pop-up dialog cancel button content.
		/// A default value is provided by the library.
		/// </summary>
		public string DialogCancelButtonContent { get; set; }

		public FeedbackHelper(string packageFamilyName, int firstCount, int secondCount)
		{
			this.packageFamilyName = packageFamilyName;
			this.firstCount = firstCount;
			this.secondCount = secondCount;

			LoadStateFromStorage();
			LoadDefaultResources();
		}

		/// <summary>
		/// Provides access to the stored launch count for any use in the client app.
		/// </summary>
		public int LaunchCount
		{ 
			get { return launchCount; } 
		}

		public IAsyncAction Start()
		{
			return StartAsync().AsAsyncAction();
		}

		/// <summary>
		/// Reset all counters used by the component (launch count, launch date, ...)
		/// </summary>
		public static void Reset()
		{
			var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
			localSettings.Values[LaunchCountKey] = 0;
			localSettings.Values[ReviewedKey] = false;
			localSettings.Values.Remove(LastLaunchDateKey);
		}

		private void LoadStateFromStorage()
		{
			var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

			if (localSettings.Values.ContainsKey(LaunchCountKey))
			{
				launchCount = (int)localSettings.Values[LaunchCountKey];
			}

			if (localSettings.Values.ContainsKey(ReviewedKey))
			{
				reviewed = (bool)localSettings.Values[ReviewedKey];
			}

			if (localSettings.Values.ContainsKey(LastLaunchDateKey))
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
