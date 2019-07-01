// <copyright file="Extensions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace AsyncClientServer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Extension methods.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Throw ArgumentNullException if the specified argument is null with the given name.
        /// </summary>
        /// <typeparam name="T">Type of the argument.</typeparam>
        /// <param name="argument">Argument which will be tested.</param>
        /// <param name="name">Name of the argument.</param>
        /// <returns>Given argument if it's not null, otherwise throws exception.</returns>
        public static T ThrowIfNull<T>(this T argument, string name)
        {
            if (argument == null)
            {
                throw new ArgumentNullException(name);
            }

            return argument;
        }
    }
}
