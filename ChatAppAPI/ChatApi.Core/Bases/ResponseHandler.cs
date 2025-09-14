namespace ChatApi.Core.Bases {
    public class ResponseHandler {



        public Response<T> Deleted<T>(string? message = null) {

            return new Response<T>() {
                StatusCode = System.Net.HttpStatusCode.OK,
                Succeeded = true,
                Message = message is null ? "Deleted Successfully" : message
            };
        }
        public Response<T> Success<T>(T? entity, object? Meta = null) {
            return new Response<T>() {
                Data = entity,
                StatusCode = System.Net.HttpStatusCode.OK,
                Succeeded = true,
                Message = "Operation Succeeded",
                Meta = Meta
            };
        }
        public Response<string> Success(object? Meta = null) {
            return new Response<string>() {
                StatusCode = System.Net.HttpStatusCode.OK,
                Succeeded = true,
                Message = "Operation Succeeded",
                Meta = Meta
            };
        }
        public Response<T> Unauthorized<T>(string? message = null) {
            return new Response<T>() {
                StatusCode = System.Net.HttpStatusCode.Unauthorized,
                Succeeded = false,
                Message = message ?? "Unauthorized Access"
            };
        }

        public Response<T> Forbid<T>(string? message = null) {
            return new Response<T>() {
                StatusCode = System.Net.HttpStatusCode.Forbidden,
                Succeeded = false,
                Message = message ?? "Access Forbidden"
            };
        }

        public Response<T> BadRequest<T>(string? Message = null) {
            return new Response<T>() {
                StatusCode = System.Net.HttpStatusCode.BadRequest,
                Succeeded = false,
                Message = Message == null ? "Bad Request" : Message
            };
        }

        public Response<T> Conflict<T>(string? Message = null) {
            return new Response<T>() {
                StatusCode = System.Net.HttpStatusCode.Conflict,
                Succeeded = false,
                Message = Message ?? "Conflict"
            };
        }

        public Response<T> NotFound<T>(string? message = null) {
            return new Response<T>() {
                StatusCode = System.Net.HttpStatusCode.NotFound,
                Succeeded = false,
                Message = message ?? "Not Found"
            };
        }

        public Response<T> Created<T>(T entity, object? Meta = null) {
            return new Response<T>() {
                Data = entity,
                StatusCode = System.Net.HttpStatusCode.Created,
                Succeeded = true,
                Message = "Created Successfully",
                Meta = Meta
            };
        }

        public Response<string> Created(string? message = null, object? Meta = null) {
            return new Response<string>() {
                StatusCode = System.Net.HttpStatusCode.Created,
                Succeeded = true,
                Message = "Created Successfully",
                Meta = Meta
            };
        }


        public Response<T> Updated<T>() {
            return new Response<T>() {
                StatusCode = System.Net.HttpStatusCode.OK,
                Succeeded = true,
                Message = "Updated Successfully"
            };
        }
    }
}
