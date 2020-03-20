using Acr.UserDialogs;
using MvvmCross.Platform;
using MvvmCross.Plugins.Messenger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TradingHatsuwa.Core.Messages;
using TradingHatsuwa.Core.ViewModels;

namespace TradingHatsuwa.Core.Helpers
{
    public static class Extensions
    {
        public static async Task ClosePicker(this IBaseViewModel viewModel, Action afterAction, bool cancel)
        {
            // MEMO: Android workaround

            Mvx.Resolve<IMvxMessenger>().Publish(new CloseMessage(viewModel));
            await Task.Run(async () =>
            {
                if (!cancel)
                {
                    await Task.Delay(1000);
                    afterAction?.Invoke();
                }
            });
        }

        public static string IconUrl(this Data.User user, int width = 128)
        {
            if (!string.IsNullOrEmpty(user.FacebookProfileId))
                return $"http://graph.facebook.com/{user.FacebookProfileId}/picture?type=square&width={width}";
            else
            {
                var id = user.GuestId.ToString("N");
                return $"res:user{id.ToLower()[id.Length - 1]}";
            }
        }

        /// <summary>
        /// milliseconds 秒経っても task が完了しない場合、Loading ダイアログを完了まで表示
        /// </summary>
        public static Task<T> LoadingDelayedAsync<T>(this IUserDialogs instance, Task<T> task, CancellationTokenSource source, string title = "", string cancelText = null, Action onCancel = null, int milliseconds = 500)
        {
            var canCancel = source != null;
            if (!canCancel)
                source = new CancellationTokenSource(); // dummy

            IProgressDialog dialog = null;

            return Task.Run(async () =>
            {
                // delay
                var t = 0.0;
                while (t < milliseconds)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(100));
                    if (task.IsCompleted || task.IsFaulted || (source?.Token.IsCancellationRequested ?? false)) return await task;

                    t += 100;
                }

                // show
                dialog = canCancel ?
                instance.Loading(title, () => { source?.Cancel(true); onCancel?.Invoke(); }, cancelText) :
                instance.Loading(title);

                // wait
                return await task;

            }, source.Token).ContinueWith(t =>
            {
                dialog?.Hide();
                dialog?.Dispose();

                return t.Result;

            }, source.Token).ContinueWith(t =>
            {
                if (t.IsCanceled)
                    return default(T);

                if (t.IsFaulted)
                {
                    // show error
                    UserDialogs.Instance.Alert(t.Exception);
                    return default(T);
                }
                return t.Result;

            }, source.Token);
        }

        public static IDisposable Alert(this IUserDialogs instance, Exception ex)
        {
            var aEx = ex as AggregateException;
            if (aEx != null)
            {
                return instance.Alert(ex.InnerException);
            }
            else if (ex.InnerException == null)
            {
                return instance.Alert(GetMessage(ex.Message));
            }
            else
            {
                return instance.Alert(ex.InnerException);
            }

            string GetMessage(string text) => $"エラーが発生しました\nMessage: {text}";
        }
    }
}
