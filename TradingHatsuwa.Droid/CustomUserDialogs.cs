using Acr.UserDialogs;
using Acr.UserDialogs.Builders;
using Acr.UserDialogs.Fragments;
using Android.App;
using Android.Content;
using Android.Support.V4.App;
using Android.Support.V7.App;
using Android.Text;
using Android.Views;
using Android.Widget;
using System;
using AlertDialog = Android.App.AlertDialog;
using AppCompatAlertDialog = Android.Support.V7.App.AlertDialog;

namespace TradingHatsuwa.Droid
{
    public class CustomUserDialogs : UserDialogsImpl
    {
        public CustomUserDialogs(Func<Activity> getTopActivity) : base(getTopActivity)
        {
        }

        // SelectAll 追加
        public override IDisposable Prompt(PromptConfig config)
        {
            var activity = this.TopActivityFunc();
            if (activity is AppCompatActivity)
                return this.ShowDialog<CustomPromptAppCompatDialogFragment, PromptConfig>((AppCompatActivity)activity, config);

            if (activity is FragmentActivity)
                return this.ShowDialog<CustomPromptDialogFragment, PromptConfig>((FragmentActivity)activity, config);

            return this.Show(activity, () => new CustomPromptBuilder().Build(activity, config));
        }

        // ACR User Dialogs v6.5.1 で Login dialog でバックスペースが効かない場合がある workaround
        // LoginDialogFragment の OnKeyPress の処理がコメント化されているため
        // https://github.com/aritchie/userdialogs/issues/400 対応されたら不要
        public override IDisposable Login(LoginConfig config)
        {
            var activity = TopActivityFunc();
            if (activity is AppCompatActivity compatActivity)
                return ShowDialog<CustomLoginAppCompatDialogFragment, LoginConfig>(compatActivity, config);

            if (activity is FragmentActivity fragmentActivity)
                return ShowDialog<CustomLoginDialogFragment, LoginConfig>(fragmentActivity, config);

            return Show(activity, () => new CustomLoginBuilder().Build(activity, config));
        }
    }

    #region "Login"
    public class CustomLoginDialogFragment : AbstractDialogFragment<LoginConfig>
    {
        protected override void OnKeyPress(object sender, DialogKeyEventArgs args)
        {
            base.OnKeyPress(sender, args);
            if (args.KeyCode != Keycode.Back)
            {
                args.Handled = true;
                return;
            }
            Config?.OnAction(new LoginResult(false, null, null));
            Dismiss();
        }


        protected override Dialog CreateDialog(LoginConfig config)
        {
            return new CustomLoginBuilder().Build(Activity, config);
        }
    }

    public class CustomLoginAppCompatDialogFragment : AbstractAppCompatDialogFragment<LoginConfig>
    {
        protected override void OnKeyPress(object sender, DialogKeyEventArgs args)
        {
            base.OnKeyPress(sender, args);
            if (args.KeyCode != Keycode.Back)
            {
                args.Handled = false;
                return;
            }
            args.Handled = true;
            Config?.OnAction(new LoginResult(false, null, null));
            Dismiss();
        }

        protected override Dialog CreateDialog(LoginConfig config)
        {
            return new CustomLoginBuilder().Build(AppCompatActivity, config);
        }
    }

    // 入力制限追加
    public class CustomLoginBuilder : IAlertDialogBuilder<LoginConfig>
    {
        public Dialog Build(Activity activity, LoginConfig config)
        {
            var txtUser = new EditText(activity)
            {
                Hint = config.LoginPlaceholder,
                InputType = InputTypes.TextVariationVisiblePassword,
                Text = config.LoginValue ?? String.Empty,

            };
            txtUser.SetFilters(new[] { new InputFilterLengthFilter(40) }); //　ユーザーID 40文字
            txtUser.SetSingleLine(true);

            var txtPass = new EditText(activity)
            {
                Hint = config.PasswordPlaceholder ?? "*",
                InputType = InputTypes.TextVariationPassword
            };
            txtPass.SetFilters(new[] { new InputFilterLengthFilter(16) }); //　パスワード 16文字
            txtPass.SetSingleLine(true);

            PromptBuilder.SetInputType(txtPass, InputType.Password);

            var layout = new LinearLayout(activity)
            {
                Orientation = Orientation.Vertical
            };

            txtUser.SetMaxLines(1);
            txtPass.SetMaxLines(1);

            layout.AddView(txtUser, ViewGroup.LayoutParams.MatchParent);
            layout.AddView(txtPass, ViewGroup.LayoutParams.MatchParent);

            return new AlertDialog.Builder(activity, config.AndroidStyleId ?? 0)
                .SetCancelable(false)
                .SetTitle(config.Title)
                .SetMessage(config.Message)
                .SetView(layout)
                .SetPositiveButton(config.OkText, (s, a) =>
                    config.OnAction(new LoginResult(true, txtUser.Text, txtPass.Text))
                )
                .SetNegativeButton(config.CancelText, (s, a) =>
                    config.OnAction(new LoginResult(false, txtUser.Text, txtPass.Text))
                )
                .Create();
        }


