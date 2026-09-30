using System.Reflection;
using Education.Domain.Common;
using FluentAssertions;

namespace Education.UnitTests.Architecture;

/// <summary>
/// Guarda o dependency flow entre camadas via inspeção dos assemblies referenciados de cada projeto
/// (mesmo padrão do LeoFoundation). Uma referência proibida quebra o build de testes antes do merge.
///
/// Regra: Domain → nada interno; Application → só Domain; Infrastructure → Application e Domain,
/// nunca Api/CrossCutting.
/// </summary>
public class DependencyRulesTests
{
    private const string ApiAssembly = "Education.API";
    private const string CrossCuttingAssembly = "Education.CrossCutting";
    private const string InfrastructureAssembly = "Education.Infrastructure";
    private const string ApplicationAssembly = "Education.Application";

    private static IEnumerable<string> ReferencedAssemblyNames(Assembly assembly) =>
        assembly.GetReferencedAssemblies().Select(a => a.Name!);

    [Fact]
    public void Domain_ShouldNotReferenceAnyOtherLayer()
    {
        var referenced = ReferencedAssemblyNames(typeof(Error).Assembly);

        referenced.Should().NotContain(new[]
        {
            ApplicationAssembly, InfrastructureAssembly, ApiAssembly, CrossCuttingAssembly,
        }, "Domain é o centro da Clean Architecture e não pode depender de nenhuma outra camada");
    }

    [Fact]
    public void Application_ShouldNotReferenceInfrastructureApiOrCrossCutting()
    {
        var referenced = ReferencedAssemblyNames(typeof(global::Education.Application.DependencyInjection).Assembly);

        referenced.Should().NotContain(new[]
        {
            InfrastructureAssembly, ApiAssembly, CrossCuttingAssembly,
        }, "Application só conhece o Domain — ports/interfaces, nunca as implementações");
    }

    [Fact]
    public void Application_ShouldNotDependOnPersistenceOrHttpFrameworks()
    {
        var referenced = ReferencedAssemblyNames(typeof(global::Education.Application.DependencyInjection).Assembly);

        referenced.Should().NotContain(name =>
                name.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal)
                || name.StartsWith("Npgsql", StringComparison.Ordinal)
                || name.StartsWith("RabbitMQ", StringComparison.Ordinal)
                || name.StartsWith("Microsoft.AspNetCore", StringComparison.Ordinal),
            "regras de negócio não podem depender de detalhes de banco, fila ou HTTP");
    }

    [Fact]
    public void Infrastructure_ShouldNotReferenceApiOrCrossCutting()
    {
        var referenced = ReferencedAssemblyNames(typeof(global::Education.Infrastructure.DependencyInjection).Assembly);

        referenced.Should().NotContain(new[]
        {
            ApiAssembly, CrossCuttingAssembly,
        }, "Infrastructure implementa ports do Domain/Application, mas não conhece a camada de apresentação");
    }
}
