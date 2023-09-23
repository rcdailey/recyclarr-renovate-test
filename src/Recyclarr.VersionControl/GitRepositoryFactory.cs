using System.IO.Abstractions;
using Serilog;

namespace Recyclarr.VersionControl;

public class GitRepositoryFactory : IGitRepositoryFactory
{
    private readonly ILogger _log;
    private readonly IGitPath _gitPath;

    // A few hand-picked files that should exist in a .git directory.
    private static readonly string[] ValidGitPaths =
    {
        ".git/config",
        ".git/index",
        ".git/HEAD"
    };

    public GitRepositoryFactory(ILogger log, IGitPath gitPath)
    {
        _log = log;
        _gitPath = gitPath;
    }

    public async Task<IGitRepository> CreateAndCloneIfNeeded(
        Uri repoUrl,
        IDirectoryInfo repoPath,
        string branch,
        CancellationToken token)
    {
        var repo = new GitRepository(_log, _gitPath, repoPath);

        if (!repoPath.Exists)
        {
            _log.Information("Cloning...");
            await repo.Clone(token, repoUrl, branch, 1);
        }
        else
        {
            // First check if the `.git` directory is present and intact. We used to just do a `git status` here, but
            // this sometimes has a false positive if our repo directory is inside another repository.
            if (ValidGitPaths.Select(repoPath.File).Any(x => !x.Exists))
            {
                throw new InvalidGitRepoException("A `.git` directory or its files are missing");
            }

            // Run just to check repository health. If unhealthy, an exception will
            // be thrown. That exception will propagate up and result in a re-clone.
            await repo.Status(token);
        }

        await repo.SetRemote(token, "origin", repoUrl);
        _log.Debug("Remote 'origin' set to {Url}", repoUrl);

        return repo;
    }
}