using BenchmarkDotNet.Running;
using System.Diagnostics.CodeAnalysis;
using System;

namespace Xo.TaskFlow.Benchmarks;

[ExcludeFromCodeCoverage]
public class Program
{
  public static void Main(string[] args)
  {
    BenchmarkRunner.Run<FunctitectBenchmarks>();
  }
}
