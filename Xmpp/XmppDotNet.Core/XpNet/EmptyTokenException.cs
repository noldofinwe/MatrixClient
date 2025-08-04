﻿/*
 * xpnet is a deriviative of James Clark's XP parser.
 * See copying.txt for more info.
 */
namespace XmppDotNet.XpNet
{
    /// <summary>
    /// An empty token was detected. This only happens with a buffer of length 0 is passed in
    /// to the parser.
    /// </summary>
    internal class EmptyTokenException : TokenException
    {
    }
}
