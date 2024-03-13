using Eryph.GenePool.Model;

namespace Eryph.GenePool.Packing;

public record PackableFile(string FullPath, string FileName, GeneType GeneType, string GeneName, bool ExtremeCompression);