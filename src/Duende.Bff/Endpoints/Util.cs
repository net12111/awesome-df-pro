// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using System;

namespace Duende.Bff
{
    internal static class Util
    {
        internal static bool IsLocalUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return false;
            }

            switch (url[0])
            {
                // Allows "/" or "/foo" but not "//" or "/\".
                // url is exactly "/"
                case '/' when url.Length == 1:
                    return true;
                // url doesn't start with "//" or "/\"
                case '/' when url[1] != '/' && url[1] != '\\':
                    return !HasControlCharacter(url.AsSpan(1));
                case '/':
                    return false;
                // Allows "~/" or "~/foo" but not "~//" or "~/\".
                case '~' when url.Length > 1 && url[1] == '/':
                {
                    // url is exactly "~/"
                    if (url.Length == 2)
                    {
                        return true;
                    }

                    // url doesn't start with "~//" or "~/\"
                    if (url[2] != '/' && url[2] != '\\')
                    {
                        return !HasControlCharacter(url.AsSpan(2));
                    }

                    return false;
                }
            }

            return false;


            static bool HasControlCharacter(ReadOnlySpan<char> readOnlySpan)
            {
                // URLs may not contain ASCII control characters.
                foreach (var t in readOnlySpan)
                {
                    if (char.IsControl(t))
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }
}