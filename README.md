# DesktopPet v0.0.1

Windows 11 x64 桌宠。功能范围以 `PROJECT_DESIGN.md` 为准。

## 下载与运行

普通用户从 GitHub 仓库的 Releases 页面下载：

    DesktopPet-v0.0.1-win-x64.zip

解压完整 ZIP 后双击 `DesktopPet.exe`。程序为自包含发布版，不要求单独安装 .NET；不能只复制或下载 EXE。

## 操作

- 左键按住桌宠并移动鼠标：拖动桌宠。
- 右键单击系统托盘图标：显示、隐藏或退出桌宠。
- 双击系统托盘图标：重新显示桌宠。

## 更换图片

替换发布目录中的文件：

    Assets/DefaultPet/pet.png

新图片必须是带透明通道的 PNG，并继续使用文件名 `pet.png`。替换后重新启动程序。

## 构建

开发环境需要 Windows 11 x64 和 .NET 10 SDK。在项目根目录运行：

    dotnet build DesktopPet.sln -c Release

## 发布

    dotnet publish src/DesktopPet.App/DesktopPet.App.csproj -c Release -r win-x64 --self-contained true -o dist/DesktopPet-v0.0.1-win-x64

`dist/` 是本地发布输出，不提交到 Code 页面。正式版本将完整发布目录压缩为 ZIP，并单独上传到 GitHub Releases。
本地最终发布归档统一保存在：

    releases/v版本号/DesktopPet-v版本号-win-x64.zip

v0.0.1 的 ZIP 和 SHA-256 校验文件位于 `releases/v0.0.1/`。`releases/` 不提交到 Code 页面。