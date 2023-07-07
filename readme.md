# The Blazor Component Registration Pattern

The *Registrstion Pattern* is a recent inovation in Blazor for coding components such as tables and data grids.

In this article I'll demonstrate how to code `FetchData` using the pattern.

The solution uses my *Blazor Base Component Library* which is included in the Repo.

The result is we code the table like this:

```csharp
<TableGrid TGridItem="WeatherForecast" Items="_weatherForecasts">
    <GridColumn TGridItem="WeatherForecast" Title="Date">
        @context.Date
    </GridColumn>
    <GridColumn TGridItem="WeatherForecast" Title="Temp &deg;C">
        @context.TemperatureC
    </GridColumn>
    <GridColumn TGridItem="WeatherForecast" Title="Temp &deg;F">
        @context.TemperatureF
    </GridColumn>
    <GridColumn TGridItem="WeatherForecast" Title="Summary">
        @context.Summary
    </GridColumn>
    <GridColumn TGridItem="WeatherForecast" Title="Actions" Class="text-end">
        <button class="btn btn-primary" @onclick="() => this.Edit(context.Uid)" >Edit</button>
    </GridColumn>
</TableGrid>
```
Each `GridColumn`:

1. Registers itself with `Grid`.
2. Contains the code to render both the header and the row cell for the specific column.

`IGridColumn` defines the interface used by the Grid component to store and use columns.

```csharp
public interface IGridColumn<TGridItem>
    where TGridItem : class
{
    public Guid ComponentUid { get; }
    public RenderFragment ItemHeaderContent { get;}
    public RenderFragment<TGridItem> ItemRowContent { get; }
}
```

`ComponentUid` provides the unique identifier for the component registration in the registration table and the two `RenderFragments` provide delegates that run in the context of the column component instance.

## GridColumn

`GridColumn` is a basic column implementation.

It inherits from `BlazrBaseComponent` which provides the basic boilerplate code for component.

It implements `IComponent` to be recognised as a component

It implements `IGridColumn<TGridItem>` to register with the grid.

```csharp
@inherits BlazrBaseComponent
@implements IGridColumn<TGridItem>
@implements IComponent
@typeparam TGridItem where TGridItem : class
```

It defines four parameters [you will add more to control a fully scoped grid column].

```csharp
    [Parameter, EditorRequired] public string Title { get; set; } = "Field";
    [Parameter] public string? Class { get; set; }

    [Parameter] public RenderFragment<TGridItem>? ChildContent { get; set; }
    [CascadingParameter] private Action<IGridColumn<TGridItem>>? Register { get; set; }
```

`SetParametersAsync` sets the parameters and then calls the cascaded `Register` action delegate to register the component.  No direct rendering takes place: there's no calls to `StateHasChanged`.

```csharp
    public virtual Task SetParametersAsync(ParameterView parameters)
    {
        parameters.SetParameterProperties(this);

        if (Register != null)
            Register(this);

        return Task.CompletedTask;
    }
```

The two interface methods construct the header cell and the data cell.

```csharp
    public virtual RenderFragment ItemHeaderContent => (__builder) =>
    {
        <th class="@this.Class">@this.Title</th>
    };

    public virtual RenderFragment<TGridItem> ItemRowContent => (item) => (__builder) =>
    {
        <td class="@this.Class" @key="@item">
            @if (ChildContent is not null)
            {
                @ChildContent(item)
            }
        </td>
    };
``` 

## TableGrid

`TableGrid` inherits from `BlazrBaseComponent` and implements `IComponent` and `IHandleEvent`.

1. The `Parameters` are self evident.
2. `GridColumns` contains the list of registered columns.

```csharp
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public IEnumerable<TGridItem> Items { get; set; } = Enumerable.Empty<TGridItem>();

    protected readonly List<IGridColumn<TGridItem>> GridColumns = new();
```

`SetParametersAsync` renders the component on the initial run by calling `RenderAsync`.  This ensures the column registration process completes before the component tries to render the columns.

