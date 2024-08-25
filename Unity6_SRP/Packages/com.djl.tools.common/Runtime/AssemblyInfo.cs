using System.Runtime.CompilerServices;
using UnityEditor;

[assembly: InternalsVisibleTo("Tools.DJLCommon.Editor")]
[assembly: InternalsVisibleTo("Tools.DJLCommon.Runtime")] // Allow third party extensions
[assembly: InternalsVisibleTo("Tools.DJLAssetbundle.Runtime")]
[assembly: InternalsVisibleTo("Tools.DJLPathFind.Tests")]

