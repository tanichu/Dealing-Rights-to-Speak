using MvvmCross.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingHatsuwa.Core.ViewModels
{
    public class ListItemViewModel : MvxNotifyPropertyChanged
    {
        public ListItemViewModel()
        {
        }

        public ListItemViewModel(string text)
        {
            Text = text;
        }

        public object Value { get; set; }

        public T GetValue<T>() => (T)Value;

        /// <summary>
        /// ListView section (ListView group)
        /// </summary>
        public bool IsSection { get; set; }

        public ObservableCollection<ListItemViewModel> SubItems { get; set; } = new ObservableCollection<ListItemViewModel>();

        public string Text
        {
            get => _text;
            set => SetProperty(ref _text, value);
        }
        private string _text;

        public string DetailText
        {
            get => _detailText;
            set => SetProperty(ref _detailText, value);
        }
        private string _detailText;

        public string Image
        {
            get => _image;
            set => SetProperty(ref _image, value);
        }
        private string _image;

        public bool Checked
        {
            get => _checked;
            set { _checked = value; RaisePropertyChanged(() => Checked); } // workaround: 同じ値でも RaisePropertyChanged を呼ぶ。iOS の UITableView Editing On/Off 時にチェックマークが消えるため、Checked 値を設定しなおして描画させる（TargetIpAddressListViewModel）
        }
        private bool _checked;

        public bool Editing
        {
            get => _editing;
            set => SetProperty(ref _editing, value);
        }
        private bool _editing;

        /// <summary>
        /// 選択したときの処理
        /// </summary>
        public Action OnAction { get; set; }

        /// <summary>
        /// 有効かどうか（ダブルタップ対策で一時的に選択を無効にするために使用）
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// チェック可能（チェックマークまたはラジオボタン表示に使用）
        /// </summary>
        public bool Checkable { get; set; }

        /// <summary>
        /// 遷移可能（ナビゲーションアイコンの表示に使用）
        /// </summary>
        public bool Transitable { get; set; }

        /// <summary>
        /// 選択可能（選択した項目の反転表示に使用）
        /// </summary>
        public bool Selectable { get; set; }

        /// <summary>
        /// 削除可能（編集モードで使用）
        /// </summary>
        public bool Deletable { get; set; }

        /// <summary>
        /// 挿入可能（編集モードで使用）
        /// </summary>
        public bool Insertable { get; set; }

        /// <summary>
        /// 並び替え可能（編集モードで使用）
        /// </summary>
        public bool Movable { get; set; }

        /// <summary>
        /// 遷移可能（編集モードで使用）
        /// </summary>
        public bool TransitableInEditing { get; set; }

        /// <summary>
        /// Alpha
        /// </summary>
        public double Alpha
        {
            get => _alpha;
            set => SetProperty(ref _alpha, value);
        }
        private double _alpha = 1.0;

    }
}
