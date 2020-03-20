using Microsoft.AspNet.SignalR.Client;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace TradingHatsuwa.HubProxy
{
    public static class ProxyExtensions
    {
        public static IDisposable On<T>(this IHubProxy self, Action<T> onData, [CallerMemberName] string eventName = null) => self.On(eventName.ToName(), onData);
        public static IDisposable On<T1, T2>(this IHubProxy self, Action<T1, T2> onData, [CallerMemberName] string eventName = null) => self.On(eventName.ToName(), onData);
        public static Task Invoke(this IHubProxy self, [CallerMemberName] string method = null, params object[] args) => self.Invoke(method, args);
        public static Task<T> Invoke<T>(this IHubProxy self, [CallerMemberName] string method = null, params object[] args) => self.Invoke<T>(method, args);

        private static string ToName(this string eventName) => eventName.Substring("On".Length);
    }
}
