using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Modio.Models;

namespace ModManager.Models.Modio;

[DataContract]
public partial class ModioModData : ReactiveObject
{
	[MemberNotNullWhen(true, nameof(IsEnabled))]

	[Reactive]
	[DataMember]
	public partial ModioMod? Data { get; set; }


	[Reactive]
	[DataMember]
	public partial uint Id { get; set; }

	[Reactive]
	[DataMember]
	public partial string? NameId { get; set; }

	[Reactive] public partial bool IsEnabled { get; private set; }

	[ObservableAsProperty] public partial string? Description { get; }
	[ObservableAsProperty] public partial DateTimeOffset LastUpdated { get; }
	[ObservableAsProperty] public partial string? ExternalLink { get; }
	[ObservableAsProperty] public partial string? Author { get; }

	private static readonly string _modPageUrlPattern = "https://mod.io/g/baldursgate3/m/{0}";

	public void Update(ModioMod? data)
	{
		if (data != null) Data = data;
	}

	public ModioModData() : base()
	{
		var whenData = this.WhenAnyValue(x => x.Data).WhereNotNull();
		whenData.Select(x => x.Id).BindTo(this, x => x.Id);
		whenData.Where(x => x.NameId.IsValid()).Select(x => x.NameId).BindTo(this, x => x.NameId);

		_descriptionHelper = whenData.Select(x => x.DescriptionPlaintext).ToUIProperty(this, x => x.Description);
		_lastUpdatedHelper = whenData.Select(x => x.DateUpdated).Select(DateTimeOffset.FromUnixTimeSeconds).ToUIProperty(this, x => x.LastUpdated);
		_externalLinkHelper = whenData.Select(x => x.NameId).Select(x => string.Format(_modPageUrlPattern, x)).ToUIProperty(this, x => x.ExternalLink);
		_authorHelper = whenData.Select(x => x.SubmittedBy?.Username).ToUIProperty(this, x => x.Author);

		this.WhenAnyValue(x => x.Id).Select(x => x != 0).ObserveOn(RxApp.MainThreadScheduler).BindTo(this, x => x.IsEnabled);
	}
}
