using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using LittleAI.Enums;
using Unity.Entities;
using UnityEditor;

internal static class ActionRunnerUnmanagedGetInfo
{
    private const string MoveControllerLookupKey = "ComponentLookup:MoveControllerInputComponent";

    private static readonly Regex MethodStartRegex = new(
        @"^[ \t]*(public|private|protected|internal)\s+(?:static\s+)?(?:unsafe\s+)?[\w<>\[\],\.]+\s+(?<name>[A-Za-z_]\w*)\s*\(",
        RegexOptions.Multiline);

    public static ActionRunnerUnmanagedGenerationInfo Collect(ActionMapBase actionMap)
    {
        if (actionMap == null)
        {
            throw new InvalidOperationException("Action map is not assigned.");
        }

        var subActions = CollectUsedSubActions(actionMap);
        using var context = new GeneratorContext(actionMap);
        var states = context.CreateStates();

        var allLookups = new List<LookupInfo>();
        var canonicalLookupsByKey = new Dictionary<string, LookupInfo>();
        var generatedStates = new List<GeneratedStateInfo>();

        for (int i = 0; i < subActions.Count; i++)
        {
            var subAction = subActions[i];
            if (states.TryGetValue(subAction, out var state) == false)
            {
                throw new InvalidOperationException($"ConstructSubActionsStates did not return a state for {subAction}.");
            }

            var stateType = state.GetType();
            var sourcePath = FindSourcePath(stateType);
            var source = File.ReadAllText(ActionRunnerUnmanagedGenerate.GetAbsoluteProjectPath(sourcePath));
            var methodBlocks = ExtractMethods(source, stateType.Name);

            var stateLookups = CollectLookupInfos(stateType, subAction);

            for (int j = 0; j < stateLookups.Count; j++)
            {
                var lookup = stateLookups[j];
                allLookups.Add(lookup);

                if (canonicalLookupsByKey.ContainsKey(lookup.Key) == false)
                {
                    canonicalLookupsByKey.Add(lookup.Key, lookup.Clone());
                }
            }

            var helperRenameMap = BuildHelperRenameMap(subAction, methodBlocks);
            var rewrittenMethods = new List<string>();

            for (int j = 0; j < methodBlocks.Count; j++)
            {
                rewrittenMethods.Add(RewriteMethodBlock(
                    subAction,
                    methodBlocks[j],
                    stateLookups,
                    canonicalLookupsByKey,
                    helperRenameMap));
            }

            generatedStates.Add(new GeneratedStateInfo(subAction, rewrittenMethods));
        }

        var finalLookups = BuildFinalLookups(allLookups, canonicalLookupsByKey, generatedStates);

        return new ActionRunnerUnmanagedGenerationInfo(subActions, finalLookups, generatedStates);
    }

    private static List<SubActionTypes> CollectUsedSubActions(ActionMapBase actionMap)
    {
        var ordered = new List<SubActionTypes>();
        var seen = new HashSet<SubActionTypes>();
        var mapItems = actionMap.GetActionsMapList();

        for (int i = 0; i < mapItems.Count; i++)
        {
            var item = mapItems[i];
            for (int j = 0; j < item.SubActions.Count; j++)
            {
                var subAction = item.SubActions[j];
                if (seen.Add(subAction))
                {
                    ordered.Add(subAction);
                }
            }
        }

        if (ordered.Count == 0)
        {
            throw new InvalidOperationException("The selected action map does not contain any subactions.");
        }

        return ordered;
    }

    private static List<LookupInfo> CollectLookupInfos(Type stateType, SubActionTypes subActionType)
    {
        var result = new List<LookupInfo>();
        var fields = stateType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

        for (int i = 0; i < fields.Length; i++)
        {
            if (TryCreateLookupInfo(fields[i], subActionType, out var lookup))
            {
                result.Add(lookup);
            }
        }

        return result;
    }

