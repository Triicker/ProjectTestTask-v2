using System;
using Xunit;

namespace ProjectTestTask.Tests;

[AttributeUsage(AttributeTargets.Method)]
public sealed class AcceptanceFactAttribute : FactAttribute
{
    public AcceptanceFactAttribute()
    {
        if (Environment.GetEnvironmentVariable("RUN_ACCEPTANCE_TESTS") != "1")
        {
            Skip = "Requires Codespaces and provisioned Docker services.";
        }
    }
}
