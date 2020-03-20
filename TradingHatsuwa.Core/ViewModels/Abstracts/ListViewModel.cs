using MvvmCross.Core.ViewModels;
using MvvmCross.Platform.Core;
using System;

namespace TradingHatsuwa.Core.ViewModels
{
    public interface IListViewModel
    {
        MvxObservableCollection<ListItemViewModel> ListItems { get; }
        IMvxCommand SelectCommand { get; }
        event EventHandler SelectoinChanged;
        IMvxCommand SetEditingCommand { get; }
        bool Editing { get; }
        event EventHandler<MvxValueEventArgs<bool>> SetEditing;
    }

    public abstract class ListViewModel<TParameter, TResult> : BaseViewModel<TParameter, TResult>, IListViewModel
        where TParameter : class
        where TResult : class
    {
        public MvxObservableCollection<ListItemViewModel> ListItems { get; } = new MvxObservableCollection<ListItemViewModel>();

        #region "Select"
        public IMvxCommand SelectCommand => new MvxCommand<ListItemViewModel>(item => item.OnAction?.Invoke());

        public event EventHandler SelectoinChanged;

        protected virtual void OnItemSelectionChanged()
        {
            SelectoinChanged?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        #region "Edit"
        /// <summary>
        /// Edit mode 変更 (View -> ViewModel)
        /// </summary>
        public virtual IMvxCommand SetEditingCommand => new MvxCommand<bool>(OnSetEditing);

        public bool Editing { get; protected set; }

        public event EventHandler<MvxValueEventArgs<bool>> SetEditing;

        protected virtual void OnSetEditing(bool editing)
        {
            Editing = editing;

            foreach (var item in ListItems)
            {
                item.Editing = editing;
            }

            // Edit mode 変更 (ViewModel -> View)
            SetEditing?.Invoke(this, new MvxValueEventArgs<bool>(editing));
        }
        #endregion
    }

    public abstract class ListViewModel<TParameter> : BaseViewModel<TParameter>, IListViewModel where TParameter : class
    {
        public MvxObservableCollection<ListItemViewModel> ListItems { get; } = new MvxObservableCollection<ListItemViewModel>();

        #region "Select"
        public IMvxCommand SelectCommand => new MvxCommand<ListItemViewModel>(item => item.OnAction?.Invoke());

        public event EventHandler SelectoinChanged;

        protected virtual void OnItemSelectionChanged()
        {
            SelectoinChanged?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        #region "Edit"
        /// <summary>
        /// Edit mode 変更 (View -> ViewModel)
        /// </summary>
        public virtual IMvxCommand SetEditingCommand => new MvxCommand<bool>(OnSetEditing);

        public bool Editing
        {
            get => _editing;
            protected set => SetProperty(ref _editing, value);
        }
        private bool _editing;

        public event EventHandler<MvxValueEventArgs<bool>> SetEditing;

        protected virtual void OnSetEditing(bool editing)
        {
            Editing = editing;

            foreach (var item in ListItems)
            {
                item.Editing = editing;
            }

            // Edit mode 変更 (ViewModel -> View)
            SetEditing?.Invoke(this, new MvxValueEventArgs<bool>(editing));
        }
        #endregion
    }

    public abstract class ListViewModelResult<TResult> : BaseViewModelResult<TResult>, IListViewModel where TResult : class
    {
        public MvxObservableCollection<ListItemViewModel> ListItems { get; } = new MvxObservableCollection<ListItemViewModel>();

        #region "Select"
        public IMvxCommand SelectCommand => new MvxCommand<ListItemViewModel>(item => item.OnAction?.Invoke());

        public event EventHandler SelectoinChanged;

        protected virtual void OnItemSelectionChanged()
        {
            SelectoinChanged?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        #region "Edit"
        /// <summary>
        /// Edit mode 変更 (View -> ViewModel)
        /// </summary>
        public virtual IMvxCommand SetEditingCommand => new MvxCommand<bool>(OnSetEditing);

        public bool Editing { get; protected set; }

        public event EventHandler<MvxValueEventArgs<bool>> SetEditing;

        protected virtual void OnSetEditing(bool editing)
        {
            Editing = editing;

            foreach (var item in ListItems)
            {
                item.Editing = editing;
            }

            // Edit mode 変更 (ViewModel -> View)
            SetEditing?.Invoke(this, new MvxValueEventArgs<bool>(editing));
        }
        #endregion
    }

    public abstract class ListViewModel : BaseViewModel, IListViewModel
    {
        public MvxObservableCollection<ListItemViewModel> ListItems { get; } = new MvxObservableCollection<ListItemViewModel>();

        #region "Select"
        public IMvxCommand SelectCommand => new MvxCommand<ListItemViewModel>(item => item.OnAction?.Invoke());

        public event EventHandler SelectoinChanged;

        protected virtual void OnItemSelectionChanged()
        {
            SelectoinChanged?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        #region "Edit"
        /// <summary>
        /// Edit mode 変更 (View -> ViewModel)
        /// </summary>
        public virtual IMvxCommand SetEditingCommand => new MvxCommand<bool>(OnSetEditing);

        public bool Editing { get; protected set; }

        public event EventHandler<MvxValueEventArgs<bool>> SetEditing;

        protected virtual void OnSetEditing(bool editing)
        {
            Editing = editing;

            foreach (var item in ListItems)
            {
                item.Editing = editing;
            }

            // Edit mode 変更 (ViewModel -> View)
            SetEditing?.Invoke(this, new MvxValueEventArgs<bool>(editing));
        }
        #endregion
    }
}
