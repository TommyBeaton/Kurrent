using System.Text.RegularExpressions;
using Kurrent.Interfaces.Git;
using Kurrent.Models.Data;
using Kurrent.Utils;
using LibGit2Sharp;

namespace Kurrent.Implementation.Git;

public class FileUpdater : IFileUpdater
{
    private readonly ILogger<FileUpdater> _logger;

    public FileUpdater(ILogger<FileUpdater> logger)
    {
        _logger = logger;
    }

    public async Task UpdateFilesInRepository(Repository repo, RepositoryConfig repoConfig, Image image)
    {
        // Filter files by extension
        var validExtensions = new HashSet<string>(repoConfig.FileExtensions, StringComparer.OrdinalIgnoreCase);
        
        var files=
            Directory.GetFiles(
                    repo.Info.WorkingDirectory, 
                    "*", 
                    SearchOption.AllDirectories
                )
                .Where(
                    file => 
                        !file.Contains(KurrentStrings.GitDirectory) && 
                        validExtensions.Contains(Path.GetExtension(file))
                )
                .ToList();

        _logger.LogInformation("Processing {fileCount} files in repository: {repositoryName}.", files.Count, repoConfig.Name);

        foreach (var file in files)
        {
            var content = await File.ReadAllTextAsync(file);
            if (!content.Contains(KurrentStrings.KurrentTag))
                continue;

            var updatedContent = TryUpdate(content, image);
            if (content != updatedContent)
            {
                await File.WriteAllTextAsync(file, updatedContent);
                Commands.Stage(repo, file);
            }
        }
    }
    
    private string TryUpdate(string content, Image image)
    {
        // If the image.Host is provided, make it mandatory in the regex pattern, otherwise make it optional.
        string hostPattern = !string.IsNullOrEmpty(image.Host) ? image.Host + "\\/" : "(?<= )"; 

        var regex = new Regex($@"{hostPattern}{Regex.Escape(image.Repository)}:[^\s]+ # kurrent update; regex: (.*);");

        string newContent = regex.Replace(content, match =>
        {
            _logger.LogTrace("Regex match: {match}", match.Value);
            
            var pattern = match.Groups[1].Value; // Adjusted group index based on the new regex

            _logger.LogTrace("Pattern: {pattern}", pattern);
            
            if (!Regex.IsMatch(image.Tag, pattern))
            {
                _logger.LogTrace("No match found for pattern: {pattern} and tag {tag}", pattern, image.Tag);
                return match.Value;
            }
            
            _logger.LogTrace("Found match with pattern: {pattern} for tag {tag}", pattern, image.Tag);
            string updated = $"{image} # kurrent update; regex: {pattern};";
            _logger.LogDebug("Updated image with new value: {value}", updated);
            return updated;
        });
        
        _logger.LogTrace("Updated content: {content}", newContent);
        return newContent;
    }
}