    private static bool TryCreateLookupInfo(FieldInfo fieldInfo, SubActionTypes subActionType, out LookupInfo lookup)
    {
        lookup = null;

        var fieldType = fieldInfo.FieldType;
        if (fieldType.IsGenericType == false)
        {
            return false;
        }

        var genericType = fieldType.GetGenericTypeDefinition();
        var elementType = fieldType.GetGenericArguments()[0];

        string lookupTypeName;
        if (genericType == typeof(ComponentLookup<>))
        {
            lookupTypeName = "ComponentLookup";
        }
        else if (genericType == typeof(BufferLookup<>))
        {
            lookupTypeName = "BufferLookup";
        }
        else
        {
            return false;
        }

        lookup = new LookupInfo
        {
            SubActionType = subActionType,
            Key = $"{lookupTypeName}:{elementType.FullName}",
            Name = fieldInfo.Name,
            LookupTypeName = lookupTypeName,
            ElementTypeName = elementType.Name,
        };

        return true;
    }

    private static Dictionary<string, string> BuildHelperRenameMap(SubActionTypes subAction, List<MethodBlock> methods)
    {
        var result = new Dictionary<string, string>();

        for (int i = 0; i < methods.Count; i++)
        {
            var methodName = methods[i].Name;
            if (IsInterfaceMethod(methodName))
            {
                continue;
            }

            result[methodName] = $"{subAction}_{methodName}";
        }

        return result;
    }

    private static string RewriteMethodBlock(
        SubActionTypes subAction,
        MethodBlock method,
        List<LookupInfo> stateLookups,
        Dictionary<string, LookupInfo> canonicalLookupsByKey,
        Dictionary<string, string> helperRenameMap)
    {
        var methodText = method.Text;
        var bodyStart = methodText.IndexOf('{');
        if (bodyStart < 0)
        {
            throw new InvalidOperationException($"Could not parse method body for {method.Name}.");
        }

        var signature = methodText.Substring(0, bodyStart);
        var body = methodText.Substring(bodyStart);

        var generatedName = GetGeneratedMethodName(subAction, method.Name, helperRenameMap);
        signature = Regex.Replace(signature, $@"\b{Regex.Escape(method.Name)}\b(?=\s*\()", generatedName);

        for (int i = 0; i < stateLookups.Count; i++)
        {
            var lookup = stateLookups[i];
            var canonicalLookup = canonicalLookupsByKey[lookup.Key];
            var pattern = $@"\b{Regex.Escape(lookup.Name)}\b";

            signature = Regex.Replace(signature, pattern, canonicalLookup.Name);
            body = Regex.Replace(body, pattern, canonicalLookup.Name);
        }

        foreach (var pair in helperRenameMap)
        {
            var pattern = $@"\b{Regex.Escape(pair.Key)}\b";
            signature = Regex.Replace(signature, pattern, pair.Value);
            body = Regex.Replace(body, pattern, pair.Value);
        }

        var moveControllerLookup = FindLookupByKey(stateLookups, canonicalLookupsByKey, MoveControllerLookupKey);
        if (moveControllerLookup != null)
        {
            body = RewriteMoveControllerCalls(body, moveControllerLookup.Name);
        }

        return $"{signature}{body}".TrimEnd();
    }

    private static string RewriteMoveControllerCalls(string body, string lookupName)
    {
        body = Regex.Replace(
            body,
            @"MoveControllerExtensions\.Enable\(\s*buffer\s*,\s*(?<entity>[^)]+?)\s*\)",
            match => $"{lookupName}.Enable({match.Groups["entity"].Value.Trim()})");

        body = Regex.Replace(
            body,
            @"MoveControllerExtensions\.ResetInput\(\s*buffer\s*,\s*(?<entity>[^)]+?)\s*\)",
            match => $"{lookupName}.ResetInput({match.Groups["entity"].Value.Trim()})");

        body = Regex.Replace(
            body,
            @"MoveControllerExtensions\.SetTarget\(\s*buffer\s*,\s*(?<entity>[^,]+)\s*,",
            match => $"{lookupName}.SetTarget({match.Groups["entity"].Value.Trim()},");

        return body;
    }

    private static LookupInfo FindLookupByKey(
        List<LookupInfo> stateLookups,
        Dictionary<string, LookupInfo> canonicalLookupsByKey,
        string key)
    {
        for (int i = 0; i < stateLookups.Count; i++)
        {
            if (stateLookups[i].Key == key)
            {
                return canonicalLookupsByKey[key];
            }
        }

        return null;
    }

