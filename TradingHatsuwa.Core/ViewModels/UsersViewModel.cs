using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acr.UserDialogs;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform;
using TradingHatsuwa.Core.Helpers;
using TradingHatsuwa.HubProxy;

namespace TradingHatsuwa.Core.ViewModels
{
    public class UsersViewModel : ListViewModel
    {
        private readonly IUserHubProxy _userHub;

        public UsersViewModel(IUserHubProxy userHub)
        {
            _userHub = userHub;
            Title = "アカウント一覧";
        }

        public override async Task Initialize()
        {
            await CreateListItemsAsync();
        }

        private async Task CreateListItemsAsync()
        {
            try
            {
                var source = new CancellationTokenSource();
                var users = await UserDialogs.Instance.LoadingDelayedAsync(_userHub.GetUsers(), source);
                if (users == null) return;

                foreach (var user in users)
                {
                    var item = new ListItemViewModel
                    {
                        Text = user.Name,
                        Selectable = true,
                        Image = user.IconUrl(),
                    };
                    item.OnAction = async () =>
                    {
                        await NavigationService.Navigate(typeof(UserResultViewModel), user);
                    };
                    ListItems.Add(item);
                }
            }
            catch (AggregateException) { return; }
            catch (TaskCanceledException) { return; }

            //for (int i = 0; i < 10; i++)
            //{
            //    var item = new ListItemViewModel
            //    {
            //        Text = $"Yamada Taro_{i}",
            //        Selectable = true,
            //        Image = "10215340156958472",
            //    };

            //    item.OnAction = async () =>
            //    {
            //        await NavigationService.Navigate<UserResultViewModel>();
            //    };

            //    ListItems.Add(item);
            //}
        }

    }
}