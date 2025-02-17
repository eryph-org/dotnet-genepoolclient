﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Eryph.GenePool.Client.Internal;

#if !NET7_0_OR_GREATER
/// <summary>
/// Indicates that the specified method requires the ability to generate new code at runtime,
/// for example through <see cref="System.Reflection"/>.
/// </summary>
/// <remarks>
/// This allows tools to understand which methods are unsafe to call when compiling ahead of time.
/// </remarks>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Class, Inherited = false)]
internal sealed class RequiresDynamicCodeAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RequiresDynamicCodeAttribute"/> class
    /// with the specified message.
    /// </summary>
    /// <param name="message">
    /// A message that contains information about the usage of dynamic code.
    /// </param>
    public RequiresDynamicCodeAttribute(string message)
    {
        Message = message;
    }

    /// <summary>
    /// Gets a message that contains information about the usage of dynamic code.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets or sets an optional URL that contains more information about the method,
    /// why it requires dynamic code, and what options a consumer has to deal with it.
    /// </summary>
    public string? Url { get; set; }
}
#endif

#if !NET5_0_OR_GREATER

#endif
