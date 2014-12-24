ree7.RateMyApp
==============

Application rating popup shown on demand after a certain launch count for Windows Store & Windows Phone Store applications.

# Usage

    // Create and setup the Feedback popup helper
	FeedbackHelper rateMyApp = new FeedbackHelper(
		Package.Current.Id.FamilyName, 
		10, 	// launch count for the first popup
		15)     // launch count for the second popup
		{
			ApplicationName = ResourceHelper.GetString("AppName")
		};

	await rateMyApp.Start();

FeedbackHelper will show a popup on the 10th launch here, and if the user does not chose to rate the app immediately, it will show the same popup again on the 15th launch.