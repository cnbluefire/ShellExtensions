# ExplorerContextMenu

使用 json 配置资源管理器的右键菜单

```ps1
dotnet add package ExplorerContextMenu
```

```json
{
  "$schema": "https://raw.githubusercontent.com/cnbluefire/ShellExtensions/src/ExplorerContextMenu/ExplorerContextMenu.Schema.json",
  "menuItems":[
    {
      "guid": "your guid",
      "title": {
        "zh":"标题",
        "en":"Title",
      },
      "toolTip": {
        "zh":"工具提示",
        "en":"ToolTip",
      },
      "icon":{
        "embeddedIconFile":"c:\\xxx\\yyy.ico"
      },
      "executeOptions":{
        "command": "notepad.exe {item}"
      }
    }
  ]
}
```
