namespace BlogApi.ViewModels
{
    public class ResultViewModel<T>
    {
        public T Payload { get; private set; }
        public List<string> Errors { get; private set; } = new();

        public ResultViewModel(T payload) // Success
        {
            Payload = payload;
        }

        public ResultViewModel(List<string> errors)
        {
            Errors = errors;
        }

        public ResultViewModel(string error)
        {
            Errors.Add(error);
        }
    }
}