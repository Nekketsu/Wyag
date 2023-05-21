using System.CommandLine;

namespace Wyag.Cli.Commands;

public class InitCommand : Command
{
    public InitCommand() : base("init", "Initializes a new, empty repository.")
    {
        var pathArgument = new Argument<string>(
            name: "path",
            description: "Where to create the repository",
            getDefaultValue: () => ".");

        AddArgument(pathArgument);

        this.SetHandler(
            path => GitRepository.Create(path),
            pathArgument);
    }
}
