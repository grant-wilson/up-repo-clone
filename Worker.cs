using Quartz;
using LibGit2Sharp;

namespace up_repo_clone;

public class Worker : IJob
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration configuration;

    public Worker(ILogger<Worker> logger, IConfiguration configuration)
    {
        _logger = logger;
        this.configuration = configuration;
    }

    public Task Execute(IJobExecutionContext context)
    {
        var cloneDirectory = configuration["CloneDirectory"];

        if (string.IsNullOrEmpty(cloneDirectory))
        {
            _logger.LogError("CloneDirectory is not set in appsettings.json");
            return Task.CompletedTask;
        }

        // create dir if not exists
        if (!Directory.Exists(cloneDirectory))
        {
            Directory.CreateDirectory(cloneDirectory);
        }

        var repoUrl = "https://github.com/grant-wilson/up-repo-clone";
        var branchName = "main";

        // if clone directory is empty then clone the repo
        if (!Directory.EnumerateFileSystemEntries(cloneDirectory).Any())
        {
            _logger.LogInformation("Cloning repo {repoUrl} to {cloneDirectory}", repoUrl, cloneDirectory);
            Repository.Clone(repoUrl, cloneDirectory, new CloneOptions() { BranchName = branchName });
        }
        else
        {
            // check if there are any changes to pull, without pulling
            using (var repo = new Repository(cloneDirectory))
            {
                var remote = repo.Network.Remotes["origin"];
                var refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);
                Commands.Fetch(repo, remote.Name, refSpecs, null, "");
                var mergeResult = Commands.Pull(repo, new Signature("name", "email", DateTimeOffset.Now), new PullOptions());
                if (mergeResult.Status == MergeStatus.UpToDate)
                {
                    _logger.LogInformation("Repo is up to date");
                }
                else
                {
                    _logger.LogInformation("Repo has changes, pulling changes");
                }
            }

        }

        _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
        return Task.CompletedTask;
    }


}
