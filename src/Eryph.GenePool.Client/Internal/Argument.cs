﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

#if AZURE_NULLABLE
#nullable enable
#endif

using System;
using System.Collections;
using System.Collections.Generic;

namespace Eryph.GenePool.Client.Internal;

public static class Argument
{
    // TODO: Add nullability attributes as needed when possible.

    /// <summary>
    /// Throws if <paramref name="value"/> is null.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="name">The name of the parameter.</param>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
#if AZURE_NULLABLE
        public static void AssertNotNull<T>([AllowNull, NotNull] T value, string name)
#else
    public static void AssertNotNull<T>(T value, string name)
#endif
    {
        if (value is null)
        {
            throw new ArgumentNullException(name);
        }
    }

    /// <summary>
    /// Throws if <paramref name="value"/> has not been initialized.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="name">The name of the parameter.</param>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> has not been initialized.</exception>
    public static void AssertNotNull<T>(T? value, string name) where T : struct
    {
        if (!value.HasValue)
        {
            throw new ArgumentNullException(name);
        }
    }

    /// <summary>
    /// Throws if <paramref name="value"/> is null or an empty collection.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="name">The name of the parameter.</param>
    /// <exception cref="ArgumentException"><paramref name="value"/> is an empty collection.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
#if AZURE_NULLABLE
        public static void AssertNotNullOrEmpty<T>([AllowNull, NotNull] IEnumerable<T> value, string name)
#else
    public static void AssertNotNullOrEmpty<T>(IEnumerable<T> value, string name)
#endif
    {
        switch (value)
        {
            case null:
                throw new ArgumentNullException(name);
            // .NET Framework's Enumerable.Any() always allocates an enumerator, so we optimize for collections here.
            case ICollection<T> { Count: 0 }:
                throw new ArgumentException("Value cannot be an empty collection.", name);
            case ICollection { Count: 0 }:
                throw new ArgumentException("Value cannot be an empty collection.", name);
        }

        using IEnumerator<T> e = value.GetEnumerator();
        if (!e.MoveNext())
        {
            throw new ArgumentException("Value cannot be an empty collection.", name);
        }
    }

    /// <summary>
    /// Throws if <paramref name="value"/> is null or an empty string.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="name">The name of the parameter.</param>
    /// <exception cref="ArgumentException"><paramref name="value"/> is an empty string.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
#if AZURE_NULLABLE
        public static void AssertNotNullOrEmpty([AllowNull, NotNull] string value, string name)
#else
    public static void AssertNotNullOrEmpty(string? value, string name)
#endif
    {
        if (value is null)
        {
            throw new ArgumentNullException(name);
        }

        if (value.Length == 0)
        {
            throw new ArgumentException("Value cannot be an empty string.", name);
        }
    }

    /// <summary>
    /// Throws if <paramref name="value"/> is null, an empty string, or consists only of white-space characters.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="name">The name of the parameter.</param>
    /// <exception cref="ArgumentException"><paramref name="value"/> is an empty string or consists only of white-space characters.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
#if AZURE_NULLABLE
        public static void AssertNotNullOrWhiteSpace([AllowNull, NotNull] string value, string name)
#else
    public static void AssertNotNullOrWhiteSpace(string? value, string name)
#endif
    {
        if (value is null)
        {
            throw new ArgumentNullException(name);
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be empty or contain only white-space characters.", name);
        }
    }

    /// <summary>
    /// Throws if <paramref name="value"/> is the default value for type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of structure to validate which implements <see cref="IEquatable{T}"/>.</typeparam>
    /// <param name="value">The value to validate.</param>
    /// <param name="name">The name of the parameter.</param>
    /// <exception cref="ArgumentException"><paramref name="value"/> is the default value for type <typeparamref name="T"/>.</exception>
    public static void AssertNotDefault<T>(ref T value, string name) where T : struct, IEquatable<T>
    {
        if (value.Equals(default))
        {
            throw new ArgumentException("Value cannot be empty.", name);
        }
    }

    /// <summary>
    /// Throws if <paramref name="value"/> is less than the <paramref name="minimum"/> or greater than the <paramref name="maximum"/>.
    /// </summary>
    /// <typeparam name="T">The type of to validate which implements <see cref="IComparable{T}"/>.</typeparam>
    /// <param name="value">The value to validate.</param>
    /// <param name="minimum">The minimum value to compare.</param>
    /// <param name="maximum">The maximum value to compare.</param>
    /// <param name="name">The name of the parameter.</param>
    public static void AssertInRange<T>(T value, T minimum, T maximum, string name) where T : notnull, IComparable<T>
    {
        if (minimum.CompareTo(value) > 0)
        {
            throw new ArgumentOutOfRangeException(name, "Value is less than the minimum allowed.");
        }

        if (maximum.CompareTo(value) < 0)
        {
            throw new ArgumentOutOfRangeException(name, "Value is greater than the maximum allowed.");
        }
    }

    /// <summary>
    /// Throws if <paramref name="value"/> is not defined for <paramref name="enumType"/>.
    /// </summary>
    /// <param name="enumType">The type to validate against.</param>
    /// <param name="value">The value to validate.</param>
    /// <param name="name">The name of the parameter.</param>
    /// <exception cref="ArgumentException"><paramref name="value"/> is not defined for <paramref name="enumType"/>.</exception>
    public static void AssertEnumDefined(Type enumType, object value, string name)
    {
        if (!Enum.IsDefined(enumType, value))
        {
            throw new ArgumentException($"Value not defined for {enumType.FullName}.", name);
        }
    }

    /// <summary>
    /// Throws if <paramref name="value"/> has not been initialized; otherwise, returns <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="name">The name of the parameter.</param>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> has not been initialized.</exception>
#if AZURE_NULLABLE
        public static T CheckNotNull<T>([AllowNull, NotNull] T value, string name) where T : class
#else
    public static T CheckNotNull<T>(T value, string name) where T : class
#endif
    {
        AssertNotNull(value, name);
        return value;
    }

    /// <summary>
    /// Throws if <paramref name="value"/> is null or an empty string; otherwise, returns <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="name">The name of the parameter.</param>
    /// <exception cref="ArgumentException"><paramref name="value"/> is an empty string.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
#if AZURE_NULLABLE
        public static string CheckNotNullOrEmpty([AllowNull, NotNull] string value, string name)
#else
    public static string CheckNotNullOrEmpty(string? value, string name)
#endif
    {
        AssertNotNullOrEmpty(value, name);
        return value!;
    }

    /// <summary>
    /// Throws if <paramref name="value"/> is not null.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="name">The name of the parameter.</param>
    /// <param name="message">The error message.</param>
    /// <exception cref="ArgumentException"><paramref name="value"/> is not null.</exception>
#if AZURE_NULLABLE
        public static void AssertNull<T>([AllowNull] T value, string name, [AllowNull] string message = null)
#else
#nullable enable
    public static void AssertNull<T>(T value, string name, string? message = null)
#nullable disable
#endif
    {
        if (value is not null)
        {
            throw new ArgumentException(message ?? "Value must be null.", name);
        }
    }
}