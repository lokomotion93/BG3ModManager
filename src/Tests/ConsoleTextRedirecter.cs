using System.Text;

using Xunit;

namespace ModManager.Tests;

internal class ConsoleTextRedirecter : System.IO.TextWriter
{
	readonly ITestOutputHelper _output;
	public ConsoleTextRedirecter(ITestOutputHelper output)
	{
		_output = output;
	}
	public override Encoding Encoding
	{
		get { return Encoding.UTF8; }
	}
	public override void WriteLine(string? message)
	{
		if(message != null)
		{
			_output.WriteLine(message);
		}
	}
	public override void WriteLine(string? format, params object?[] args)
	{
		if (format != null)
		{
			_output.WriteLine(format, args);
		}
	}

	public override void Write(char value)
	{
		throw new NotSupportedException("This text writer only supports WriteLine(string) and WriteLine(string, params object[]).");
	}
}
