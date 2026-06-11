namespace EAuditoria.Application.Common;

/// <summary>Erro de validação de entrada (mapeado para HTTP 400 com campo errors).</summary>
public class ValidationException(IReadOnlyDictionary<string, string[]> errors)
    : Exception("Um ou mais erros de validação ocorreram.")
{
    public IReadOnlyDictionary<string, string[]> Errors { get; } = errors;

    public ValidationException(string field, string message)
        : this(new Dictionary<string, string[]> { [field] = [message] }) { }
}

/// <summary>Recurso não encontrado (mapeado para HTTP 404).</summary>
public class NotFoundException(string message) : Exception(message);

/// <summary>Conflito de estado, ex.: CNPJ ou entrega duplicada (mapeado para HTTP 409).</summary>
public class ConflictException(string message) : Exception(message);
