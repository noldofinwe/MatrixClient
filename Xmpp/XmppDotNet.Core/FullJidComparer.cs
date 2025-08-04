using System;
using System.Collections.Generic;

namespace XmppDotNet
{
    public class FullJidComparer : IComparer<Jid>   
    {
        #region IComparer<Jid> Members
        public int Compare(Jid x, Jid y)
        {
            if (x != null && y != null)
            {
                if (x.ToString() == y.ToString())
                    return 0;

                return String.CompareOrdinal(x.ToString(), y.ToString());
            }

            if (x == null)
            {
                if (y == null)
                {
                    return 0;
                }
                return -1;
            }
               
            return 1;
        }
        #endregion
    }
}
