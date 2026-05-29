using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace CodeHive.Shared.Cqrs;

public interface IQuery<T> : IRequest<Result<T>> {}

public interface IQueryHandler<TQry, T>
: IRequestHandler<TQry, Result<T>> where TQry : IQuery<T> { }

