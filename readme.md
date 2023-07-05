# The Blazor Component Registration Pattern

The *Component Registrstion Pattern* is a recently developed pattern for addressing composite components such as tables and data grids.

In this article I'll demonstrate how to code this pattern in a simple setting as I can devise.

The solution uses my *Blazor Base Component Library* which is included in the Repo.

The basic concept is that we define a grid like this:

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
```
