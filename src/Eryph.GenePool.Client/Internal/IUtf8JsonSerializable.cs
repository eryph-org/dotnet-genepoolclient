// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json;

namespace Eryph.GenePool.Client.Internal;

internal interface IUtf8JsonSerializable
{
    void Write(Utf8JsonWriter writer);
}