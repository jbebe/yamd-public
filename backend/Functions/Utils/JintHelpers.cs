using Jint.Native.Array;
using Jint.Runtime;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace Functions.Utils
{
    public class JintHelpers
    {
        public static List<T> SerializeArray<T>(ArrayInstance instance)
        {
            var arrayLength = TypeConverter.ToInt32(instance.Get("length"));
            var mediaList = new List<T>();
            for (var i = 0; i < arrayLength; ++i)
            {
                string propName = i.ToString();
                var isOwnProperty = instance.HasProperty(propName);
                if (isOwnProperty)
                {
                    // get and deserialize js object
                    var jsValue = instance.Get(propName);
                    var jObj = JObject.FromObject(jsValue.ToObject());
                    mediaList.Add(jObj.ToObject<T>());
                }
            }

            return mediaList;
        }
    }
}
