# The Blazor Component Registration Pattern

The *Component Registrstion Pattern* is a recently developed pattern for addressing composite components such as tables and data grids.

In this article I'll demonstrate how to code this pattern in a simple setting as I can devise.

The solution uses my *Blazor Base Component Library* which is included in the Repo.

The basic concept is we define a grid like this:

```csharp
<Grid TGridItem="City" Items="CityProvider.Cities">
    <GridColumn TGridItem="City" Title="Country">
        @context.Country
    </GridColumn>
    <GridColumn TGridItem="City" Title="City">
        @context.Name
    </GridColumn>
</Grid>
```

Where `City` is:

```csharp
public record City(string Country, string Name);
```

Each `GridColumn` registers itself with `Grid` and contains the code to render both the header and the data cells for the specific column.

`IGridColumn` defines the interface used by the Grid component to comminicate with each column component.

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