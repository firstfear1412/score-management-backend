using Microsoft.AspNetCore.SignalR.Protocol;
using Newtonsoft.Json;

namespace ScoreManagement.Model
{
    public class ApiResponse<T>
    {
        public bool IsSuccess { get; set; } = false;
        public ApiMessage? Message { get; set; }
        //[System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        public T? ObjectResponse { get; set; }
        //[System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        public T? Parameter { get; set; }
        //[System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        public T? TokenResult { get; set; }
    }
    public class ApiMessage
    {
        //[System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        public string MessageKey { get; set; } = string.Empty;
        public string MessageDescription { get; set; } = string.Empty;
    }

}
