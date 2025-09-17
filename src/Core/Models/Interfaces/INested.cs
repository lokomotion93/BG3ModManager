namespace ModManager.Models.Interfaces;
public interface INested<T, T2> where T : IList<T2>
{
	T Children { get; }
}