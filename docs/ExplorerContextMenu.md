# ExplorerContextMenu

使用 json 配置资源管理器的右键菜单

```ps1
dotnet add package ExplorerContextMenu
```

支持的项目配置
```xml
<PropertyGroup>
    <!-- 是否启用 ExplorerContextMenu -->
    <ExplorerContextMenuEnabled>True</ExplorerContextMenuEnabled>
    <!-- 生成的 dll 文件名 -->
    <ExplorerContextMenuOutputFileName>ExplorerContextMenu.dll</ExplorerContextMenuOutputFileName>
</PropertyGroup>
```

Json 文件示例

```jsonc
{
  "$schema": "https://raw.githubusercontent.com/cnbluefire/ShellExtensions/refs/heads/main/src/ExplorerContextMenu/ExplorerContextMenu.Schema.json",
  
  // 存储菜单项配置的注册表键，只支持存储到 HKCU 中，默认为 Software\\{pfn}\\ExplorerContextMenu
  "configRegistryKey":"Software\\{pfn}\\ExplorerContextMenu",

  // 菜单项集合
  // 支持的环境变量：
  // dllPath        : 菜单 dll 的完整路径

  // dllFolder      : 菜单 dll 所在目录，包含尾随的'\'

  // packageFolder  :
  // appFolder      : 打包模式下 app 的安装目录，包含尾随的'\'

  // packageFamilyName :
  // pfn            : package family name

  // currentFolder  :
  // currentDirectory :
  // currentDir     : 当前所在文件夹，包含尾随的'\'

  // items          : 选中的文件或文件夹路径，仅在 MultipleFilesOperation 为 Join 时可用，使用 PathDelimiter 分隔
  // item           : 选中的文件或文件夹路径，MultipleFilesOperation 为 OneByOne 时返回正在处理的文件，Join 时返回选中文件中的第一个
  // itemName       : 选中的文件或文件夹名

  // 本地化字符串字典的 key 不处理大小写

  "menuItems":[
    {
      "guid": "your guid",

      // 菜单项的标题，支持本地化
      "title": {
        "zh":"标题",
        "en":"Title",
      },

      // 菜单项的工具提示，支持本地化
      "toolTip": {
        "zh":"工具提示",
        "en":"ToolTip",
      },

      "checkOptions": {

        // 菜单项是否可选中
        // 可选中的菜单在用户点击时，会自动向注册表中 HKCU\{configRegistryKey}\{guid}\Checked 写入字符串 "0" 或 "1"
        // {guid} 用连字符分隔的 32 位数字，用大括号括起来
        // 如 {00000000-0000-0000-0000-000000000000}
        "isCheckable": false,
        
        // 菜单项选中类型，支持 check 和 radioCheck
        "checkType": "check",

        // 菜单项组名，每个组内当前只允许有一个项选中
        "groupName": "any group name",

        // 默认是否被选中
        "defaultChecked": false
      },

      "flags": {
        
        // 此菜单项显示为一条分隔线
        // 仅在经典菜单中可见
        "isSeparator": false,

        // 显示 UAC 盾牌标志
        // 设置此项后将使用管理员权限执行 executeOptions.command
        // 仅在经典菜单中可见
        "hasLuaShield": false,

        // 此菜单项上方显示一条分隔线
        // 仅在经典菜单中可见
        "separatorBefore": false,

        // 此菜单项下方显示一条分隔线
        // 仅在经典菜单中可见
        "separatorAfter": false
      },
      
      // 菜单项的图标
      "icon": {

        // 引用其他文件中的图标资源，支持 .exe / .dll / .ico
        // 绝对路径，可以使用变量
        // "iconFile": "shell32.dll,-249",

        // 嵌入生成的 dll 中的 ico 文件，
        // 相对于当前 json 文件的路径
        // "embeddedIconFile":"c:\\xxx\\yyy.ico",
        
        // 需要嵌入 dll 内的 png 资源, key 为尺寸, value 为 png 图片路径。
        // 生成内嵌 ico 时, png 将自动缩放至 key 指定的尺寸。
        // 如 ["16"] = "Assets\16x16.png", ["32"] = "Assets\32x32.png"
        // 支持的尺寸: 256, 64, 48, 40, 32, 24, 20, 16 
        // 相对于当前 json 文件的路径
        "embeddedPngFiles":{
          "48": "Images\\LockScreenLogo.scale-200.png"
        }
      },

      // 暗色模式下的图标，参数同 icon，如果为 null 则显示 icon
      "darkIcon": null,
      "executeOptions":{
        "command": "notepad.exe \"{item}\"",

        // 设置为 true 时, command 中的环境变量将被 uri encode
        "commandIsUri": false,

        // 选中多个文件时点击的行为
        // oneByOne - 选中多个文件时, 分别对每个文件执行一次 Command。此选项不会在 {item} 两侧自动拼接引号
        // spaceSeparatedItemsWithPrefix - 选中多个文件时, 将每个文件作为一个参数, 并且拼接前缀。此选项会在每个{items}两侧自动拼接引号。如: --input "c:\file1" --input "c:\file2"
        // allItemsWithDelimiter - 选中多个文件时, 将所有文件拼接到一个参数中。分隔符不能使用空格。如: "c:\file1,c:\file2"
        "multipleItemsOperation": "oneByOne",

        // 当 multipleItemsOperation 为 allItemsWithDelimiter 时，在 {items} 两侧拼接此处指定的字符
        "itemPathDelimiter": ",",

        // 当 multipleItemsOperation 为 spaceSeparatedItemsWithPrefix 时，在 {items} 每个项之前拼接此处指定的字符，如 "--input "
        "itemPathArgumentPrefix": ""
      },
      "visibilityOptions":{
        
        // 菜单项是否可见，默认为 true
        "show": true,

        // 在 Windows 11 的现代右键菜单中可见，默认为 true
        "showInModernContextMenu": true,
        
        // 在经典右键菜单中可见，默认为 true
        "showInClassicContextMenu": true,
        
        // 选中项包含文件时是否可见，默认为 true
        "showWithFile": true,
        
        // 选中项包含文件夹时是否可见，默认为 true
        "showWithFolder": true,
        
        // 选中项包含多个项目时是否可见，默认为 true
        "showWithMultipleItems": true,
        
        // 是否允许在注册表中重写上述配置
        // 为 true 时会从 HKCU\{configRegistryKey}\{guid}\{对应的属性} 中读取字符串值，"0" 为 false，"1" 为 "true"
        // 注册表中的键名为属性名将首字母大写，如 showWithMultipleItems 将读取 HKCU\{configRegistryKey}\{guid}\ShowWithMultipleItems 的值
        // {guid} 用连字符分隔的 32 位数字，用大括号括起来
        // 如 {00000000-0000-0000-0000-000000000000}
        "registryOverrideSupport": true,
      },
      
      // 子菜单的 guid
      // 设置此项后将忽略 checkOptions 和 executeOptions
      "subMenuItems": [
        // "your sub guid"
      ]
    },
    {
      "guid": "your sub guid",
      "title": {
        "en": "submenu"
      }
    }
  ]
}
```
