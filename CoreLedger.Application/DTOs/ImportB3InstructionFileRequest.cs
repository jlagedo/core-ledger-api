namespace CoreLedger.Application.DTOs;

/// <summary>
/// Request DTO for importing B3 instruction file.
/// </summary>
public record ImportB3InstructionFileRequest(
    string ReferenceId,
    string JobDescription
);
