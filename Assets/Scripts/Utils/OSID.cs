using System;
using System.IO;

namespace Utils
{
    public static class OSID
    {
        private static bool _Uninitialized = true;
        private static Platform _Current;
        public static Platform Current
        {
            get
            {
                if (_Uninitialized)
                {
                    _Uninitialized = false;
                    switch (Environment.OSVersion.Platform)
                    {
                        case PlatformID.Unix:
                            // Well, there are chances MacOSX is reported as Unix instead of MacOSX.
                            // Instead of platform check, we'll do a feature checks (Mac specific root folders)
                            if (Directory.Exists("/Applications")
                                & Directory.Exists("/System")
                                & Directory.Exists("/Users")
                                & Directory.Exists("/Volumes"))
                                _Current= Platform.Mac;
                            _Current= Platform.Linux;
                            break;
                        case PlatformID.MacOSX:
                            _Current = Platform.Mac;
                            break;
                        default:
                            _Current = Platform.Windows;
                            break;
                    }
                }
                return _Current;

            }

        }
    }
}

