# syntax=docker/dockerfile:1
# CONS-009: multi-stage build SDK -> ASP.NET runtime.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution + csproj trước để tận dụng layer cache của restore.
COPY FoodBlog.slnx ./
COPY src/FoodBlog.Domain/FoodBlog.Domain.csproj src/FoodBlog.Domain/
COPY src/FoodBlog.Application/FoodBlog.Application.csproj src/FoodBlog.Application/
COPY src/FoodBlog.Infrastructure/FoodBlog.Infrastructure.csproj src/FoodBlog.Infrastructure/
COPY src/FoodBlog.API/FoodBlog.API.csproj src/FoodBlog.API/
RUN dotnet restore src/FoodBlog.API/FoodBlog.API.csproj

COPY src/ ./src/
RUN dotnet publish src/FoodBlog.API/FoodBlog.API.csproj \
    -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true
EXPOSE 8080
COPY --from=build /app/publish ./
ENTRYPOINT ["dotnet", "FoodBlog.API.dll"]
