using Acr.UserDialogs;
using MvvmCross.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TradingHatsuwa.Core.Helpers;
using TradingHatsuwa.Data;

namespace TradingHatsuwa.Core.ViewModels
{
    public class MenuViewModel : ListViewModel
    {
        public MenuViewModel()
        {
            Title = "メニュー";
            CreateListItems();
        }

        public async Task<bool> ConfirmLogoutAsync()
        {
            var config = new ConfirmConfig
            {
                Message = "サインアウトしますか？",
                OkText = "サインアウト",
            };
            var ok = await UserDialogs.Instance.ConfirmAsync(config);
            if (ok)
                Settings.LoginUser = null;

            return ok;
        }

        private void CreateListItems()
        {
            {
                var user = Settings.LoginUser;
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
            {
                var item = new ListItemViewModel
                {
                    Text = "会議一覧",
                    Selectable = true,
                    Image = "res:ic_forum_grey600_24dp",
                };
                item.OnAction = async () =>
                {
                    await NavigationService.Navigate<MeetingsViewModel>();
                };
                ListItems.Add(item);
            }
            {
                var item = new ListItemViewModel
                {
                    Text = "アカウント一覧",
                    Selectable = true,
                    Image = "res:ic_account_grey600_24dp",
                };
                item.OnAction = async () =>
                {
                    await NavigationService.Navigate<UsersViewModel>();
                };
                ListItems.Add(item);
            }
            {
                var item = new ListItemViewModel
                {
                    Text = "Main（debug)",
                    Selectable = true,
                };
                item.OnAction = async () =>
                {
                    await NavigationService.Navigate<EventViewModel, Meeting>(new Meeting());
                };
                ListItems.Add(item);
            }
            {
                var r = new Random();
                var item = new ListItemViewModel
                {
                    Text = "Dialog（debug)",
                    Selectable = true,
                };
                item.OnAction = async () =>
                {
                    var names = new[]{
                        "本村 翔平",
                        "熊田 栄治",
                        "大河内 和広",
                        "佐田 真",
                        "勝見 裕貴",
                        "蝦名 樹",
                        "田浦 文昭",
                        "樋野 辰哉",
                        "土師 勝広",
                        "相本 義貴",
                        "蘆田 弘毅",
                        "島谷 俊久",
                        "藍川 憲之",
                        "中富 洋三",
                        "勝連 はじめ",
                        "芳本 洋二郎",
                        "溝畑 邦生",
                        "比良 春男",
                        "梅北 直裕",
                        "兼久 恭佑",
                        "柏岡 修弘",
                        "武貞 芳孝",
                        "連 貢一",
                        "楠谷 秀直",
                        "米須 高博",
                        "賀茂 まさと",
                        "野垣 知希",
                        "四辻 金太郎",
                        "入木田 太平",
                        "新実 禎久" };

                    var list = new List<Data.EventUser>();
                    for (int i = 0; i < 10; i++)
                    {
                        var user = new Data.EventUser
                        {
                            GuestId = new Guid($"9546482E-887A-4CAB-A403-AD9C326FFDA{i.ToString("X")}"),
                            Name = names[i],
                        };
                        list.Add(user);
                    }

                    var options = new RouletteOptions
                    {
                        Users = list,
                        SelectedUserId = r.Next(0, list.Count),
                    };

                    await NavigationService.Navigate(typeof(RouletteViewModel), options);
                };
                ListItems.Add(item);
            }
            //{
            //    var item = new ListItemViewModel
            //    {
            //        Text = "個人評価結果（debug)",
            //        Selectable = true,
            //    };
            //    item.OnAction = async () =>
            //    {
            //        await NavigationService.Navigate<UserResultViewModel>();
            //    };
            //    ListItems.Add(item);
            //}
            //{
            //    var item = new ListItemViewModel
            //    {
            //        Text = "会議評価結果（debug)",
            //        Selectable = true,
            //    };
            //    item.OnAction = async () =>
            //    {
            //        await NavigationService.Navigate<MeetingResultTabRootViewModel>();
            //    };
            //    ListItems.Add(item);
            //}
        }

    }
}