//namespace Xo.TaskFlow.Abstractions;

///// <summary>
/////   A branch, that will evaluate the results of a workflow and invoke the root node of a new branch. 
///// </summary>
//public interface IFlow<T1, T2, T3>
//{
///// <summary>
///// Core branching logic. 
///// </summary>
///// <param name="msg">Optional msgs for the branching logic to use in its evaluation.</param>
///// <returns><see cref="IBranch"/></returns>
//INode Build(IWorkflowContext? context = null);
//}

//internal class Flow<T1, T2, T3> : IFlow<T1, T2, T3>
//{
//private readonly IFunctitect _functitect;
//private readonly INodeFactory _nodeFactory;
//private readonly IMsgFactory _msgFactory;

//public INode Build(IWorkflowContext? context = null)
//{
//var fn1 = this._functitect.Build<T1>();
//var fn1IsAsync = IsAsync(typeof(T1));
//var fn1Method = typeof(T1).GetMethods().First();
//var fn1ReturnType = fn1Method.ReturnType;
//var fn1Params = fn1Method.GetParameters();
//var n1 = this._nodeFactory
//.Create(context);
//if (fn1IsAsync) n1.SetFunctory(fn1 as IAsyncFunctory);
//else n1.SetFunctory(fn1 as ISyncFunctory);
//var n1Id = n1.Id;

//var fn2 = this._functitect.Build<T2>();
//var fn2IsAsync = IsAsync(typeof(T2));
//var fn2Method = typeof(T2).GetMethods().First();
//var fn2ReturnType = fn2Method.ReturnType;
//var fn2Params = fn2Method.GetParameters();
//var n2 = this._nodeFactory
//.Create(context)
//.AddArg(n1);
//if (fn2IsAsync) n2.SetFunctory(fn2 as IAsyncFunctory);
//else n2.SetFunctory(fn2 as ISyncFunctory);
//var n2Id = n2.Id;

//var reuse = fn2Params.FirstOrDefault(p => p.ParameterType.FullName == fn1ReturnType.FullName);
//if (reuse is not null)
//{
//n2.AddArg(c => this._msgFactory.Create(c.GetMsg(n1Id).SetParam(reuse.Name!)));
//}

//var fn3 = this._functitect.Build<T3>();
//var fn3IsAsync = IsAsync(typeof(T3));
//var fn3Method = typeof(T3).GetMethods().First();
//var fn3ReturnType = fn3Method.ReturnType;
//var fn3Params = fn3Method.GetParameters();
//var n3 = this._nodeFactory
//.Create(context)
//.AddArg(n2);
//if (fn3IsAsync) n3.SetFunctory(fn3 as IAsyncFunctory);
//else n3.SetFunctory(fn3 as ISyncFunctory);
//var n3Id = n3.Id;

//var reuse2 = fn3Params.FirstOrDefault(p => p.ParameterType.FullName == fn2ReturnType.FullName);
//if (reuse2 is not null)
//{
//n3.AddArg(c => this._msgFactory.Create(c.GetMsg(n2Id).SetParam(reuse2.Name!)));
//}

//return n3;
//}

//private static bool IsAsync(Type type)
//{
//var methodInfo = type.GetMethods().First();
//return TypeInspector.MethodHasReturnTypeOfTask(methodInfo);
//}
//}

//public static class tmp
//{
//public static void drive()
//{

//var flow = new 



//}
//}
