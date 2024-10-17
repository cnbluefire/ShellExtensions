namespace ShellExtensions.ComInterfaces
{
    internal static class HResults
    {
        internal const int S_OK = 0;
        internal const int S_FALSE = 1;
        internal const int E_FAIL = unchecked((int)(0x80004005));
        internal const int E_INVALIDARG = unchecked((int)(0x80070057));
        internal const int E_NOTIMPL = unchecked((int)(0x80004001));
        internal const int E_NOINTERFACE = unchecked((int)(0x80004002));
        internal const int E_PENDING = unchecked((int)(0x8000000A));
        internal const int E_UNEXPECTED = unchecked((int)(0x8000FFFF));
        internal const int CLASS_E_CLASSNOTAVAILABLE = unchecked((int)(0x80040111));
        internal const int CLASS_E_NOAGGREGATION = unchecked((int)(0x80040110));
        internal const int RPC_E_CANTCALLOUT_ININPUTSYNCCALL = unchecked((int)(0x8001010D));
    }
}
