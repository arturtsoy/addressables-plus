using System.Reflection;

namespace AssetBundles.GooglePlayAssetDelivery.Utils
{
    public static class ReflectionUtils
    {
        public static TReturn GetPrivateFieldValue<TReturn>(object subject, string name) where TReturn : class
        {
            return subject.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(subject) as TReturn;
        }
    }
}