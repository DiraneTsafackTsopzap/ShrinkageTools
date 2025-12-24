using Grpc.Core;

namespace DataAccess.CRUD.Extensions
{
    public static class RpcExceptions
    {
        public static RpcException InvalidArgument(string detail) =>
            new(new Status(StatusCode.InvalidArgument, detail));

        public static RpcException NotFound(string detail) =>
            new(new Status(StatusCode.NotFound, detail));

        public static RpcException Internal(string detail) =>
            new(new Status(StatusCode.Internal, detail));

        public static RpcException AlreadyExists(string detail) =>
            new(new Status(StatusCode.AlreadyExists, detail));
    }
}
