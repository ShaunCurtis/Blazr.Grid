namespace Blazr.Grid.Components;

public partial class TableGrid<TGridItem> : BlazrBaseComponent, IComponent, IHandleEvent
    where TGridItem : class
{
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public IEnumerable<TGridItem> Items { get; set; } = Enumerable.Empty<TGridItem>();

    protected readonly List<IGridColumn<TGridItem>> GridColumns = new();

    public async Task SetParametersAsync(ParameterView parameters)
    {
        parameters.SetParameterProperties(this);

        // Render Now on initialization
        if (this.NotInitialized)
            await this.RenderAsync();

        this.StateHasChanged();
    }

    async Task IHandleEvent.HandleEventAsync(EventCallbackWorkItem item, object? obj)
    {
        await item.InvokeAsync(obj);
        this.StateHasChanged();
    }

    public void RegisterColumn(IGridColumn<TGridItem> column)
    {
        if (!GridColumns.Any(item => item.ComponentUid == column.ComponentUid))
            GridColumns.Add(column);
    }
}
