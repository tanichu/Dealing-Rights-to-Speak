using System;
using System.Threading;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Hubs;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform;
using TradingHatsuwa.Core.Helpers;
using TradingHatsuwa.HubProxy;

namespace TradingHatsuwa.Core.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private Data.User _autoLoginUser;

        public LoginViewModel()
        {
            Title = "TradingHatsuwa";
        }

        public override async Task Initialize()
        {
            await ConnectAsync();
        }

        private bool _viewAppeared;

        public override void ViewAppeared()
        {
            _viewAppeared = true;
            if (_hubConnection?.State != ConnectionState.Connected)
            {
                _dialog?.Dispose();
                _dialog = UserDialogs.Instance.Loading("接続中");
            }
        }

        private HubConnection _hubConnection;
        private async Task ConnectAsync()
        {
            if (_hubConnection != null)
            {
                _hubConnection.Closed -= _hubConnection_Closed;
                _hubConnection.StateChanged -= _hubConnection_StateChanged;
                _hubConnection.Dispose();
            }

            try
            {
                _hubConnection = new HubConnection(Settings.ServerUrl);

                Mvx.RegisterSingleton<IUserHubProxy>(new UserHubProxy(_hubConnection.CreateHubProxy("UserHub")));
                Mvx.RegisterSingleton<IMeetingHubProxy>(new MeetingHubProxy(_hubConnection.CreateHubProxy("MeetingHub")));
                Mvx.RegisterSingleton<IEventHubProxy>(new EventHubProxy(_hubConnection.CreateHubProxy("EventHub")));

                _hubConnection.Closed += _hubConnection_Closed;
                _hubConnection.StateChanged += _hubConnection_StateChanged;
                await _hubConnection.Start();

            }
            catch (Exception ex)
            {
                UserDialogs.Instance.Alert(ex.Message);
            }
        }

        IProgressDialog _dialog;

        private async void _hubConnection_StateChanged(StateChange obj)
        {
            if (obj.NewState == ConnectionState.Connected)
            {
                _dialog?.Dispose();
                _dialog = null;

                if (_autoLoginUser != null)
                {
                    await CreateOrUpdateAsync(_autoLoginUser);
                    _autoLoginUser = null;

                    if (Settings.LoginUser != null)
                        await NavigationService.Navigate<MenuViewModel>();
                }
            }
            else
            {
                if (_viewAppeared)
                {
                    _dialog?.Dispose();
                    _dialog = UserDialogs.Instance.Loading("接続中");
                }
            }
        }

        private void _hubConnection_Closed()
        {
            Task.Run(async () =>
            {
                await Task.Delay(5000);
                try
                {
                    await _hubConnection?.Start();
                }
                catch
                {
                }
            });
        }

        public IMvxAsyncCommand DebugSettingsCommand => new MvxAsyncCommand(async () =>
        {
            var config = new ActionSheetConfig();
            config.Title = "接続先";
            config.Add("アドレス入力", async () =>
            {
                var result = await UserDialogs.Instance.PromptAsync(new PromptConfig
                {
                    Text = Settings.LocalServerUrl,
                });
                if (!result.Ok) return; // cancel

                Settings.LocalServerUrl = result.Text;
                Settings.ServerUrl = result.Text;
                await ConnectAsync();
            });
            config.Add("Azure", async () =>
            {
                Settings.ServerUrl = "http://tradinghatsuwadev.azurewebsites.net/";
                await ConnectAsync();
            });

            config.SetCancel();
            UserDialogs.Instance.ActionSheet(config);
        });

        public IMvxAsyncCommand<Data.User> FacebookLoginCommand => new MvxAsyncCommand<Data.User>(async user =>
        {
            if (_hubConnection.State != ConnectionState.Connected)
            {
                _autoLoginUser = user;
                return;
            }

            await CreateOrUpdateAsync(user);
            if (Settings.LoginUser != null)
                await NavigationService.Navigate<MenuViewModel>();
        });

        public IMvxAsyncCommand GuestLoginCommand => new MvxAsyncCommand(async () =>
        {
            //// debug
            //await NavigationService.Navigate<MenuViewModel>();
            //return;

            var config = new PromptConfig
            {
                Message = "名前",
                OkText = "ログイン",
                Text = Settings.GuestName,
                MaxLength = 50,
                OnTextChanged = r => r.IsValid = !string.IsNullOrWhiteSpace(r.Value),
            };
            var result = await UserDialogs.Instance.PromptAsync(config);
            if (!result.Ok) return; // cancel

            // save
            Settings.GuestName = result.Text;

            // create or update
            var success = await CreateOrUpdateAsync(new Data.User
            {
                GuestId = Settings.GuestId,
                Name = result.Text,
            });

            if (success)
                await NavigationService.Navigate<MenuViewModel>();
        });

        private async Task<bool> CreateOrUpdateAsync(Data.User user)
        {
            try
            {
                var source = new CancellationTokenSource();
                var result = await UserDialogs.Instance.LoadingDelayedAsync(Mvx.Resolve<IUserHubProxy>().CreateOrUpdate(user), source);
                Settings.LoginUser = result;

                return result != null;
            }
            catch (AggregateException) { return false; }
            catch (TaskCanceledException) { return false; }
        }

    }
}