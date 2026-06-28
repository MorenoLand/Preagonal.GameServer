using System.Collections;

namespace Preagonal.GameServer.Configuration;

internal class CommandLineArguments(IReadOnlyList<string> args) : ICommandLineArguments
{
	public IEnumerator<string> GetEnumerator() => args.GetEnumerator();

	IEnumerator IEnumerable.   GetEnumerator() => GetEnumerator();

	public int                 Count => args.Count;

	public string this[int index] => args[index];
}