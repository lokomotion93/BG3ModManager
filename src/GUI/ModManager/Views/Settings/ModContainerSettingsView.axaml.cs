using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using ModManager.Models.Mod;
using ModManager.ViewModels.Settings;

using SukiUI;

namespace ModManager.Views.Settings;

public partial class ModContainerSettingsView : ReactiveUserControl<ModContainerSettingsViewModel>
{
	private readonly List<CancellationTokenSource> _animationTokens = [];

	public ModContainerSettingsView()
    {
        InitializeComponent();

#if DEBUG
		this.DesignSetup();
#endif

		if (!Design.IsDesignMode)
		{
			ViewModel = AppServices.Get<ModContainerSettingsViewModel>();
		}
		//var vmBinding = ViewModel.WhenAnyValue(x => x.Icon).ToBinding();
		//IconSettings[!DataContextProperty] = vmBinding;
		//IconSettings[!ViewModelProperty] = vmBinding;

		IconExpander.Loaded += (o, e) =>
		{
			IconExpander.Content = new IconSettingsView() { ViewModel = ViewModel.Icon };
		};

		this.WhenActivated(d =>
		{
			if (ViewModel != null)
			{
				d(this.GetObservable(IsVisibleProperty).BindTo(ViewModel, x => x.IsVisible));
				d(this.GetObservable(OpacityProperty).Skip(1).Subscribe(opacity =>
				{
					if(opacity == 0 && !ViewModel.IsVisible)
					{
						IsVisible = false;
					}
				}));

				d(ViewModel.WhenAnyValue(x => x.IsVisible).Skip(1).Subscribe(b =>
				{
					_animationTokens.ForEach(x => x.Cancel());
					_animationTokens.Clear();

					if (b)
					{
						IsVisible = true;
						_animationTokens.Add(this.Animate(OpacityProperty, 0d, 1d, TimeSpan.FromMilliseconds(250)));
						_animationTokens.Add(this.Animate(WidthProperty, 0d, 500d, TimeSpan.FromMilliseconds(500)));
					}
					else
					{
						_animationTokens.Add(this.Animate(OpacityProperty, 1d, 0d, TimeSpan.FromMilliseconds(500)));
						_animationTokens.Add(this.Animate(WidthProperty, Width, 0d, TimeSpan.FromMilliseconds(250)));
					}
				}));
			}
		});
	}
}