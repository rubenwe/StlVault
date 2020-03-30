using System;

namespace StlVault.Config
{
    [Flags]
    public enum AutoTagMode
    {
        None = 0b000,
        ExplodeFileName = 0b001,
        ExplodeResourcePath = 0b011
    }
}