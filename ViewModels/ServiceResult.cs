namespace WebApplication1.ViewModels
{
    public class ServiceResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? EntityId { get; set; }

        public static ServiceResult Ok(string message = "", int? id = null) =>
            new ServiceResult { Success = true, Message = message, EntityId = id };

        public static ServiceResult Fail(string message) =>
            new ServiceResult { Success = false, Message = message };
    }
}