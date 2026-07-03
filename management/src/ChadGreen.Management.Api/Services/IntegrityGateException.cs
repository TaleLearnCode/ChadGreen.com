using ChadGreen.Management.Shared.Contracts;

namespace ChadGreen.Management.Api.Services;

public sealed class IntegrityGateException(IntegrityValidationResponse result)
    : InvalidOperationException("Save blocked by integrity validation findings.")
{
    public IntegrityValidationResponse Result { get; } = result;
}
