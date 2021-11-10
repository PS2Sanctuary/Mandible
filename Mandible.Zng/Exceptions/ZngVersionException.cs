using System;

namespace Mandible.Zng.Exceptions;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "RCS1194:Implement exception constructors.")]
public class ZngVersionException : Exception
{
    public byte ExpectedMajorVersion { get; }
    public string? ActualVersion { get; }

    public ZngVersionException(byte expectedMajorVersion, string? actualVersion)
    {
        ExpectedMajorVersion = expectedMajorVersion;
        ActualVersion = actualVersion;
    }

    public override string ToString()
    {
        return $"Expected Major Version: {(char)ExpectedMajorVersion} | Actual Version: {ActualVersion}";
    }
}
