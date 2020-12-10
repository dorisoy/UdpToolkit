using System;
using System.Linq;
using UnityEditor;

public static class BuildUtils
{
        private const string MacOS = "macos";

        public static void Build()
        {
                var buildType = Environment.GetCommandLineArgs().FirstOrDefault();
                switch (buildType)
                {
                        case MacOS:
                                BuildMacOS();
                                break;
                        default:
                                BuildMacOS();
                                break;
                }
        }

        private static void BuildMacOS()
        {
                var buildPlayerOptions = new BuildPlayerOptions();
                buildPlayerOptions.scenes = new[]
                {
                        "Assets/Scenes/Level.unity",
                };
                buildPlayerOptions.locationPathName = "../../../client_build/MacOSBuild";
                buildPlayerOptions.target = BuildTarget.StandaloneOSX;
                buildPlayerOptions.options = BuildOptions.None;
                BuildPipeline.BuildPlayer(buildPlayerOptions);
        }
}