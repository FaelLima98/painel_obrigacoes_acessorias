using System.Linq;

namespace EAuditoria.Application.Common;

/// <summary>
/// Utilitários de CNPJ: normalização (apenas dígitos) e validação de dígitos verificadores.
/// </summary>
public static class Cnpj
{
    private static readonly int[] Peso1 = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
    private static readonly int[] Peso2 = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

    /// <summary>Remove tudo que não for dígito.</summary>
    public static string Normalizar(string? cnpj) =>
        new(string.IsNullOrEmpty(cnpj) ? [] : cnpj.Where(char.IsDigit).ToArray());

    /// <summary>Valida um CNPJ (já normalizado ou com máscara).</summary>
    public static bool EhValido(string? cnpj)
    {
        var digits = Normalizar(cnpj);

        if (digits.Length != 14)
            return false;

        // Rejeita sequências repetidas (00000000000000, 11111111111111, ...)
        if (digits.Distinct().Count() == 1)
            return false;

        var dv1 = CalcularDigito(digits, Peso1);
        var dv2 = CalcularDigito(digits, Peso2);

        return digits[12] - '0' == dv1 && digits[13] - '0' == dv2;
    }

    private static int CalcularDigito(string digits, int[] pesos)
    {
        var soma = 0;
        for (var i = 0; i < pesos.Length; i++)
            soma += (digits[i] - '0') * pesos[i];

        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }
}
