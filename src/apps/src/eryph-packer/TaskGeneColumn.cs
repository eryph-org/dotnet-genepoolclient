using Spectre.Console;
using Spectre.Console.Rendering;

public sealed class TaskGeneColumn : ProgressColumn
{

    /// <summary>
    /// Gets or sets the alignment of the task description.
    /// </summary>
    public Justify Alignment { get; set; } = Justify.Left;

    /// <inheritdoc/>
    public override IRenderable Render(RenderOptions options, ProgressTask task, TimeSpan deltaTime)
    {
        var text = task.State.Get<GeneUploadTask>("gene").GeneName;
        return new Markup(text ?? string.Empty).Overflow(Overflow.Ellipsis).Justify(Alignment);
    }
}

public sealed class TaskGeneCountColumn : ProgressColumn
{

    /// <summary>
    /// Gets or sets the alignment of the task description.
    /// </summary>
    public Justify Alignment { get; set; } = Justify.Left;

    /// <inheritdoc/>
    public override IRenderable Render(RenderOptions options, ProgressTask task, TimeSpan deltaTime)
    {
        var state = task.State.Get<GeneUploadTask>("gene");
        var text = $"{state.No} of {state.Total}";
        return new Markup(text ?? string.Empty).Overflow(Overflow.Ellipsis).Justify(Alignment);
    }
}