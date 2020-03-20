using MvvmCross.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingHatsuwa.Core.ViewModels
{
    public class MeetingAwardListItemViewModel : ListItemViewModel
    {
        public MeetingAwardListItemViewModel()
        {
        }

        public string HeaderText
        {
            get => _text;
            set => SetProperty(ref _text, value);
        }
        private string _text;

        public string HeaderDetailText
        {
            get => _detailText;
            set => SetProperty(ref _detailText, value);
        }
        private string _detailText;

        public string HeaderImage
        {
            get => _image;
            set => SetProperty(ref _image, value);
        }
        private string _image;

    }
}
