using CoreLedger.Domain.Exceptions;

namespace CoreLedger.Domain.Cadastros.ValueObjects;

/// <summary>
///     Value Object representando um CNPJ (Cadastro Nacional da Pessoa Jurídica).
/// </summary>
public sealed class CNPJ : IEquatable<CNPJ>
{
    /// <summary>
    ///     CNPJ apenas com dígitos (14 caracteres).
    /// </summary>
    public string Valor { get; }

    private CNPJ(string valor)
    {
        Valor = valor;
    }

    /// <summary>
    ///     Cria uma instância de CNPJ a partir de uma string.
    /// </summary>
    /// <param name="cnpj">CNPJ com ou sem formatação.</param>
    /// <returns>Instância de CNPJ validada.</returns>
    /// <exception cref="DomainValidationException">Quando o CNPJ é inválido.</exception>
    public static CNPJ Criar(string cnpj)
    {
        if (string.IsNullOrWhiteSpace(cnpj))
            throw new DomainValidationException("CNPJ não pode ser vazio.");

        var apenasDigitos = ExtrairDigitos(cnpj);

        if (apenasDigitos.Length != 14)
            throw new DomainValidationException("CNPJ deve conter 14 dígitos.");

        if (TodosDigitosIguais(apenasDigitos))
            throw new DomainValidationException("CNPJ inválido.");

        if (!ValidarDigitosVerificadores(apenasDigitos))
            throw new DomainValidationException("CNPJ inválido - dígitos verificadores incorretos.");

        return new CNPJ(apenasDigitos);
    }

    /// <summary>
    ///     Tenta criar uma instância de CNPJ a partir de uma string.
    /// </summary>
    /// <param name="cnpj">CNPJ com ou sem formatação.</param>
    /// <param name="resultado">Instância de CNPJ se válido, null caso contrário.</param>
    /// <returns>True se o CNPJ é válido, false caso contrário.</returns>
    public static bool TentarCriar(string cnpj, out CNPJ? resultado)
    {
        resultado = null;

        if (string.IsNullOrWhiteSpace(cnpj))
            return false;

        var apenasDigitos = ExtrairDigitos(cnpj);

        if (apenasDigitos.Length != 14)
            return false;

        if (TodosDigitosIguais(apenasDigitos))
            return false;

        if (!ValidarDigitosVerificadores(apenasDigitos))
            return false;

        resultado = new CNPJ(apenasDigitos);
        return true;
    }

    /// <summary>
    ///     Retorna o CNPJ formatado (XX.XXX.XXX/XXXX-XX).
    /// </summary>
    public string Formatado =>
        $"{Valor[..2]}.{Valor[2..5]}.{Valor[5..8]}/{Valor[8..12]}-{Valor[12..]}";

    private static string ExtrairDigitos(string valor)
    {
        return new string(valor.Where(char.IsDigit).ToArray());
    }

    private static bool TodosDigitosIguais(string digitos)
    {
        return digitos.Distinct().Count() == 1;
    }

    private static bool ValidarDigitosVerificadores(string cnpj)
    {
        // Cálculo do primeiro dígito verificador
        int[] multiplicadores1 = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        var soma = 0;

        for (var i = 0; i < 12; i++)
            soma += (cnpj[i] - '0') * multiplicadores1[i];

        var resto = soma % 11;
        var digito1 = resto < 2 ? 0 : 11 - resto;

        if (cnpj[12] - '0' != digito1)
            return false;

        // Cálculo do segundo dígito verificador
        int[] multiplicadores2 = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        soma = 0;

        for (var i = 0; i < 13; i++)
            soma += (cnpj[i] - '0') * multiplicadores2[i];

        resto = soma % 11;
        var digito2 = resto < 2 ? 0 : 11 - resto;

        return cnpj[13] - '0' == digito2;
    }

    public bool Equals(CNPJ? other)
    {
        if (other is null) return false;
        return Valor == other.Valor;
    }

    public override bool Equals(object? obj)
    {
        return obj is CNPJ other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Valor.GetHashCode();
    }

    public override string ToString()
    {
        return Formatado;
    }

    public static bool operator ==(CNPJ? left, CNPJ? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(CNPJ? left, CNPJ? right)
    {
        return !(left == right);
    }

    /// <summary>
    ///     Conversão implícita de CNPJ para string (retorna valor sem formatação).
    /// </summary>
    public static implicit operator string(CNPJ cnpj) => cnpj.Valor;
}
