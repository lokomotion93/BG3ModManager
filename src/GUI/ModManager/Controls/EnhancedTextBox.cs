namespace ModManager.Controls;
public class EnhancedTextBox : TextBox
{
	//protected override Type StyleKeyOverride => typeof(TextBox);

	public static KeyGesture? SelectAllGesture => Application.Current?.PlatformSettings?.HotkeyConfiguration.SelectAll.FirstOrDefault();
	public static KeyGesture ClearGesture { get; } = new KeyGesture(Key.Delete, KeyModifiers.Control);

	public static readonly StyledProperty<bool> CanClearProperty = AvaloniaProperty.Register<EnhancedTextBox, bool>(nameof(CanClear));

	public bool CanClear
	{
		get => GetValue(CanClearProperty);
		set => SetValue(CanClearProperty, value);
	}

	protected override void OnKeyDown(KeyEventArgs e)
	{
		base.OnKeyDown(e);

		if (!IsFocused) return;

		if (e.Key == Key.Escape || ((e.Key == Key.Enter || e.Key == Key.Return) && !AcceptsReturn))
		{
			if (TopLevel.GetTopLevel(this) is Window window)
			{
				window.Focus(NavigationMethod.Tab);
			}
		}
	}

	private void OnContextRequested(ContextRequestedEventArgs e)
	{
		if(!IsFocused)
		{
			Focus();
		}
	}

	public EnhancedTextBox() : base()
	{
		this.GetObservable(TextProperty).Select(Validators.IsValid).BindTo(this, x => x.CanClear);

		var hasContextFlyout = this.GetObservable(ContextFlyoutProperty).WhereNotNull();
		Observable.FromEvent<EventHandler<ContextRequestedEventArgs>, ContextRequestedEventArgs>(
			h => (sender, e) => h(e),
			h => ContextRequested += h,
			h => ContextRequested -= h
		).SkipUntil(hasContextFlyout).Subscribe(OnContextRequested);
	}
}
