// Copyright 1998-2018 Epic Games, Inc. All Rights Reserved.

// 第三方lib库 约定格式为
/*
    include
    lib32
        debug
            *.lib;*.dll
        release
            ...
    lib64
        ...
*/

using UnrealBuildTool;
using System.IO;
using System.Collections.Generic;

public class ivrModuleRules : ModuleRules
{
    public ivrModuleRules(ReadOnlyTargetRules Target) : base(Target)
    {
        if (Target.Configuration != UnrealTargetConfiguration.Shipping)
        {
            OptimizeCode = CodeOptimization.Never;
        }
    }

    public string UProjectPath
    {
        // 对应的是项目文件夹
        get { return Path.GetFullPath(Path.Combine(ModuleDirectory, "../../../../")); }
    }
    public string CurrentModulePath
    {
        // 对应的是.uplugin文件所在的目录
        get { return Path.GetFullPath(Path.Combine(ModuleDirectory, "..", "..")); }
    }

    public string[] GetDefaultHeaderFolder()
    {
        return new string[] { "include" };
    }
    public string[] GetDefaultDllFolder()
    {
        List<string> dllPath = new List<string>();

        string tag = "release";
        string lib32 = "lib32";
        if (Target.Configuration < UnrealTargetConfiguration.Development)
        {
            tag = "debug";
        }

        if (Target.Platform == UnrealTargetPlatform.Win64)
        {
            lib32 = "lib64";
        }

        dllPath.Add(Path.Combine(lib32, tag));

        return dllPath.ToArray();
    }

    public void AddThirdParty(string fulldir, string ModuleName)
    {
        AddThirdParty(fulldir, ModuleName, GetDefaultHeaderFolder());
    }
    public void AddThirdParty(string fulldir, string ModuleName, string[] headerFolderList)
    {
        AddThirdParty(fulldir, ModuleName, headerFolderList, GetDefaultDllFolder());

    }
    public void AddThirdParty(string fulldir, string ModuleName, string[] headerFolderList, string[] dllFolderList)
    {
        var RootDir = Path.Combine(fulldir, ModuleName);
        foreach(var header in headerFolderList)
        {
            PublicIncludePaths.Add(Path.Combine(RootDir, header));
        }

        if (Target.Platform == UnrealTargetPlatform.Win64 || Target.Platform == UnrealTargetPlatform.Win32)
        {
            foreach (var it in dllFolderList)
            {
                // 遍历出所有的lib，都加入到依赖；所有的dll，都自动拷贝到binary文件夹
                var dllfolder = Path.Combine(RootDir, it);
                var libs = Directory.GetFiles(dllfolder, "*.lib");
                foreach (var lib in libs)
                {
                    System.Console.WriteLine(lib);

                    PublicAdditionalLibraries.Add(lib);
                }
                var dlls = Directory.GetFiles(dllfolder, "*.dll");
                foreach (var dll in dlls)
                {
                    System.Console.WriteLine(dll);

                    PublicDelayLoadDLLs.Add(Path.GetFileName(dll));
                    RuntimeDependencies.Add(dll);
                    CopyToBinary(dll, Path.GetFileName(dll), Target);
                }
            }
        }
        else
        {
            throw new System.Exception("not implement");
        }
    }
    public string CopyToBinary(string FilePath, string FileName, ReadOnlyTargetRules Target)
    {
        string binariesDir = Path.GetFullPath(Path.Combine(UProjectPath, "Binaries", Target.Platform.ToString()));

        if (!Directory.Exists(binariesDir))
            Directory.CreateDirectory(binariesDir);

        // 覆盖拷贝
        string FullReturnPath = Path.Combine(binariesDir, FileName);
        try
        {
            System.Console.WriteLine(string.Format("from:{0}, to:{1}", FilePath, FullReturnPath));
            File.Copy(FilePath, FullReturnPath, true);
        }
        catch (System.Exception ex)
        {

        }
        return FullReturnPath;
    }
}

public class zThird : ivrModuleRules
{
	public zThird(ReadOnlyTargetRules Target) : base(Target)
	{
		PCHUsage = ModuleRules.PCHUsageMode.UseExplicitOrSharedPCHs;
		
		PublicIncludePaths.AddRange(
			new string[] {
				// ... add public include paths required here ...
			}
			);
				
		
		PrivateIncludePaths.AddRange(
			new string[] {
				// ... add other private include paths required here ...
			}
			);
			
		
		PublicDependencyModuleNames.AddRange(
			new string[]
			{
			
				// ... add other public dependencies that you statically link with here ...
			}
			);
			
		
		PrivateDependencyModuleNames.AddRange(
			new string[]
			{
				"Core",				
				// ... add private dependencies that you statically link with here ...	
			}
			);
		
		
		DynamicallyLoadedModuleNames.AddRange(
			new string[]
			{
				// ... add any modules that your module loads dynamically here ...
			}
			);
        if (Target.Type == TargetRules.TargetType.Editor)
        {
            PrivateDependencyModuleNames.AddRange(
                new string[]
                {
					
                }
                );
        }
    }
}
