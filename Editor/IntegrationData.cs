using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GlitchSDK.IntegrationManager.Editor
{
    [CreateAssetMenu(fileName = "IntegrationData", menuName = "Joyglitch/Plugins/IntegrationData")]
    public class IntegrationData : ScriptableObject
    {
        public List<PackageData> packages;
    }

    [System.Serializable]
    public class PackageData
    {
        public string repoName;
    }

    public class ReleaseDTO
    {
        public string tag_name;
        public string published_at;
        public List<ReleaseAsset> assets;
    }

    public class ReleaseAsset
    {
        public string browser_download_url;
        public string body;
    }
}
