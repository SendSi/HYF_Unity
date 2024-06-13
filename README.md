### 首次出包
###### 1.HyBridCLR/Generate/All    
###### 2.HyBridCLR/Generate/All_Coopy_replace_dlls_to_bytes
###### 3.改对AppConfig.cs的resVersion字段与(YooAsset/AssetBundleBuilder的BuildVersion值相等),使用ClearAndCopyAll,然后build两个包
###### 4.执行.YooAsset/CopyWWW_复制下,然后启动web服务器.http-server --port 80 -b --cors
###### 5.正常出apk或exe,启动打开游戏

### 出增量包,即热更
###### 1.HyBridCLR/CompileDll/ActiveBuildTarget    
###### 2.HyBridCLR/Generate/All_Coopy_replace_dlls_to_bytes
###### 3.改对AppConfig.cs的resVersion字段与(YooAsset/AssetBundleBuilder的BuildVersion值相等),使用None,然后build两个包
###### 4.执行.YooAsset/CopyWWW_复制下,然后启动web服务器.http-server --port 80 -b --cors
###### 5.重启打开游戏