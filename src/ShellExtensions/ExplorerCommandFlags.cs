﻿namespace ShellExtensions
{
    [Flags]
    public enum ExplorerCommandFlags : uint
    {
        ECF_DEFAULT = 0,
        ECF_HASSUBCOMMANDS = 1,
        ECF_HASSPLITBUTTON = 2,
        ECF_HIDELABEL = 4,
        ECF_ISSEPARATOR = 8,
        ECF_HASLUASHIELD = 16,
        ECF_SEPARATORBEFORE = 32,
        ECF_SEPARATORAFTER = 64,
        ECF_ISDROPDOWN = 128,
        ECF_TOGGLEABLE = 256,
        ECF_AUTOMENUICONS = 512,
    }
}
