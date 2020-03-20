using Acr.UserDialogs;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Hubs;
using MvvmCross.Platform;
using MvvmCross.Platform.IoC;
using System;
using System.Threading.Tasks;
using TradingHatsuwa.Core.ViewModels;
using TradingHatsuwa.HubProxy;

namespace TradingHatsuwa.Core
{
    public class App : MvvmCross.Core.ViewModels.MvxApplication
    {
        public override void Initialize()
        {
            CreatableTypes()
                .EndingWith("Service")
                .AsInterfaces()
                .RegisterAsLazySingleton();

            //var conn = new HubConnection("http://192.168.0.17:61650/");            
            //conn.Closed += async () =>
            //{
            //    await Task.Delay(5000);
            //    try
            //    {
            //        await conn.Start();
            //    }
            //    catch
            //    {
            //    }
            //};

            //Mvx.RegisterSingleton<IUserHubProxy>(new UserHubProxy(conn.CreateHubProxy("UserHub")));
            //Mvx.RegisterSingleton<IMeetingHubProxy>(new MeetingHubProxy(conn.CreateHubProxy("MeetingHub")));
            //Mvx.RegisterSingleton<IEventHubProxy>(new EventHubProxy(conn.CreateHubProxy("EventHub")));

            //try
            //{
            //    conn.Start();
            //}
            //catch (Exception)
            //{
            //}

            //Mvx.RegisterSingleton<IHubConnection>(() =>
            //{
            //    return conn;
            //});


            // Acr UserDialogs
            Mvx.RegisterSingleton(() => UserDialogs.Instance);
            const string cancel = "キャンセル";
            const string ok = "OK";
            ActionSheetConfig.DefaultCancelText = cancel;
            PromptConfig.DefaultCancelText = cancel;
            PromptConfig.DefaultOkText = ok;
            ConfirmConfig.DefaultOkText = ok;
            ConfirmConfig.DefaultCancelText = cancel;
            ConfirmConfig.DefaultYes = "はい";
            ConfirmConfig.DefaultNo = "いいえ";
            AlertConfig.DefaultOkText = ok;
            ProgressDialogConfig.DefaultCancelText = cancel;
            ProgressDialogConfig.DefaultMaskType = MaskType.Black;
            LoginConfig.DefaultOkText = ok;
            LoginConfig.DefaultCancelText = cancel;

            // Start
            RegisterNavigationServiceAppStart<LoginViewModel>();
        }
    }
}