    private static List<LookupInfo> BuildFinalLookups(
        List<LookupInfo> allLookups,
        Dictionary<string, LookupInfo> canonicalLookupsByKey,
        List<GeneratedStateInfo> generatedStates)
    {
        allLookups.Sort(CompareLookupInfos);

        var result = new List<LookupInfo>();
        var seenKeys = new HashSet<string>();

        for (int i = 0; i < allLookups.Count; i++)
        {
            var lookup = allLookups[i];
            if (seenKeys.Add(lookup.Key))
            {
                result.Add(canonicalLookupsByKey[lookup.Key].Clone());
            }
        }

        for (int i = result.Count - 1; i >= 0; i--)
        {
            if (IsLookupUsed(result[i], generatedStates) == false)
            {
                result.RemoveAt(i);
            }
        }

        for (int i = 0; i < result.Count; i++)
        {
            result[i].IsWritable = IsLookupWritable(result[i], generatedStates);
        }

        return result;
    }

    private static int CompareLookupInfos(LookupInfo left, LookupInfo right)
    {
        var typeCompare = string.Compare(left.ElementTypeName, right.ElementTypeName, StringComparison.Ordinal);
        if (typeCompare != 0)
        {
            return typeCompare;
        }

        var lookupTypeCompare = string.Compare(left.LookupTypeName, right.LookupTypeName, StringComparison.Ordinal);
        if (lookupTypeCompare != 0)
        {
            return lookupTypeCompare;
        }

        return string.Compare(left.Name, right.Name, StringComparison.Ordinal);
    }

