﻿using System.Runtime.CompilerServices;

namespace Shared
{
    public static class TaskExtensions
    {
        //--------------------------------------------------------Attributes:-----------------------------------------------------------------\\
        #region --Attributes--


        #endregion
        //--------------------------------------------------------Constructor:----------------------------------------------------------------\\
        #region --Constructors--


        #endregion
        //--------------------------------------------------------Set-, Get- Methods:---------------------------------------------------------\\
        #region --Set-, Get- Methods--


        #endregion
        //--------------------------------------------------------Misc Methods:---------------------------------------------------------------\\
        #region --Misc Methods (Public)--
        /// <summary>
        /// Configures await to false.
        /// Same as: .ConfigureAwait(false)
        /// </summary>
        public static ConfiguredTaskAwaitable ConfAwaitFalse(this Task task)
        {
            return task.ConfigureAwait(false);
        }

        /// <summary>
        /// Configures await to false.
        /// Same as: .ConfigureAwait(false)
        /// </summary>
        public static ConfiguredTaskAwaitable<T> ConfAwaitFalse<T>(this Task<T> task)
        {
            return task.ConfigureAwait(false);
        }

        #endregion

        #region --Misc Methods (Private)--


        #endregion

        #region --Misc Methods (Protected)--


        #endregion
        //--------------------------------------------------------Events:---------------------------------------------------------------------\\
        #region --Events--


        #endregion
    }
}
