using System.CommandLine;
using Wyag.Cli.Commands;

var rootCommand = new RootCommand("The stupidest content tracker");

rootCommand.AddCommand(new AddCommand());
rootCommand.AddCommand(new CatFileCommand());
rootCommand.AddCommand(new CheckoutCommand());
rootCommand.AddCommand(new CommitCommand());
rootCommand.AddCommand(new HashObjectCommand());
rootCommand.AddCommand(new InitCommand());
rootCommand.AddCommand(new LogCommand());
rootCommand.AddCommand(new LsFilesCommand());
rootCommand.AddCommand(new LsTreeCommand());
rootCommand.AddCommand(new MergeCommand());
rootCommand.AddCommand(new RebaseCommand());
rootCommand.AddCommand(new RevParseCommand());
rootCommand.AddCommand(new RmCommand());
rootCommand.AddCommand(new ShowRefCommand());
rootCommand.AddCommand(new TagCommand());

await rootCommand.InvokeAsync(args);
