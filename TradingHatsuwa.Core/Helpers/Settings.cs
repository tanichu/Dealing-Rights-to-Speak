// Helpers/Settings.cs
using Plugin.Settings;
using Plugin.Settings.Abstractions;
using System;

namespace TradingHatsuwa.Core.Helpers
{
    /// <summary>
    /// This is the Settings static class that can be used in your Core solution or in any
    /// of your client applications. All settings are laid out the same exact way with getters
    /// and setters. 
    /// </summary>
    public static class Settings
    {
        private static ISettings AppSettings => CrossSettings.Current;

        public static Data.User LoginUser { get; set; }

        public static bool IsDebugMode { get; } = true;

        public static string ServerUrl
        {
            //get => AppSettings.GetValueOrDefault(nameof(ServerUrl), "http://tradinghatsuwadev.azurewebsites.net/");
			get => AppSettings.GetValueOrDefault(nameof(ServerUrl), "http://192.168.1.21:61650/");
            set => AppSettings.AddOrUpdateValue(nameof(ServerUrl), value);
        }
        public static string LocalServerUrl
        {
            get => AppSettings.GetValueOrDefault(nameof(LocalServerUrl), "http://192.168.0.17:61650/");
            set => AppSettings.AddOrUpdateValue(nameof(LocalServerUrl), value);
        }

        public static Guid GuestId
        {
            get
            {
                var id = AppSettings.GetValueOrDefault(nameof(GuestId), default(string));
                if (string.IsNullOrEmpty(id))
                {
                    id = Guid.NewGuid().ToString(); // create guest id
                    AppSettings.AddOrUpdateValue(nameof(GuestId), id); // save
                }
                return Guid.Parse(id);
            }
        }

        public static string GuestName
        {
            get => AppSettings.GetValueOrDefault(nameof(GuestName), default(string));
            set => AppSettings.AddOrUpdateValue(nameof(GuestName), value);
        }

    }
}