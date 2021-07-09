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

public class zThird : ivrModuleRules
{
	public zThird(ReadOnlyTargetRules Target) : base(Target)
    {
        if (Target.Configuration != UnrealTargetConfiguration.Shipping)
        {
            OptimizeCode = CodeOptimization.Never;
        }

        PCHUsage = ModuleRules.PCHUsageMode.UseExplicitOrSharedPCHs;
				
		PrivateDependencyModuleNames.AddRange(
			new string[]
			{
				"Core",				
				// ... add private dependencies that you statically link with here ...	
			}
			);
		
    }
}

public class ivrModuleRules : ModuleRules
{
    public ThirdUtils thirdUtils;
    public ivrModuleRules(ReadOnlyTargetRules Target) : base(Target)
    {
        if (Target.Configuration != UnrealTargetConfiguration.Shipping)
        {
            OptimizeCode = CodeOptimization.Never;
        }
        thirdUtils = new ThirdUtils(this, Target);
    }

    #region thirdutils
    // <-- Begin ThirdUtils
    // 方便引入第三方库。将自动引用文件夹内的所有dll
    public class ThirdUtils
    {
        /*
         *  譬如
            约定目录为：
                ThirdParty
                    XXX_LIB
                        include
                        win32
                        win64
                            debug
                                *.dll
                                *.lib
                            release
                        linux                    

        */
        public ThirdUtils(ModuleRules InTarget, ReadOnlyTargetRules InTargetRules)
        {
            Module = InTarget;
            Target = InTargetRules;
        }

        ModuleRules Module;
        ReadOnlyTargetRules Target;

        public string UProjectPath
        {
            // 对应的是项目文件夹
            get { return Path.GetFullPath(Path.Combine(Module.ModuleDirectory, "../../../../")); }
        }
        public string CurrentModulePath
        {
            // 对应的是.uplugin文件所在的目录
            get { return Path.GetFullPath(Path.Combine(Module.ModuleDirectory, "..", "..")); }
        }
        public string ThirdPartyPath
        {
            // 对应的是.uplugin文件所在的目录下的ThirdParty文件夹
            get { return Path.GetFullPath(Path.Combine(CurrentModulePath, "ThirdParty")); }
        }

        // 约定的路径
        public string[] GetDefaultHeaderFolder()
        {
            return new string[] { "include" };
        }
        public string[] GetDefaultDllFolder()
        {
            return GetDefaultDllFolder("");
        }
        public string[] GetDefaultDllFolder(string fullpath)
        {
            List<string> dllPath = new List<string>();

            string tag = "release";
            string lib32 = "lib32";

            if (Target.Configuration <= UnrealTargetConfiguration.Development)
            {
                tag = "debug";

                // 如果没有debug文件夹，依然使用release文件夹
                if (fullpath != "")
                {
                    if (!Directory.Exists(Path.Combine(fullpath, tag)))
                    {
                        tag = "release";
                    }
                }
            }

            if (Target.Platform == UnrealTargetPlatform.Win64)
            {
                lib32 = "lib64";
            }

            dllPath.Add(Path.Combine(lib32, tag));

            return dllPath.ToArray();
        }

        // 添加依赖的头文件
        public void AddPrivateHeader(string ModuleName)
        {
            AddPrivateHeader(ThirdPartyPath, ModuleName);
        }
        public void AddPrivateHeader(string fulldir, string ModuleName)
        {
            AddPrivateHeader(fulldir, ModuleName, GetDefaultHeaderFolder());
        }
        public void AddPrivateHeader(string fulldir, string ModuleName, string[] headerFolderList)
        {
            System.Console.WriteLine("AddPrivateHeader:" + ModuleName);
            var RootDir = Path.Combine(fulldir, ModuleName);
            foreach (var header in headerFolderList)
            {
                Module.PrivateIncludePaths.Add(Path.Combine(RootDir, header));
            }
        }

        // 添加依赖的dll+头文件
        public void AddThirdParty(string ModuleName)
        {
            AddThirdParty(ThirdPartyPath, ModuleName);
        }
        public void AddThirdParty(string fulldir, string ModuleName)
        {
            AddThirdParty(fulldir, ModuleName, GetDefaultHeaderFolder());
        }
        public void AddThirdParty(string fulldir, string ModuleName, string[] headerFolderList)
        {
            AddThirdParty(fulldir, ModuleName, headerFolderList, GetDefaultDllFolder(Path.Combine(fulldir, ModuleName)));
        }
        public void AddThirdParty(string fulldir, string ModuleName, string[] headerFolderList, string[] dllFolderList)
        {
            System.Console.WriteLine("AddThirdParty:" + ModuleName);
            var RootDir = Path.Combine(fulldir, ModuleName);
            foreach (var header in headerFolderList)
            {
                Module.PublicIncludePaths.Add(Path.Combine(RootDir, header));
            }

            if (Target.Platform == UnrealTargetPlatform.Win64 || Target.Platform == UnrealTargetPlatform.Win32)
            {
                foreach (var it in dllFolderList)
                {
                    // 遍历出所有的lib，都加入到依赖；所有的dll，都自动拷贝到binary文件夹
                    var dllfolder = Path.Combine(RootDir, it);

                    if (!Directory.Exists(dllfolder))
                    {
                        continue;
                    }

                    var libs = Directory.GetFiles(dllfolder, "*.lib");
                    foreach (var lib in libs)
                    {
                        System.Console.WriteLine(lib);

                        Module.PublicAdditionalLibraries.Add(lib);
                    }
                    var dlls = Directory.GetFiles(dllfolder, "*.dll");
                    foreach (var dll in dlls)
                    {
                        System.Console.WriteLine(dll);

                        //PublicDelayLoadDLLs.Add(Path.GetFileName(dll));
                        var fullpath = CopyToBinary(dll, Path.GetFileName(dll), Target);
                        Module.RuntimeDependencies.Add(fullpath);
                    }
                }
            }
            else
            {
                //throw new System.Exception("not implement");
            }
        }

        string CopyToBinary(string SrcFile, string FileName, ReadOnlyTargetRules Module)
        {
            // Binaries\Win64\
            string BinariesDir = Path.GetFullPath(Path.Combine(UProjectPath, "Binaries", Module.Platform.ToString()));

            if (!Directory.Exists(BinariesDir))
            {
                Directory.CreateDirectory(BinariesDir);
            }

            // 覆盖拷贝
            string DestFile = Path.Combine(BinariesDir, FileName);
            try
            {
                System.Console.WriteLine(string.Format("auto copy. from:{0}, to:{1}", SrcFile, DestFile));
                File.Copy(SrcFile, DestFile, true);
            }
            catch (System.Exception)
            {

            }
            return DestFile;
        }
    }
    // <-- End. ThirdUtils
    #endregion

}