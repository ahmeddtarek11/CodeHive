using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace CodeHive.Shared.Cqrs;

public interface ICommand : IRequest<Result> {}
public interface ICommand<T> : IRequest<Result<T>> { }


public interface ICommandHandler<TCmd>
: IRequestHandler<TCmd, Result> where TCmd : ICommand { }

public interface ICommandHandler<TCmd , T > 
 :IRequestHandler <TCmd, Result<T>> where TCmd :ICommand<T> {}

