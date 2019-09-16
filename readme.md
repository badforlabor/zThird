### 目的

- 使得UE4能够更方便的依赖第三方库



### 约定

```
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
```

那么在打包，或者编译dll的时候，将自动引用依赖以及拷贝dll



### 示例

在uplugin文件中，加入引用字段"AdditionalDependencies"

```
//ivrEvpp.uplugin文件
{
	"FileVersion": 3,
	"Version": 1,
	"VersionName": "1.0",
	"FriendlyName": "ivrEvpp",
	"Description": "",
	"Category": "Other",
	"CreatedBy": "",
	"CreatedByURL": "",
	"DocsURL": "",
	"MarketplaceURL": "",
	"SupportURL": "",
	"CanContainContent": true,
	"IsBetaVersion": false,
	"Installed": false,
	"Modules": [
    {
      "Name": "ivrEvpp",
      "Type": "Runtime",
      "LoadingPhase": "Default",
      "AdditionalDependencies": [
        "zThird"
      ]
    }
	]
}
```

在build.cs中，定义

```
// ivrEvpp.build.cs
// Copyright 1998-2019 Epic Games, Inc. All Rights Reserved.

using UnrealBuildTool;
using System.IO;

public class ivrEvpp : ivrModuleRules
{
    public ivrEvpp(ReadOnlyTargetRules Target) : base(Target)
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
                "Core",
                "Projects"
				// ... add other public dependencies that you statically link with here ...
			}
            );


        PrivateDependencyModuleNames.AddRange(
            new string[]
            {
				// ... add private dependencies that you statically link with here ...	
			}
            );


        DynamicallyLoadedModuleNames.AddRange(
            new string[]
            {
				// ... add any modules that your module loads dynamically here ...
			}
            );

		// *********   重点在这里 ************
        string MyThird = Path.GetFullPath(Path.Combine(CurrentModulePath, "ThirdParty"));        
        AddThirdParty(MyThird, "evpp", new string[] { ".", "3dparty" });

        Definitions.Add("GOOGLE_PROTOBUF_NO_RTTI=0");
        Definitions.Add("GOOGLE_PROTOBUF_USE_UNALIGNED=0");
        AddThirdParty(Path.Combine(MyThird), "protobuf");

        if (Target.Platform == UnrealTargetPlatform.Win64)
        {

        }
        else
        {
            throw new System.Exception("failed");
        }
    }
}
```

