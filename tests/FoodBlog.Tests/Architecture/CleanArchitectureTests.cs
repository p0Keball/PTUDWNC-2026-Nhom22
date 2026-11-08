using FoodBlog.Domain.Common;
using FoodBlog.Domain.Entities;
using Xunit;

namespace FoodBlog.Tests.Architecture;

/// <summary>
/// NFR-MAINT-004 / CONS-001: Kiểm tra ràng buộc phân tầng Clean Architecture:
/// 1. Tầng Domain không được phụ thuộc vào Application, Infrastructure hay Presentation (API).
/// 2. Tầng Domain không được phụ thuộc vào bất kỳ thư viện bên thứ ba nào (chỉ dùng System/Microsoft platform).
/// 3. Tầng Application không được phụ thuộc vào Infrastructure hay Presentation.
/// 4. Tầng Infrastructure không được phụ thuộc vào Presentation.
/// 5. Các thực thể chính trong Domain phải kế thừa từ BaseEntity.
/// </summary>
public sealed class CleanArchitectureTests
{
    private readonly System.Reflection.Assembly _domainAssembly =
        typeof(BaseEntity).Assembly;

    private readonly System.Reflection.Assembly _applicationAssembly =
        typeof(FoodBlog.Application.DependencyInjection).Assembly;

    private readonly System.Reflection.Assembly _infrastructureAssembly =
        typeof(FoodBlog.Infrastructure.Persistence.FoodBlogDbContext).Assembly;

    private readonly System.Reflection.Assembly _apiAssembly =
        typeof(Program).Assembly;

    [Fact]
    public void DomainLayer_DoesNotDependOn_OuterLayers()
    {
        var referencedAssemblies = _domainAssembly.GetReferencedAssemblies()
            .Select(a => a.Name)
            .ToList();

        Assert.DoesNotContain("FoodBlog.Application", referencedAssemblies);
        Assert.DoesNotContain("FoodBlog.Infrastructure", referencedAssemblies);
        Assert.DoesNotContain("FoodBlog.API", referencedAssemblies);
    }

    [Fact]
    public void DomainLayer_DoesNotDependOn_ExternalThirdPartyLibraries()
    {
        // CONS-001: Tầng Domain không được phụ thuộc thư viện ngoài ứng dụng,
        // chỉ sử dụng BCL (System.*) và Microsoft Identity Core (cho ApplicationUser : IdentityUser).
        var referencedAssemblies = _domainAssembly.GetReferencedAssemblies()
            .Select(a => a.Name ?? string.Empty)
            .ToList();

        foreach (var assemblyName in referencedAssemblies)
        {
            var isPlatform = assemblyName.StartsWith("System", StringComparison.OrdinalIgnoreCase)
                          || assemblyName.StartsWith("Microsoft", StringComparison.OrdinalIgnoreCase)
                          || assemblyName.StartsWith("netstandard", StringComparison.OrdinalIgnoreCase)
                          || assemblyName.StartsWith("mscorlib", StringComparison.OrdinalIgnoreCase);

            Assert.True(isPlatform,
                $"Domain layer references non-platform assembly: {assemblyName}");
        }
    }

    [Fact]
    public void ApplicationLayer_DoesNotDependOn_InfrastructureOrPresentation()
    {
        var referencedAssemblies = _applicationAssembly.GetReferencedAssemblies()
            .Select(a => a.Name)
            .ToList();

        Assert.DoesNotContain("FoodBlog.Infrastructure", referencedAssemblies);
        Assert.DoesNotContain("FoodBlog.API", referencedAssemblies);
    }

    [Fact]
    public void InfrastructureLayer_DoesNotDependOn_Presentation()
    {
        var referencedAssemblies = _infrastructureAssembly.GetReferencedAssemblies()
            .Select(a => a.Name)
            .ToList();

        Assert.DoesNotContain("FoodBlog.API", referencedAssemblies);
    }

    [Fact]
    public void CoreDomainEntities_InheritFromBaseEntity()
    {
        var entityTypes = new[]
        {
            typeof(Recipe),
            typeof(RecipeStep),
            typeof(RecipeIngredient),
            typeof(RecipeImage),
            typeof(Category)
        };

        foreach (var type in entityTypes)
        {
            Assert.True(
                typeof(BaseEntity).IsAssignableFrom(type),
                $"{type.Name} must inherit from BaseEntity.");
        }
    }
}
