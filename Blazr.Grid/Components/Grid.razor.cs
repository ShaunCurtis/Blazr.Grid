namespace Blazr.Grid.Components;

public partial class Grid<TGridItem> : BlazrBaseComponent, IComponent, IHandleEvent
    where TGridItem : class
{
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public IEnumerable<TGridItem> Items { get; set; } = Enumerable.Empty<TGridItem>();

    protected readonly List<IGridItem<TGridItem>> GridColumns = new();

    public void RegisterColumn(IGridItem<TGridItem> column)
    {
        if (!GridColumns.Any(item => item.ComponentUid == column.ComponentUid))
            GridColumns.Add(column);
    }

    public async Task SetParametersAsync(ParameterView parameters)
    {
        parameters.SetParameterProperties(this);

        // Render Now on initialization
        if (this.NotInitialized)
            await this.RenderAsync();

        await this.OnParametersSetAsync();
        this.StateHasChanged();
    }

    protected virtual Task OnParametersSetAsync()
        => Task.CompletedTask;

    async Task IHandleEvent.HandleEventAsync(EventCallbackWorkItem item, object? obj)
    {
        await item.InvokeAsync(obj);
        this.StateHasChanged();
    }

}
