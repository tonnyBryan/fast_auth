namespace fast_auth.annotation
{

    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class TokenRequired : Attribute
    {
       
    }
}
