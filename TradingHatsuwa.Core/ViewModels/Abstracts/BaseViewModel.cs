using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform;
using MvvmCross.Plugins.Messenger;

namespace TradingHatsuwa.Core.ViewModels
{
    public interface IBaseViewModel
    {
        string Title { get; set; }
    }

    public abstract class BaseViewModel<TParameter, TResult> : MvxViewModel<TParameter, TResult>, IBaseViewModel
        where TParameter : class
        where TResult : class
    {
        protected IMvxNavigationService NavigationService => Mvx.Resolve<IMvxNavigationService>();

        #region "Implementations"
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }
        private string _title;
        #endregion

        #region "Services"
        #endregion

        #region "Plugins"
        protected IMvxMessenger Messenger => Mvx.Resolve<IMvxMessenger>();
        //protected IMvxFileStore FileService => Mvx.Resolve<IMvxFileStore>();

        #endregion

        public IMvxCommand CloseCommand => new MvxAsyncCommand(async () => await NavigationService.Close(this));
    }

    public abstract class BaseViewModel<TParameter> : MvxViewModel<TParameter>, IBaseViewModel where TParameter : class
    {
        protected IMvxNavigationService NavigationService => Mvx.Resolve<IMvxNavigationService>();

        #region "Implementations"
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }
        private string _title;
        #endregion

        #region "Plugins"
        protected IMvxMessenger Messenger => Mvx.Resolve<IMvxMessenger>();
        //protected IMvxFileStore FileService => Mvx.Resolve<IMvxFileStore>();

        #endregion

        public IMvxCommand CloseCommand => new MvxAsyncCommand(async () => await NavigationService.Close(this));
    }

    public abstract class BaseViewModelResult<TResult> : MvxViewModelResult<TResult>, IBaseViewModel where TResult : class
    {
        protected IMvxNavigationService NavigationService => Mvx.Resolve<IMvxNavigationService>();

        #region "Implementations"
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }
        private string _title;
        #endregion

        #region "Plugins"
        protected IMvxMessenger Messenger => Mvx.Resolve<IMvxMessenger>();
        //protected IMvxFileStore FileService => Mvx.Resolve<IMvxFileStore>();

        #endregion

        public IMvxCommand CloseCommand => new MvxAsyncCommand(async () => await NavigationService.Close(this));
    }

    public abstract class BaseViewModel : MvxViewModel, IBaseViewModel
    {
        protected IMvxNavigationService NavigationService => Mvx.Resolve<IMvxNavigationService>();

        #region "Implementations"
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }
        private string _title;
        #endregion

        #region "Plugins"
        protected IMvxMessenger Messenger => Mvx.Resolve<IMvxMessenger>();
        //protected IMvxFileStore FileService => Mvx.Resolve<IMvxFileStore>();

        #endregion

        public IMvxCommand CloseCommand => new MvxCommand(() => NavigationService.Close(this));
    }
}
