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

```
        string MyThird = Path.GetFullPath(Path.Combine(CurrentModulePath, "ThirdParty"));        
        AddThirdParty(MyThird, "evpp", new string[] { ".", "3dparty" });

        Definitions.Add("GOOGLE_PROTOBUF_NO_RTTI=0");
        Definitions.Add("GOOGLE_PROTOBUF_USE_UNALIGNED=0");
        AddThirdParty(Path.Combine(MyThird), "protobuf");
```

