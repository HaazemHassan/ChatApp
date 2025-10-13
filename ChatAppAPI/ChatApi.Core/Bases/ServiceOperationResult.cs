using ChatApi.Core.Enums;

namespace ChatApi.Core.Bases {
    public class ServiceOperationResult<T> {

        public ServiceOperationStatus Status { get; private set; }
        public string? ErrorMessage { get; private set; }
        public T? Data { get; private set; }

        private ServiceOperationResult(ServiceOperationStatus status, string? errorMessage = null, T? data = default) {
            Status = status;
            ErrorMessage = errorMessage;
            Data = data;
        }

        public static ServiceOperationResult<T?> Success(T? data)
            => new ServiceOperationResult<T?>(ServiceOperationStatus.Succeeded, null, data);

        public static ServiceOperationResult<T?> Failure(ServiceOperationStatus status, string message)
            => new ServiceOperationResult<T?>(status, message);
    }
}