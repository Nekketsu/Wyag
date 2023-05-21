using IniParser;
using IniParser.Model;

namespace Wyag;

// A git repository
public class GitRepository
{
    private readonly string worktree;
    private readonly string gitdir;

    private readonly IniData conf;

    private GitRepository(string path, bool force = false)
    {
        conf = null!;

        worktree = path;
        gitdir = Path.Join(path, ".git");

        if (!Directory.Exists(gitdir))
        {
            if (force)
            {
                var directoryInfo = Directory.CreateDirectory(gitdir);
                directoryInfo.Attributes |= FileAttributes.Hidden;
            }
            else
            {
                throw new Exception($"Not a Git repository {path}");
            }
        }

        // Read configuration file in .git/config
        var parser = new FileIniDataParser();
        var cf = RepoFile("config");

        if (cf is not null && File.Exists(cf))
        {
            conf = parser.ReadFile(cf);
        }
        else if (!force)
        {
            throw new Exception("Configuration file missing");
        }

        if (!force)
        {
            var vers = conf["core"]["repositoryformatversion"];
            if (vers != "0")
            {
                throw new Exception($"Unsupported repositoryformatversion {vers}");
            }
        }
    }

    // Compute path under repo's gitdir.
    private string RepoPath(string path)
    {
        return Path.Combine(gitdir, path);
    }

    // Same as repo_path, but create dirname(*path) if absent.  For
    // example, RepoFile("refs\remotes\origin\HEAD") will create
    // .git/refs/remotes/origin.
    private string? RepoFile(string path, bool mkdir = false)
    {
        if (RepoDir(Path.GetDirectoryName(path)!, mkdir) is not null)
        {
            return RepoPath(path);
        }

        return null;
    }

    // Same as repo_path, but mkdir *path if absent if mkdir.
    private string? RepoDir(string path, bool mkdir = false)
    {
        path = RepoPath(path);

        if (Directory.Exists(path))
        {
            return path;
        }
        else if (File.Exists(path))
        {
            throw new Exception($"Not a directory {path}");
        }

        if (mkdir)
        {
            Directory.CreateDirectory(path);
        }

        return null;
    }

    // Create a new repository at path.
    public static GitRepository Create(string path)
    {
        var repo = new GitRepository(path, true);

        // First, we make sure the path either doens't exist or is an
        // empty dir.
        if (Path.Exists(repo.worktree))
        {
            if (File.Exists(repo.worktree))
            {
                throw new Exception($"{path} is not a directory!");
            }
            if (Directory.EnumerateFiles(repo.worktree).Any())
            {
                throw new Exception($"{path} is not empty!");
            }
        }
        else
        {
            Directory.CreateDirectory(repo.worktree);
        }

        repo.RepoDir("branches", true);
        repo.RepoDir("objects", true);
        repo.RepoDir(Path.Combine("refs", "tags"), true);
        repo.RepoDir(Path.Combine("refs", "heads"), true);

        // .git/description
        File.WriteAllText(repo.RepoFile("description")!, "Unnamed repository; edit this file 'description' to name the repository.");
        File.WriteAllText(repo.RepoFile("HEAD")!, "ref: refs/heads/master");
        var parser = new FileIniDataParser();
        parser.WriteFile(repo.RepoFile("config"), DefaultConfig);

        return repo;
    }

    public static GitRepository? Find(string path = ".", bool required = true)
    {
        path = Path.GetFullPath(path);

        if (Directory.Exists(Path.Combine(path, ".git")))
        {
            return new GitRepository(path);
        }

        // If we haven't returned, recurse in parent, if w
        var parent = Path.GetFullPath(Path.Combine(path, ".."));

        if (parent == path)
        {
            // Bottom case
            // Path.GetFullPath(Path.Combine("/", "..")) == "/";
            // If parent == path, then path is root.
            if (required)
            {
                throw new Exception("No git directory.");
            }
            else
            {
                return null;
            }
        }

        // Recursive case
        return Find(parent, required);
    }

    private static IniData DefaultConfig
    {
        get
        {
            var defaultConfig = new IniData();

            var core = new SectionData("core");
            core.Keys.AddKey("repositoryformatversion", "0");
            core.Keys.AddKey("filemode", "false");
            core.Keys.AddKey("bare", "false");

            defaultConfig.Sections.Add(core);

            return defaultConfig;
        }
    }
}
