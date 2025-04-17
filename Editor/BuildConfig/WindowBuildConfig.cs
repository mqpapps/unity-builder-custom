using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

namespace UNKO.Unity_Builder
{
    /// <summary>
    /// Window 빌드 설정
    /// </summary>
    [CreateAssetMenu(fileName = "WindowBuildConfig", menuName = GlobalConst.CreateAssetMenu_Prefix + "/WindowBuildConfig")]
    [StructLayout(LayoutKind.Auto)] // ignore codacy
    public class WindowBuildConfig : BuildConfigBase
    {
        ///<inheritdoc cref="IBuildConfig.buildTarget"/>
        public override BuildTarget buildTarget => BuildTarget.StandaloneWindows64;

        /// <summary>
        /// CPP 빌드를 할지 체크, CPP빌드는 오래 걸리므로 Test빌드가 아닌 Alpha 빌드부터 하는걸 권장
        /// </summary>
        [SerializeField]
        protected ScriptingImplementation scriptingBackEnd;

        ///<inheritdoc cref="IBuildConfig.ResetSetting"/>
        public override void ResetSetting(BuildConfigBase config)
        {
            base.ResetSetting(config);

            BuildTargetGroup targetGroup = BuildPipeline.GetBuildTargetGroup(buildTarget);
            scriptingBackEnd = PlayerSettings.GetScriptingBackend(targetGroup);
            config.AddBuildPath(
                     "\n_{bundleVersion}.{bundleVersionCode}" +
                     "\n_{scriptingBackEnd}");
        }

        ///<inheritdoc cref="IBuildConfig.OnPreBuild"/>
        public override void OnPreBuild(IDictionary<string, string> commandLine, ref BuildPlayerOptions buildPlayerOptions)
        {
            base.OnPreBuild(commandLine, ref buildPlayerOptions);

            string buildPath = GetBuildPath();
            DirectoryInfo directoryInfo = new DirectoryInfo(buildPath);
            DirectoryInfo parentDirectory = directoryInfo.Parent;
            Debug.Log($"build directory({parentDirectory.FullName}) is exist:{parentDirectory.Exists}");
            if (parentDirectory.Exists)
            {
                parentDirectory.Delete();
            }

            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, scriptingBackEnd);
        }

        ///<inheritdoc cref="IBuildConfig.GetBuildPath"/>
        public override string GetBuildPath()
        {
            return base.GetBuildPath()
                .Replace("{scriptingBackEnd}", scriptingBackEnd.ToString())
                + ".exe";
        }
    }

    /// <summary>
    /// <see cref="WindowBuildConfig"/> 인스펙터
    /// </summary>
    [CustomEditor(typeof(WindowBuildConfig))]
    public class WindowBuildConfig_Inspector : Editor
    {
        string _commandLine;

        ///<inheritdoc cref="Editor.OnInspectorGUI"/>
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            BuildConfigBase config = target as BuildConfigBase;
            if (GUILayout.Button("Reset to Current EditorSetting"))
            {
                config.ResetSetting(config);
            }

            if (GUILayout.Button("Build!"))
            {
                UnityBuilder.Build(config);
            }

            _commandLine = EditorGUILayout.TextField("commandLine", _commandLine);
            if (GUILayout.Button($"Build with \'{_commandLine}\'"))
            {
                string[] commands = _commandLine.Split(' ');
                for (int i = 0; i < commands.Length; i++)
                {
                    string command = commands[i];
                    bool hasNextCommand = i + 1 < commands.Length;
                    if (command.StartsWith("-"))
                    {
                        if (hasNextCommand)
                        {
                            Environment.SetEnvironmentVariable(command, commands[i + 1]);
                        }
                        else
                        {
                            Environment.SetEnvironmentVariable(command, "");
                        }
                    }
                }

                UnityBuilder.Build();
            }
        }
    }
}