        public Dialog Build(AppCompatActivity activity, LoginConfig config)
        {
            var txtUser = new EditText(activity)
            {
                Hint = config.LoginPlaceholder,
                InputType = InputTypes.TextVariationVisiblePassword,
                Text = config.LoginValue ?? String.Empty
            };
            txtUser.SetFilters(new[] { new InputFilterLengthFilter(40) }); //　ユーザーID 40文字
            txtUser.SetSingleLine(true);

            var txtPass = new EditText(activity)
            {
                Hint = config.PasswordPlaceholder ?? "*",
                InputType = InputTypes.TextVariationPassword
            };
            txtPass.SetFilters(new[] { new InputFilterLengthFilter(16) }); //　パスワード 16文字
            PromptBuilder.SetInputType(txtPass, InputType.Password);

            var layout = new LinearLayout(activity)
            {
                Orientation = Orientation.Vertical
            };

            txtUser.SetMaxLines(1);
            txtPass.SetMaxLines(1);

            layout.AddView(txtUser, ViewGroup.LayoutParams.MatchParent);
            layout.AddView(txtPass, ViewGroup.LayoutParams.MatchParent);

            return new AppCompatAlertDialog.Builder(activity, config.AndroidStyleId ?? 0)
                .SetCancelable(false)
                .SetTitle(config.Title)
                .SetMessage(config.Message)
                .SetView(layout)
                .SetPositiveButton(config.OkText, (s, a) =>
                    config.OnAction(new LoginResult(true, txtUser.Text, txtPass.Text))
                )
                .SetNegativeButton(config.CancelText, (s, a) =>
                    config.OnAction(new LoginResult(false, txtUser.Text, txtPass.Text))
                )
                .Create();
        }
    }
    #endregion

    #region "Prompt"
    public class CustomPromptDialogFragment : PromptDialogFragment
    {
        protected override Dialog CreateDialog(PromptConfig config)
        {
            return new CustomPromptBuilder().Build(this.Activity, config);
        }
    }

    public class CustomPromptAppCompatDialogFragment : PromptAppCompatDialogFragment
    {
        protected override Dialog CreateDialog(PromptConfig config)
        {
            return new CustomPromptBuilder().Build(this.AppCompatActivity, config);
        }
    }

    public class CustomPromptBuilder : PromptBuilder
    {
        public new Dialog Build(Activity activity, PromptConfig config)
        {
            var txt = new EditText(activity)
            {
                Id = Int32.MaxValue,
                Hint = config.Placeholder
            };
            if (config.Text != null)
            {
                txt.Text = config.Text;
                txt.FocusChange += (o, e) => { if (e.HasFocus) txt.SelectAll(); }; // modify
            }

            if (config.MaxLength != null)
                txt.SetFilters(new[] { new InputFilterLengthFilter(config.MaxLength.Value) });

            SetInputType(txt, config.InputType);

            var builder = new AlertDialog.Builder(activity, config.AndroidStyleId ?? 0)
                .SetCancelable(false)
                .SetMessage(config.Message)
                .SetTitle(config.Title)
                .SetView(txt)
                .SetPositiveButton(config.OkText, (s, a) =>
                    config.OnAction(new PromptResult(true, txt.Text))
                );

            if (config.IsCancellable)
            {
                builder.SetNegativeButton(config.CancelText, (s, a) =>
                    config.OnAction(new PromptResult(false, txt.Text))
                );
            }
            var dialog = builder.Create();
            this.HookTextChanged(dialog, txt, config);

            return dialog;
        }

        public new Dialog Build(AppCompatActivity activity, PromptConfig config)
        {
            var txt = new EditText(activity)
            {
                Id = Int32.MaxValue,
                Hint = config.Placeholder
            };
            if (config.Text != null)
            {
                txt.Text = config.Text;
                txt.FocusChange += (o, e) => { if (e.HasFocus) txt.SelectAll(); }; // modify
            }
            if (config.MaxLength != null)
                txt.SetFilters(new[] { new InputFilterLengthFilter(config.MaxLength.Value) });

            SetInputType(txt, config.InputType);

            var builder = new AppCompatAlertDialog.Builder(activity, config.AndroidStyleId ?? 0)
                .SetCancelable(false)
                .SetMessage(config.Message)
                .SetTitle(config.Title)
                .SetView(txt)
                .SetPositiveButton(config.OkText, (s, a) =>
                    config.OnAction(new PromptResult(true, txt.Text))
                );

            if (config.IsCancellable)
            {
                builder.SetNegativeButton(config.CancelText, (s, a) =>
                    config.OnAction(new PromptResult(false, txt.Text))
                );
            }
            var dialog = builder.Create();
            this.HookTextChanged(dialog, txt, config);

            return dialog;
        }
    }

    #endregion
}