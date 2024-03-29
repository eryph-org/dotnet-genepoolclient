﻿using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Eryph.GenePool.Model;



public class GeneManifestData
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }


    [JsonPropertyName("format")]
    public string? Format { get; set; }


    [JsonPropertyName("filename")]
    public string? FileName { get; set; }


    [JsonPropertyName("parts")]
    public string[]? Parts { get; set; }


    [JsonPropertyName("size")]
    public long? Size { get; set; }

    [JsonPropertyName("original_size")]
    public long? OriginalSize { get; set; }

}
