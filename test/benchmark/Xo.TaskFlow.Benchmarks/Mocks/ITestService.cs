using Xo.TaskFlow.Abstractions;
using Xo.TaskFlow.Core;
using Xo.TaskFlow.DependencyInjection.Extensions;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System;

namespace Xo.TaskFlow.Benchmarks;

public interface ITestService
{
  Task<bool> RunAsync(string strArg, bool boolArg);
}
