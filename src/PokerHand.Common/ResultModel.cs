namespace PokerHand.Common
{
    public class ResultModel
    {
        public string Message { get; set; }
        public bool IsSuccess { get; set; }
    }
    
    public class ResultModel<T> : ResultModel
    {
        public T Value { get; set; }

        public ResultModel()
        { }

        public ResultModel(T value)
        {
            Value = value;
        }
    }
}