    private static bool IsLookupUsed(LookupInfo lookup, List<GeneratedStateInfo> generatedStates)
    {
        var pattern = $@"\b{Regex.Escape(lookup.Name)}\b";

        for (int i = 0; i < generatedStates.Count; i++)
        {
            var methods = generatedStates[i].Methods;
            for (int j = 0; j < methods.Count; j++)
            {
                if (Regex.IsMatch(methods[j], pattern))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool IsLookupWritable(LookupInfo lookup, List<GeneratedStateInfo> generatedStates)
    {
        var name = Regex.Escape(lookup.Name);

        for (int i = 0; i < generatedStates.Count; i++)
        {
            var methods = generatedStates[i].Methods;
            for (int j = 0; j < methods.Count; j++)
            {
                var methodText = methods[j];
                if (Regex.IsMatch(methodText, $@"\b{name}\.(Enable|ResetInput|SetTarget)\(") ||
                    Regex.IsMatch(methodText, $@"\b{name}\.TryGetBuffer\(") ||
                    Regex.IsMatch(methodText, $@"\b{name}\[[^\]]+\]\s*="))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static List<MethodBlock> ExtractMethods(string source, string className)
    {
        var classIndex = source.IndexOf($"class {className}", StringComparison.Ordinal);
        if (classIndex < 0)
        {
            throw new InvalidOperationException($"Could not find class {className} in source.");
        }

        var classBodyStart = source.IndexOf('{', classIndex);
        if (classBodyStart < 0)
        {
            throw new InvalidOperationException($"Could not find class body for {className}.");
        }

        var classBodyEnd = FindMatchingBrace(source, classBodyStart);
        var methods = new List<MethodBlock>();
        var searchIndex = classBodyStart + 1;

        while (searchIndex < classBodyEnd)
        {
            var match = MethodStartRegex.Match(source, searchIndex);
            if (match.Success == false || match.Index >= classBodyEnd)
            {
                break;
            }

            var methodName = match.Groups["name"].Value;
            if (methodName == "Refresh")
            {
                searchIndex = SkipMethod(source, match.Index);
                continue;
            }

            var signatureBrace = source.IndexOf('{', match.Index);
            if (signatureBrace < 0 || signatureBrace > classBodyEnd)
            {
                break;
            }

            var methodEnd = FindMatchingBrace(source, signatureBrace);
            var methodText = source.Substring(match.Index, methodEnd - match.Index + 1);

            methods.Add(new MethodBlock(methodName, methodText.TrimEnd()));
            searchIndex = methodEnd + 1;
        }

        return methods;
    }

    private static int SkipMethod(string source, int methodStart)
    {
        var bodyStart = source.IndexOf('{', methodStart);
        return bodyStart < 0 ? methodStart + 1 : FindMatchingBrace(source, bodyStart) + 1;
    }

    private static int FindMatchingBrace(string source, int openingBraceIndex)
    {
        var depth = 0;
        for (int i = openingBraceIndex; i < source.Length; i++)
        {
            if (source[i] == '{')
            {
                depth++;
            }
            else if (source[i] == '}')
            {
                depth--;
                if (depth == 0)
                {
                    return i;
                }
            }
        }

        throw new InvalidOperationException("Could not find the matching closing brace.");
    }

    private static bool IsInterfaceMethod(string methodName)
    {
        return methodName == "Enable" || methodName == "Disable" || methodName == "Update";
    }

    private static string GetGeneratedMethodName(
        SubActionTypes subAction,
        string sourceMethodName,
        Dictionary<string, string> helperRenameMap)
    {
        if (sourceMethodName == "Enable" || sourceMethodName == "Disable" || sourceMethodName == "Update")
        {
            return $"{sourceMethodName}_{subAction}";
        }

        return helperRenameMap[sourceMethodName];
    }

    private static string FindSourcePath(Type type)
    {
        var guids = AssetDatabase.FindAssets($"{type.Name} t:MonoScript");

        for (int i = 0; i < guids.Length; i++)
        {
            var path = AssetDatabase.GUIDToAssetPath(guids[i]);
            var script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
            if (script != null && script.GetClass() == type)
            {
                return path;
            }
        }

        throw new FileNotFoundException($"Could not find source file for {type.FullName}.");
    }

    private sealed class GeneratorContext : IDisposable
    {
        private readonly World world;

        private sealed partial class GeneratorSystem : SystemBase
        {
            protected override void OnUpdate()
            {
            }
        }

        public GeneratorContext(ActionMapBase actionMap)
        {
            ActionMap = actionMap;
            world = new World("ActionRunnerUnmanagedGeneratorWorld");
            System = world.GetOrCreateSystemManaged<GeneratorSystem>();
        }

        public ActionMapBase ActionMap { get; }

        public SystemBase System { get; }

        public Dictionary<SubActionTypes, ISubActionState> CreateStates()
        {
            return ActionMap.ConstructSubActionsStates(System);
        }

        public void Dispose()
        {
            world.Dispose();
        }
    }
}

internal sealed class ActionRunnerUnmanagedGenerationInfo
{
    public ActionRunnerUnmanagedGenerationInfo(
        List<SubActionTypes> subActions,
        List<LookupInfo> lookups,
        List<GeneratedStateInfo> generatedStates)
    {
        SubActions = subActions;
        Lookups = lookups;
        GeneratedStates = generatedStates;
    }

    public List<SubActionTypes> SubActions { get; }

    public List<LookupInfo> Lookups { get; }

    public List<GeneratedStateInfo> GeneratedStates { get; }
}

internal sealed class GeneratedStateInfo
{
    public GeneratedStateInfo(SubActionTypes subAction, List<string> methods)
    {
        SubAction = subAction;
        Methods = methods;
    }

    public SubActionTypes SubAction { get; }

    public List<string> Methods { get; }
}

internal sealed class LookupInfo
{
    public SubActionTypes SubActionType;
    public string Key;
    public string Name;
    public string LookupTypeName;
    public string ElementTypeName;
    public bool IsWritable;

    public LookupInfo Clone()
    {
        return new LookupInfo
        {
            SubActionType = SubActionType,
            Key = Key,
            Name = Name,
            LookupTypeName = LookupTypeName,
            ElementTypeName = ElementTypeName,
            IsWritable = IsWritable,
        };
    }
}

internal sealed class MethodBlock
{
    public MethodBlock(string name, string text)
    {
        Name = name;
        Text = text;
    }

    public string Name { get; }

    public string Text { get; }
}
