using System.IO;
using System.Text;
using UnityEngine;

internal static class ActionRunnerUnmanagedGenerate
{
    public const string TemplateAssetPath = "Assets/Scripts/ECS/ActionChain/Editor/ActionRunnerUnmanagedSystem.cs_template";
    public const string OutputAssetPath = "Assets/Scripts/ECS/ActionRunnerUnmanagedSystem.cs";

    public static void GenerateToFile(ActionMapBase actionMap)
    {
        var generatedCode = Generate(actionMap, TemplateAssetPath);
        var outputPath = GetAbsoluteProjectPath(OutputAssetPath);

        Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? string.Empty);
        File.WriteAllText(outputPath, generatedCode, new UTF8Encoding(false));

        UnityEditor.AssetDatabase.Refresh();
    }

    public static string Generate(ActionMapBase actionMap, string templateAssetPath)
    {
        var info = ActionRunnerUnmanagedGetInfo.Collect(actionMap);
        return Generate(info, templateAssetPath);
    }

    public static string GetAbsoluteProjectPath(string assetPath)
    {
        return Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), assetPath));
    }

    private static string Generate(ActionRunnerUnmanagedGenerationInfo info, string templateAssetPath)
    {
        var templatePath = GetAbsoluteProjectPath(templateAssetPath);
        if (File.Exists(templatePath) == false)
        {
            throw new FileNotFoundException($"Template not found: {templateAssetPath}");
        }

        var template = File.ReadAllText(templatePath);
        template = template.Replace("{_pass lookups_}", BuildPassLookups(info));
        template = template.Replace("{_define job lookups_}", BuildJobLookups(info));
        template = template.Replace("{_switch disable state_}", BuildDisableSwitch(info));
        template = template.Replace("{_switch enable state_}", BuildEnableSwitch(info));
        template = template.Replace("{_switch update state_}", BuildUpdateSwitch(info));
        template = template.Replace("{_subactions_}", BuildSubActions(info));

        return template;
    }

    private static string BuildPassLookups(ActionRunnerUnmanagedGenerationInfo info)
    {
        var readOnlyBuilder = new StringBuilder();
        var writableBuilder = new StringBuilder();

        for (int i = 0; i < info.Lookups.Count; i++)
        {
            var lookup = info.Lookups[i];
            var line = $"            {lookup.Name} = SystemAPI.Get{lookup.LookupTypeName}<{lookup.ElementTypeName}>({(lookup.IsWritable ? "false" : "true")}),";

            if (lookup.IsWritable)
            {
                writableBuilder.AppendLine(line);
            }
            else
            {
                readOnlyBuilder.AppendLine(line);
            }
        }

        if (readOnlyBuilder.Length > 0 && writableBuilder.Length > 0)
        {
            readOnlyBuilder.AppendLine();
        }

        readOnlyBuilder.Append(writableBuilder);
        return readOnlyBuilder.ToString().TrimEnd();
    }

    private static string BuildJobLookups(ActionRunnerUnmanagedGenerationInfo info)
    {
        var writableBuilder = new StringBuilder();
        var readOnlyBuilder = new StringBuilder();

        for (int i = 0; i < info.Lookups.Count; i++)
        {
            var lookup = info.Lookups[i];
            if (lookup.IsWritable)
            {
                writableBuilder.AppendLine($"    public {lookup.LookupTypeName}<{lookup.ElementTypeName}> {lookup.Name};");
            }
            else
            {
                readOnlyBuilder.AppendLine($"    [ReadOnly] public {lookup.LookupTypeName}<{lookup.ElementTypeName}> {lookup.Name};");
            }
        }

        if (writableBuilder.Length > 0 && readOnlyBuilder.Length > 0)
        {
            writableBuilder.AppendLine();
        }

        writableBuilder.Append(readOnlyBuilder);
        return writableBuilder.ToString().TrimEnd();
    }

    private static string BuildDisableSwitch(ActionRunnerUnmanagedGenerationInfo info)
    {
        var builder = new StringBuilder();

        for (int i = 0; i < info.SubActions.Count; i++)
        {
            var subAction = info.SubActions[i];
            builder.AppendLine($"            case SubActionTypes.{subAction}:");
            builder.AppendLine($"                Disable_{subAction}(entity, runner.Target, Buffer);");
            builder.AppendLine("                break;");
        }

        return builder.ToString().TrimEnd();
    }

    private static string BuildEnableSwitch(ActionRunnerUnmanagedGenerationInfo info)
    {
        var builder = new StringBuilder();

        for (int i = 0; i < info.SubActions.Count; i++)
        {
            var subAction = info.SubActions[i];
            builder.AppendLine($"            case SubActionTypes.{subAction}:");
            builder.AppendLine($"                Enable_{subAction}(entity, runner.Target, Buffer, ref randomComponent.Random);");
            builder.AppendLine("                break;");
        }

        return builder.ToString().TrimEnd();
    }

    private static string BuildUpdateSwitch(ActionRunnerUnmanagedGenerationInfo info)
    {
        var builder = new StringBuilder();

        for (int i = 0; i < info.SubActions.Count; i++)
        {
            var subAction = info.SubActions[i];
            builder.AppendLine($"            case SubActionTypes.{subAction}:");
            builder.AppendLine($"                return Update_{subAction}(entity, runner.Target, Buffer, in timer, ref randomComponent.Random);");
        }

        builder.AppendLine("            default:");
        builder.AppendLine("                return SubActionResult.Fail(-1);");

        return builder.ToString().TrimEnd();
    }

    private static string BuildSubActions(ActionRunnerUnmanagedGenerationInfo info)
    {
        var builder = new StringBuilder();

        for (int i = 0; i < info.GeneratedStates.Count; i++)
        {
            var generatedState = info.GeneratedStates[i];

            if (i > 0)
            {
                builder.AppendLine();
            }

            builder.AppendLine("    // =========================================================================");
            builder.AppendLine($"    // {generatedState.SubAction}");
            builder.AppendLine("    // =========================================================================");
            builder.AppendLine();

            for (int j = 0; j < generatedState.Methods.Count; j++)
            {
                builder.AppendLine(generatedState.Methods[j]);
                if (j < generatedState.Methods.Count - 1)
                {
                    builder.AppendLine();
                }
            }
        }

        return builder.ToString().TrimEnd();
    }
}
