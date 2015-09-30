namespace HttpResponder.Config
{
    using System.Collections.Generic;

    public class Configuration : Dictionary<string, MethodHandling>
    {
        public IResponseSpec FindMatching(string method, string path)
        {
            if (this.ContainsKey(method.ToLower()))
            {
                return this[method.ToLower()].FindMatching(path);
            }

            if (this.ContainsKey("*"))
            {
                return this["*"].FindMatching(path);
            }

            return null;
        }
    }
}