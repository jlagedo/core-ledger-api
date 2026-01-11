using CoreLedger.Domain.Cadastros.ValueObjects;
using CoreLedger.Domain.Exceptions;

namespace CoreLedger.UnitTests.Domain.Cadastros.ValueObjects;

/// <summary>
///     Testes unitários para o Value Object CNPJ.
/// </summary>
public class CNPJTests
{
    #region Testes de Validação - CNPJs Válidos

    [Theory]
    [InlineData("11222333000181")] // CNPJ válido sem formatação
    [InlineData("11.222.333/0001-81")] // CNPJ válido com formatação
    [InlineData("  11.222.333/0001-81  ")] // CNPJ válido com espaços
    public void Criar_ComCNPJValido_DeveCriarInstancia(string cnpj)
    {
        // Act
        var resultado = CNPJ.Criar(cnpj);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal("11222333000181", resultado.Valor);
    }

    [Theory]
    [InlineData("11222333000181", "11.222.333/0001-81")]
    [InlineData("00000000000191", "00.000.000/0001-91")] // CNPJ válido (Banco do Brasil)
    public void Formatado_DeveRetornarCNPJFormatado(string entrada, string esperado)
    {
        // Arrange
        var cnpj = CNPJ.Criar(entrada);

        // Act & Assert
        Assert.Equal(esperado, cnpj.Formatado);
    }

    #endregion

    #region Testes de Validação - CNPJs Inválidos

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Criar_ComCNPJVazio_DeveLancarDomainValidationException(string? cnpj)
    {
        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() => CNPJ.Criar(cnpj!));
        Assert.Equal("CNPJ não pode ser vazio.", exception.Message);
    }

    [Theory]
    [InlineData("1122233300018")] // 13 dígitos
    [InlineData("112223330001811")] // 15 dígitos
    [InlineData("123456")] // muito curto
    public void Criar_ComQuantidadeDigitosIncorreta_DeveLancarDomainValidationException(string cnpj)
    {
        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() => CNPJ.Criar(cnpj));
        Assert.Equal("CNPJ deve conter 14 dígitos.", exception.Message);
    }

    [Theory]
    [InlineData("00000000000000")]
    [InlineData("11111111111111")]
    [InlineData("22222222222222")]
    [InlineData("99999999999999")]
    public void Criar_ComTodosDigitosIguais_DeveLancarDomainValidationException(string cnpj)
    {
        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() => CNPJ.Criar(cnpj));
        Assert.Equal("CNPJ inválido.", exception.Message);
    }

    [Theory]
    [InlineData("11222333000182")] // dígito verificador errado
    [InlineData("11222333000191")] // dígito verificador errado
    [InlineData("12345678000199")] // dígito verificador errado
    public void Criar_ComDigitosVerificadoresIncorretos_DeveLancarDomainValidationException(string cnpj)
    {
        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() => CNPJ.Criar(cnpj));
        Assert.Equal("CNPJ inválido - dígitos verificadores incorretos.", exception.Message);
    }

    #endregion

    #region Testes de TentarCriar

    [Fact]
    public void TentarCriar_ComCNPJValido_DeveRetornarTrue()
    {
        // Act
        var sucesso = CNPJ.TentarCriar("11.222.333/0001-81", out var resultado);

        // Assert
        Assert.True(sucesso);
        Assert.NotNull(resultado);
        Assert.Equal("11222333000181", resultado!.Valor);
    }

    [Fact]
    public void TentarCriar_ComCNPJInvalido_DeveRetornarFalse()
    {
        // Act
        var sucesso = CNPJ.TentarCriar("11222333000182", out var resultado);

        // Assert
        Assert.False(sucesso);
        Assert.Null(resultado);
    }

    [Fact]
    public void TentarCriar_ComCNPJVazio_DeveRetornarFalse()
    {
        // Act
        var sucesso = CNPJ.TentarCriar("", out var resultado);

        // Assert
        Assert.False(sucesso);
        Assert.Null(resultado);
    }

    #endregion

    #region Testes de Igualdade

    [Fact]
    public void Equals_ComMesmoValor_DeveRetornarTrue()
    {
        // Arrange
        var cnpj1 = CNPJ.Criar("11.222.333/0001-81");
        var cnpj2 = CNPJ.Criar("11222333000181");

        // Act & Assert
        Assert.True(cnpj1.Equals(cnpj2));
        Assert.True(cnpj1 == cnpj2);
        Assert.Equal(cnpj1.GetHashCode(), cnpj2.GetHashCode());
    }

    [Fact]
    public void Equals_ComValoresDiferentes_DeveRetornarFalse()
    {
        // Arrange
        var cnpj1 = CNPJ.Criar("11222333000181");
        var cnpj2 = CNPJ.Criar("00000000000191");

        // Act & Assert
        Assert.False(cnpj1.Equals(cnpj2));
        Assert.True(cnpj1 != cnpj2);
    }

    [Fact]
    public void Equals_ComNull_DeveRetornarFalse()
    {
        // Arrange
        var cnpj = CNPJ.Criar("11222333000181");

        // Act & Assert
        Assert.False(cnpj.Equals(null));
        Assert.False(cnpj == null);
        Assert.True(cnpj != null);
    }

    #endregion

    #region Testes de Conversão

    [Fact]
    public void ToString_DeveRetornarFormatado()
    {
        // Arrange
        var cnpj = CNPJ.Criar("11222333000181");

        // Act & Assert
        Assert.Equal("11.222.333/0001-81", cnpj.ToString());
    }

    [Fact]
    public void ConversaoImplicita_DeveRetornarValorSemFormatacao()
    {
        // Arrange
        var cnpj = CNPJ.Criar("11.222.333/0001-81");

        // Act
        string valor = cnpj;

        // Assert
        Assert.Equal("11222333000181", valor);
    }

    #endregion
}
