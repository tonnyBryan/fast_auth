namespace fast_auth.model.dto
{
    public class Error
    {
        public int Code { get; set; }

        public Error(int code)
        {
            this.Code = code;
        }
    }
}
