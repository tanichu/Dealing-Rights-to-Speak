using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform;
using MvvmCross.Platform.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TradingHatsuwa.Core.Helpers;

namespace TradingHatsuwa.Core.ViewModels
{
    public class RouletteOptions
    {
        public List<Data.EventUser> Users { get; set; }
        public int SelectedUserId { get; set; }
    }

    public class RouletteViewModel : MvxViewModel<RouletteOptions>
    {

        private class RouletteItem
        {
            public DateTime EndTime { get; set; }
            public Data.User User { get; set; }
        }

        private RouletteOptions _options;
        private List<RouletteItem> _items = new List<RouletteItem>();
		private DateTime _closeTime;
        private Timer _timer;

        public string Image
        {
            get => _image;
            set => SetProperty(ref _image, value);
        }
        private string _image;

        public string Text
        {
            get => _text;
            set => SetProperty(ref _text, value);
        }
        private string _text;

        public bool Selected
        {
            get => _selected;
            set => SetProperty(ref _selected, value);
        }
        private bool _selected;


        public override void Prepare(RouletteOptions parameter)
        {
            _options = parameter;
        }

        public override void ViewDisappeared()
        {
            base.ViewDisappeared();
            _timer?.Dispose();
        }

        public override void ViewAppeared()
        {
            base.ViewAppeared();

			// ルーレットの内容を作成
			var count = 30;
			var now = DateTime.UtcNow;
			var endTime = now.AddMilliseconds(1000);
			for (int i = 0; i < count; i++)
			{
				endTime = endTime.AddMilliseconds(100 + (50 * (i / 5)));
				_items.Add(new RouletteItem
				{
					EndTime = endTime,
				});
			}

			var index = _options.Users.FindIndex(u => u.Id == _options.SelectedUserId);
			for (int i = 0; i < count; i++)
			{
				_items[count - i - 1].User = _options.Users[index];
				--index;
				if (index < 0) index = _options.Users.Count - 1;
			}

			// このウインドウを閉じる時刻
			_closeTime = now.AddMilliseconds(10000);

			// タイマー起動（100ms間隔）
			_timer = new Timer(TimerCallback, null, 0, 100);
		}

		/// <summary>
		/// タイマーのコールバックメソッドです。100ms間隔で呼ばれます。
		/// </summary>
		/// <param name="state"></param>
        private void TimerCallback(object state)
        {
			// System.Threding.Timerは別スレッドで動くのでメインスレッドにスイッチする
			Mvx.Resolve<IMvxMainThreadDispatcher>().RequestMainThreadAction(() =>
			{
				var now = DateTime.UtcNow;
				if (_closeTime < now)
				{
					// 終了時刻になったので自分を閉じる
					_timer.Dispose();
					_timer = null;
					Mvx.Resolve<IMvxNavigationService>().Close(this);
				}

				// 終了した要素を削除してから表示更新
				while (_items.Count > 0)
				{
					if (now < _items[0].EndTime)
					{
						break;
					}
					_items.RemoveAt(0);
				}
				if (_items.Count > 0)
				{
					Image = _items[0].User.IconUrl();
					Text = _items[0].User.Name;
				}
				if (_items.Count <= 1)
				{
					// 最後の当選者のところまで来た
					Selected = true;
				}
			});
        }
    }
}
