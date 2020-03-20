using Acr.UserDialogs;
using MvvmCross.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TradingHatsuwa.Core.Helpers;
using TradingHatsuwa.Data;
using TradingHatsuwa.HubProxy;

namespace TradingHatsuwa.Core.ViewModels
{
	public class RatingParameter
    {
		/// <summary>評価対象の会議のID</summary>
		public int MeetingId { get; set; }
		/// <summary>表示中の評価項目</summary>
		public EvaluationItem CurrentEvaluationItem { get; set; }
		/// <summary>全評価データ</summary>
		public Dictionary<int, Dictionary<EvaluationItem, int>> UsersRatings { get; set; }
		/// <summary>参加者</summary>
		public List<User> Participants { get; set; }
	}

    public class RatingResult
    {

    }

    public class RatingsViewModel : ListViewModel<RatingParameter, RatingResult>
    {
        private RatingParameter _parameter;
		private readonly IMeetingHubProxy _meetingHub;

		public RatingsViewModel(IMeetingHubProxy meetingHub)
        {
			_meetingHub = meetingHub;
		}

        public override void Prepare(RatingParameter parameter)
        {
            _parameter = parameter;
            Title = $"評価 ({(int)_parameter.CurrentEvaluationItem}/{(int)EvaluationItem.Honest})";
        }

        public override async Task Initialize()
        {
			await CreateListItemsAsync();
        }

		private async Task RegisterRatingsAsync(RatingParameter para)
		{
			try
			{
				var source = new CancellationTokenSource();
				var success = await UserDialogs.Instance.LoadingDelayedAsync(_meetingHub.RegisterRatings(para.MeetingId, para.UsersRatings, Settings.LoginUser.Id), source);
			}
			catch (AggregateException) { return; }
			catch (TaskCanceledException) { return; }
		}

		public IMvxAsyncCommand OkCommand => new MvxAsyncCommand(async () =>
        {

			if (_parameter.CurrentEvaluationItem == EvaluationItem.Honest)
            {
				// end
				AddRatingParameter();
				await RegisterRatingsAsync(_parameter);
				await NavigationService.Close(this, new RatingResult());
            }
            else
            {
				AddRatingParameter();
				var p = new RatingParameter
				{
					MeetingId = _parameter.MeetingId,
					CurrentEvaluationItem = (EvaluationItem)((int)_parameter.CurrentEvaluationItem + 1),
					UsersRatings = _parameter.UsersRatings,
					Participants = _parameter.Participants
				};
                var result = await NavigationService.Navigate<RatingsViewModel, RatingParameter, RatingResult>(p);
				await NavigationService.Close(this, new RatingResult());
            }
        });

		private void AddRatingParameter()
		{
			if (_parameter.UsersRatings == null)
			{
				_parameter.UsersRatings = new Dictionary<int, Dictionary<EvaluationItem, int>>();
			}

			foreach (var item in ListItems)
			{
				foreach (RatingListItemViewModel sub in item.SubItems)
				{
					if (_parameter.UsersRatings.ContainsKey(sub.MemberId))
					{
						_parameter.UsersRatings[sub.MemberId].Add(_parameter.CurrentEvaluationItem, sub.Rating);
					}
					else
					{
						_parameter.UsersRatings.Add(sub.MemberId, new Dictionary<EvaluationItem, int>() { { _parameter.CurrentEvaluationItem, sub.Rating } });
					}
				}
			}
		}


        private async Task CreateListItemsAsync()
        {
            var header = new ListItemViewModel()
            {
                Text = $"{_parameter.CurrentEvaluationItem.ToString()}",
                Image = $"evaluation_{_parameter.CurrentEvaluationItem.ToString().ToLower()}",
                Selectable = false,
                IsSection = true,
            };

			if (_parameter.Participants == null)
			{
				try
				{
					_parameter.Participants = new List<User>();
					var source = new CancellationTokenSource();
					var participants = await UserDialogs.Instance.LoadingDelayedAsync(_meetingHub.GetParticipants(_parameter.MeetingId), source);
					_parameter.Participants.AddRange(participants);
				}
				catch (AggregateException) { return; }
				catch (TaskCanceledException) { return; }
			}

			foreach (var member in _parameter.Participants)
			{
				var item = new RatingListItemViewModel
				{
					MemberId = member.Id,
					Text = member.Name,
					Image = member.IconUrl(),
					Rating = 0,
					Selectable = false,
				};
				header.SubItems.Add(item);
			}
			ListItems.Add(header);
		}


        private class RatingListItemViewModel : ListItemViewModel
        {
			public int MemberId
			{
				get => _memberId;
				set => SetProperty(ref _memberId, value);
			}
			private int _memberId;

			public int Rating
            {
                get => _rating;
                set => SetProperty(ref _rating, value);
            }
            private int _rating = 0;
        }

    }
}