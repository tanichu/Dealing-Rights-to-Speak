using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using HockeyApp.Android;
using HockeyApp.Android.Metrics;
using HockeyApp.Android.Utils;
using System;
using TradingHatsuwa.Core.ViewModels;
using Xamarin.Facebook;
using Xamarin.Facebook.Login;
using Xamarin.Facebook.Login.Widget;

namespace TradingHatsuwa.Droid.Views
{

    [Activity(ScreenOrientation = ScreenOrientation.Portrait, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, LaunchMode = LaunchMode.SingleTop)]
    public class LoginView : BaseView<LoginViewModel>, IFacebookCallback
    {
        private ICallbackManager _callbackManager;
        private MyProfileTracker _profileTracker;
        private LoginButton _loginButton;

        protected override int LayoutResource => Resource.Layout.LoginView;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            MetricsManager.Register(Application);
            HockeyLog.LogLevel = 3;


            SupportActionBar.SetDisplayHomeAsUpEnabled(false);
            Title = ViewModel.Title;

            _profileTracker = new MyProfileTracker();
            _profileTracker.ProfileChanged += MyProfileTracker_ProfileChanged;
            _profileTracker.StartTracking();

            _loginButton = FindViewById<LoginButton>(Resource.Id.fblogin);
            _callbackManager = CallbackManagerFactory.Create();
            _loginButton.RegisterCallback(_callbackManager, this);

            if (Profile.CurrentProfile != null)
            {
                LoginByFacebook();
                return;
            }
        }

        private void LoginByFacebook()
        {
            var profile = Profile.CurrentProfile;
            if (profile == null) return;

            ViewModel.FacebookLoginCommand.Execute(new Data.User
            {
                FacebookProfileId = profile.Id,
                Name = profile.Name,
            });
        }

        protected override void OnResume()
        {
            base.OnResume();
            CrashManager.Register(this, new HockeyCrashManagerSettings());
        }

        private void UnregisterManagers()
        {
            UpdateManager.Unregister();
        }

        protected override void OnPause()
        {
            base.OnPause();
            UnregisterManagers();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            UnregisterManagers();
        }

        private void CheckForUpdates()
        {
            //UpdateManager.Register(this); // TODO: Remove this for store builds!
        }

        #region "IFacebookCallback"
        public void OnCancel()
        {
        }

        public void OnError(FacebookException error)
        {
        }

        private bool _login;

        public void OnSuccess(Java.Lang.Object result)
        {
            _login = true;

            var loginResult = result as LoginResult;
            Profile.FetchProfileForCurrentAccessToken();
        }
        #endregion

        private void MyProfileTracker_ProfileChanged(object sender, OnProfileChangedEventArgs e)
        {
            if (e.Profile != null && _login)
            {
                _login = false;
                LoginByFacebook();
            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Android.Content.Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            _callbackManager.OnActivityResult(requestCode, (int)resultCode, data);
        }

        /// <summary>
        /// Add Toolbar items
        /// </summary>
        /// <param name="menu"></param>
        /// <returns></returns>
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.DebugSettingsMenu, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        /// <summary>
        /// Toolbar menu item selected
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.DebugSettings:
                    ViewModel.DebugSettingsCommand.Execute();
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }
    }

    public class MyProfileTracker : ProfileTracker
    {
        public event EventHandler<OnProfileChangedEventArgs> ProfileChanged;
        protected override void OnCurrentProfileChanged(Profile oldProfile, Profile newProfile)
        {
            ProfileChanged?.Invoke(this, new OnProfileChangedEventArgs(newProfile));
        }
    }

    public class OnProfileChangedEventArgs : EventArgs
    {
        public Profile Profile { get; set; }
        public OnProfileChangedEventArgs(Profile profile)
        {
            Profile = profile;
        }
    }

    public class HockeyCrashManagerSettings : CrashManagerListener
    {
        public override bool ShouldAutoUploadCrashes() => true;
    }
}
