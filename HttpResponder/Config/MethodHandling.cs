namespace HttpResponder.Config
{
    using System.Collections.Generic;

    public class MethodHandling : Dictionary<string, IResponseSpec>
    {
        public IResponseSpec FindMatching(string path)
        {
            if (this.ContainsKey(path))
            {
                return this[path];
            }

            if (this.ContainsKey("*"))
            {
                return this["*"];
            }

            return null;
        }
    }
}