```csharp
    public async Task SetParametersAsync(ParameterView parameters)
    {
        parameters.SetParameterProperties(this);

        // Render Now on initialization
        if (this.NotInitialized)
            await this.RenderAsync();

        this.StateHasChanged();
    }
```

It implements a single render UI event handler.

```csharp
    async Task IHandleEvent.HandleEventAsync(EventCallbackWorkItem item, object? obj)
    {
        await item.InvokeAsync(obj);
        this.StateHasChanged();
    }
```

And defines a `RegisterColumn` method that is cascaded for columns to register through.

```csharp
    public void RegisterColumn(IGridColumn<TGridItem> column)
    {
        if (!GridColumns.Any(item => item.ComponentUid == column.ComponentUid))
            GridColumns.Add(column);
    }
```

The main render code:

```csharp
<CascadingValue Value="this.RegisterColumn">
    @ChildContent
</CascadingValue>

@this.GridRenderFragment
```
Two methods build the header and row content.  They iterate through the `GridColumns` collection and call the appropriate render fragments.

```csharp
    protected void BuildHeaderRow(RenderTreeBuilder __builder)
    {
        <tr>
            @foreach (var col in GridColumns)
            {
                @col.ItemHeaderContent;
            }
        </tr>
    }

    protected void BuildRow(RenderTreeBuilder __builder, int rowIndex, TGridItem item)
    {
        <tr aria-rowindex="@rowIndex" class="">
            @foreach (var col in GridColumns)
            {
                @col.ItemRowContent(item)
                ;
            }
        </tr>
    }
```

Finally the grid builder RenderFragment.  Nothing is rendered on the first render [`Initialized` is `false`].  It builds out the table, the headers and iterates the `Items` collection to build each row.  It passes the item into `BuildRow` which makes the item available as the `context` object in each cell.

```csharp
   protected virtual RenderFragment GridRenderFragment => (__builder) =>
    {
        if (this.Initialized)
        {
            int rowIndex = 0;

            <table class="table">
                <thead class="">
                    @{
                        this.BuildHeaderRow(__builder);
                    }
                </thead>
                <tbody>
                    @foreach (var item in this.Items)
                    {
                        this.BuildRow(__builder, rowIndex, item);
                        rowIndex++;
                    }
                </tbody>
            </table>
        }
    };
```

## Demo Page

```csharp
@page "/"
@inject WeatherForecastService Service

<PageTitle>Index</PageTitle>

<h1>Hello, world!</h1>

<TableGrid TGridItem="WeatherForecast" Items="_weatherForecasts">
    <GridColumn TGridItem="WeatherForecast" Title="Date">
        @context.Date
    </GridColumn>
    <GridColumn TGridItem="WeatherForecast" Title="Temp &deg;C">
        @context.TemperatureC
    </GridColumn>
    <GridColumn TGridItem="WeatherForecast" Title="Temp &deg;F">
        @context.TemperatureF
    </GridColumn>
    <GridColumn TGridItem="WeatherForecast" Title="Summary">
        @context.Summary
    </GridColumn>
    <GridColumn TGridItem="WeatherForecast" Title="Actions" Class="text-end">
        <button class="btn btn-primary" @onclick="() => this.Edit(context.Uid)" >Edit</button>
    </GridColumn>
</TableGrid>

@code {
    private IEnumerable<WeatherForecast> _weatherForecasts = Enumerable.Empty<WeatherForecast>();

    protected override async Task OnInitializedAsync()
    {
        _weatherForecasts = await Service.GetForecastsAsync();
    }

    private void Edit(Guid uid) {}
}
```

```csharp
public class WeatherForecastService
{
    private static readonly string[] Summaries = new[]
    { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" };

    private List<WeatherForecast>? _forecasts;

    public async Task<IEnumerable<WeatherForecast>> GetForecastsAsync()
    {
        if (_forecasts is null)
            _forecasts = GetForecasts();
        await Task.Delay(100);
        return _forecasts;
    }

    private List<WeatherForecast> GetForecasts()
    {
        return Enumerable.Range(1, 10).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now).AddDays(index),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        }).ToList();
    }
}
```