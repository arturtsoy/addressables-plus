namespace AssetBundles.GooglePlayAssetDelivery
{
    public class Debug
    {
        public static void LogFormat(string format, params object[] args)
        {
            UnityEngine.Debug.LogFormat(format, args);
        }
    }
}