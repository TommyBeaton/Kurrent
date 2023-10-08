using System.Text.RegularExpressions;
using Lighthouse.Interfaces;
using Lighthouse.Models.Data;

namespace Lighthouse.Implementation;

public class FileUpdater : IFileUpdater
{
    private readonly ILogger<FileUpdater> _logger;

    public FileUpdater(ILogger<FileUpdater> logger)
    {
        _logger = logger;
    }
    
    public string TryUpdate(string content, Container container)
    {
        // If the container.Host is provided, make it mandatory in the regex pattern, otherwise make it optional.
        string hostPattern = !string.IsNullOrEmpty(container.Host) ? container.Host + "\\/" : "(?<= )"; 

        var regex = new Regex($@"{hostPattern}{Regex.Escape(container.Repository)}:[^\s]+ # lighthouse update; regex: (.*);");

        string newContent = regex.Replace(content, match =>
        {
            _logger.LogTrace("Regex match: {match}", match.Value);
            
            var pattern = match.Groups[1].Value; // Adjusted group index based on the new regex

            _logger.LogTrace("Pattern: {pattern}", pattern);
            
            if (!Regex.IsMatch(container.Tag, pattern))
            {
                _logger.LogTrace("No match found for pattern: {pattern} and tag {tag}", pattern, container.Tag);
                return match.Value;
            }
            
            _logger.LogTrace("Found match with pattern: {pattern} for tag {tag}", pattern, container.Tag);
            string updated = $"{container} # lighthouse update; regex: {pattern};";
            _logger.LogDebug("Updated image with new value: {value}", updated);
            return updated;
        });
        
        _logger.LogTrace("Updated content: {content}", newContent);
        return newContent;
    }
}