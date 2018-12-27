cmd /c start dotnet run --launch-profile "Shop.Web Development" --no-build --project ./Shop.Web/Shop.Web.csproj

cmd /c start dotnet run --launch-profile "Shop Development" --no-build --project ./Shop.Services.Order/Shop.Services.Order.csproj
cmd /c start dotnet run --launch-profile "Shop Development Second Instance" --no-build --project ./Shop.Services.Order/Shop.Services.Order.csproj

cmd /c start dotnet run --launch-profile "Shop Development" --no-build --project ./Shop.Services.Cart/Shop.Services.Cart.csproj