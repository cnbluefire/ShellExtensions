

# Shell Extensions

为打包项目提供基于 COM 的资源管理器右键菜单。

<!-- PROJECT SHIELDS -->

[![MIT License][license-shield]][license-url]
[![NUGET][nuget-shellextensions-shield]][nuget-shellextensions-url]
[![NUGET][nuget-explorercontextmenu-shield]][nuget-explorercontextmenu-url]


<!-- PROJECT LOGO -->
<!-- <br />

<p align="center">
  <a href="https://github.com/shaojintian/Best_README_template/">
    <img src="images/logo.png" alt="Logo" width="80" height="80">
  </a>

  <h3 align="center">"完美的"README模板</h3>
  <p align="center">
    一个"完美的"README模板去快速开始你的项目！
    <br />
    <a href="https://github.com/shaojintian/Best_README_template"><strong>探索本项目的文档 »</strong></a>
    <br />
    <br />
    <a href="https://github.com/shaojintian/Best_README_template">查看Demo</a>
    ·
    <a href="https://github.com/shaojintian/Best_README_template/issues">报告Bug</a>
    ·
    <a href="https://github.com/shaojintian/Best_README_template/issues">提出新特性</a>
  </p>

</p> -->

### 项目说明
[ShellExtensions](./docs/ShellExtensions.md)
使用 .net native 发布 shell 扩展

[ExplorerContextMenu](./docs/ExplorerContextMenu.md)
使用 json 配置资源管理器右键菜单

### 注意事项
在 appxmanifest.xml 中配置右键菜单时，desktop5:Verb 的 Id 属性应该以 "C" 开头，否则在文件夹的经典右键菜单中可能不显示。
```xml
<desktop5:ItemType Type="Directory">
    <desktop5:Verb Id="CExplorerContextMenu" Clsid="your guid" />
</desktop5:ItemType>
```

<!-- links -->
[your-project-path]:cnbluefire/ShellExtensions
[license-shield]: https://img.shields.io/github/license/cnbluefire/ShellExtensions.svg?style=flat
[license-url]: https://github.com/cnbluefire/ShellExtensions/blob/main/LICENSE
[nuget-shellextensions-shield]: https://img.shields.io/badge/nuget-ShellExtensions-blue
[nuget-shellextensions-url]: https://www.nuget.org/packages/ShellExtensions
[nuget-explorercontextmenu-shield]: https://img.shields.io/badge/nuget-ExplorerContextMenu-blue
[nuget-explorercontextmenu-url]: https://www.nuget.org/packages/ExplorerContextMenu