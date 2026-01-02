namespace Aivana_RDP_WPF.Models;

/// <summary>
/// Represents a workflow template for creating workflows
/// </summary>
public class WorkflowTemplate 
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public List<TemplateParameter> Parameters { get; set; } = new();
    public WorkflowDefinition WorkflowDefinition { get; set; } = new();
    public string Icon { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public int UsageCount { get; set; }
    public double Rating { get; set; }
    public string Author { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsBuiltIn { get; set; }
    public string Version { get; set; } = "1.0.0";
}

/// <summary>
/// Parameter definition for workflow templates
/// </summary>
public class TemplateParameter 
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ParameterType Type { get; set; }
    public object? DefaultValue { get; set; }
    public bool IsRequired { get; set; } = true;
    public List<string>? Options { get; set; }
    public string? ValidationPattern { get; set; }
}

/// <summary>
/// Types of template parameters
/// </summary>
public enum ParameterType 
{
    String,
    Number,
    Boolean,
    Selection,
    FilePath,
    FolderPath,
    ConnectionProfile
}
