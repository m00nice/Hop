using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
class InfoFile : IPostprocessBuildWithReport
{
    public int callbackOrder { get { return 0; } }

    public void OnPostprocessBuild(BuildReport report)
    {
        if (report.summary.result == BuildResult.Unknown || report.summary.result == BuildResult.Succeeded)
        {
            List<InfoFileData> allDatas = FindAssetsByType<InfoFileData>();
            
            if (allDatas.Count == 1)
            {
                string path = Path.GetDirectoryName(report.summary.outputPath);

                allDatas[0].CreateDataFile(path + "\\info.txt");

                if (allDatas[0].gameThumb != null)
                {
                    string assetPath = AssetDatabase.GetAssetPath(allDatas[0].gameThumb);
                    string filePath = Path.Combine(Directory.GetCurrentDirectory(), assetPath);
                    
                    //filePath = filePath.Replace("/", "\\");

                    if (filePath.ToLower().Contains(".png"))
                    {
                        File.Copy(filePath, path + "\\thumb.png");
                    }
                    else
                    {
                        Debug.Log("Couldn't include Game Thumb. Seems to not be a .png file!");
                    }
                    
                    
                }
                if (allDatas[0].screenShot != null)
                {
                    string assetPath = AssetDatabase.GetAssetPath(allDatas[0].screenShot);
                    string filePath = Path.Combine(Directory.GetCurrentDirectory(), assetPath);
                    
                    //filePath = filePath.Replace("/", "\\");

                    if (filePath.ToLower().Contains(".png"))
                    {
                        File.Copy(filePath, path + "\\screen.png");
                    }
                    else
                    {
                        Debug.Log("Couldn't include Screen shot. Seems to not be a .png file!");
                    }


                }
                // i.e. "C:\Your Unity Project\Assets\Some\Path\YourObject.prefab"




            }
            else if (allDatas.Count > 1)
            {
                Debug.Log("Too many Info files in project. Delete all but THE ONE!");
            }
        }
        

    }

    List<T> FindAssetsByType<T>() where T : UnityEngine.Object
    {
        List<T> assets = new List<T>();
        string[] guids = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(T)));
        for (int i = 0; i < guids.Length; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (asset != null)
            {
                assets.Add(asset);
            }
        }
        return assets;
    }
}