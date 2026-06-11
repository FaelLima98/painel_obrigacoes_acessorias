namespace EAuditoria.Application.DTOs;

public record DashboardResponse(
    int TotalEmpresas,
    int ObrigacoesMes,
    int Pendentes,
    int Entregues,
    int Atrasadas);
