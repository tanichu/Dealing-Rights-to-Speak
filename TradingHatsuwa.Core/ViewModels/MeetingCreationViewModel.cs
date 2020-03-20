using System;
using System.Threading;
using System.Threading.Tasks;
using Acr.UserDialogs;
using MvvmCross.Core.ViewModels;
using TradingHatsuwa.Core.Helpers;
using TradingHatsuwa.Data;
using TradingHatsuwa.HubProxy;

namespace TradingHatsuwa.Core.ViewModels
{
    public class MeetingCreationViewModel : ListViewModelResult<Meeting>
    {
        private Meeting _meeting = new Meeting();
        private readonly IMeetingHubProxy _meetingHub;

        public MeetingCreationViewModel(IMeetingHubProxy meetingHub)
        {
            _meetingHub = meetingHub;
            Title = "会議作成";
        }

        public IMvxAsyncCommand OkCommand => new MvxAsyncCommand(async () =>
        {
            try
            {
                var source = new CancellationTokenSource();
                var result = await UserDialogs.Instance.LoadingDelayedAsync(_meetingHub.Create(_meeting), source);
                if (result == null)
                {
                    UserDialogs.Instance.Alert("会議の作成に失敗しました。");
                    return;
                };
                await NavigationService.Close(this, result);
            }
            catch (AggregateException) { return; }
            catch (TaskCanceledException) { return; }
        });

        public IMvxAsyncCommand CancelCommand => new MvxAsyncCommand(async () => await NavigationService.Close(this, null));

        public override Task Initialize()
        {
            _meeting.Name = $"会議 {DateTime.Now}";
            _meeting.Tickets = 4;
            _meeting.Seconds = 30;
            _meeting.Coupons = 0;
            _meeting.IdleSeconds = 5;
            _meeting.CreatedBy = Settings.LoginUser?.Id ?? -1;

            CreateListItems();
            return base.Initialize();
        }

        private void CreateListItems()
        {
            {
                var item = new ListItemViewModel
                {
                    Text = "会議名",
                    DetailText = _meeting.Name,
                    Selectable = true,
                };
                item.OnAction = async () =>
                {
                    var config = new PromptConfig
                    {
                        Message = item.Text,
                        Text = _meeting.Name,
                        OnTextChanged = r => r.IsValid = !string.IsNullOrWhiteSpace(r.Value) && _meeting.Name != r.Value,
                    };
                    var result = await UserDialogs.Instance.PromptAsync(config);
                    if (!result.Ok) return; // cancel

                    // update
                    _meeting.Name = result.Text;
                    item.DetailText = _meeting.Name;
                };
                ListItems.Add(item);
            }
            {
                var item = new ListItemViewModel
                {
                    Text = "発話券/人",
                    DetailText = $"{_meeting.Tickets} 枚",
                };
                item.OnAction = async () =>
                {
                    var config = new PromptConfig
                    {
                        Message = item.Text,
                        Text = _meeting.Tickets.ToString(),
                        InputType = InputType.Number,
                        OnTextChanged = r => r.IsValid = byte.TryParse(r.Value, out var value) && value > 0 && _meeting.Tickets != value,
                    };
                    var result = await UserDialogs.Instance.PromptAsync(config);
                    if (!result.Ok) return; // cancel

                    // update
                    _meeting.Tickets = int.Parse(result.Text);
                    item.DetailText = $"{_meeting.Tickets} 枚";
                };
                ListItems.Add(item);
            }
            {
                var item = new ListItemViewModel
                {
                    Text = "時間/枚",
                    DetailText = $"{_meeting.Seconds} 秒",
                };
                item.OnAction = async () =>
                {
                    var config = new PromptConfig
                    {
                        Message = item.Text,
                        Text = _meeting.Seconds.ToString(),
                        InputType = InputType.Number,
                        OnTextChanged = r => r.IsValid = ushort.TryParse(r.Value, out var value) && value > 0 && _meeting.Seconds != value,
                    };
                    var result = await UserDialogs.Instance.PromptAsync(config);
                    if (!result.Ok) return; // cancel

                    // update
                    _meeting.Seconds = int.Parse(result.Text);
                    item.DetailText = $"{_meeting.Seconds} 秒";
                };
                ListItems.Add(item);
            }
            {
                var item = new ListItemViewModel
                {
                    Text = "発話振興券/人",
                    DetailText = $"{_meeting.Coupons} 枚",
                };
                item.OnAction = async () =>
                {
                    var config = new PromptConfig
                    {
                        Message = item.Text,
                        Text = _meeting.Coupons.ToString(),
                        InputType = InputType.Number,
                        OnTextChanged = r => r.IsValid = byte.TryParse(r.Value, out var value) && _meeting.Coupons != value,
                    };
                    var result = await UserDialogs.Instance.PromptAsync(config);
                    if (!result.Ok) return; // cancel

                    // update
                    _meeting.Coupons = int.Parse(result.Text);
                    item.DetailText = $"{_meeting.Coupons} 枚";
                };
                ListItems.Add(item);
            }
            {
                var item = new ListItemViewModel
                {
                    Text = "ルーレット開始までの時間",
                    DetailText = $"{_meeting.IdleSeconds} 秒",
                };
                item.OnAction = async () =>
                {
                    var config = new PromptConfig
                    {
                        Message = item.Text,
                        Text = _meeting.IdleSeconds.ToString(),
                        InputType = InputType.Number,
                        OnTextChanged = r => r.IsValid = ushort.TryParse(r.Value, out var value) && value > 0 && _meeting.IdleSeconds != value,
                    };
                    var result = await UserDialogs.Instance.PromptAsync(config);
                    if (!result.Ok) return; // cancel

                    // update
                    _meeting.IdleSeconds = int.Parse(result.Text);
                    item.DetailText = $"{_meeting.IdleSeconds} 秒";
                };
                ListItems.Add(item);
            }
        }
    }
}