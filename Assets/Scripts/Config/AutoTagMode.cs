using System;

namespace StlVault.Config
{
    [Flags]
    public enum AutoTagMode
    {
        None = 0b000,
        ExplodeFileName = 0b001,
        ExplodeSubDirPath = 0b011,
        ExplodeAbsolutePath = 0b111,
    }
}