# DesktopPet

Windows 11 x64 桌宠。功能范围以 `PROJECT_DESIGN.md` 为准。

- 当前正式版本：v0.0.1
- 当前开发版本：v0.0.2-dev（未发布）

## 下载与运行

普通用户从 GitHub 仓库的 Releases 页面下载已发布的 v0.0.1：

    DesktopPet-v0.0.1-win-x64.zip

解压完整 ZIP 后双击 `DesktopPet.exe`。程序为自包含发布版，不要求单独安装 .NET；不能只复制或下载 EXE。

## 操作

- 左键按住桌宠并移动鼠标：拖动桌宠。
- 右键单击系统托盘图标：显示、隐藏或退出桌宠。
- 双击系统托盘图标：重新显示桌宠。

## 更换图片

v0.0.2-dev 优先读取发布目录中的文件：

    可替换桌宠/pet.png

退出程序后，用带透明通道的 PNG 覆盖该文件并保持文件名 `pet.png`，然后重新启动程序。如果该文件不存在，程序回退到：

    Assets/DefaultPet/pet.png

## 开发环境

需要 Windows 11 x64、Git 和 .NET 10 SDK。仓库通过 `global.json` 接受 .NET 10 的可用 Feature Band。

克隆并构建：

    git clone https://github.com/Yunbaby8/pet.git
    cd pet
    dotnet --info
    dotnet build DesktopPet.sln -c Release

开发运行：

    dotnet run --project src/DesktopPet.App/DesktopPet.App.csproj

## 发布

以下命令记录已发布的 v0.0.1；v0.0.2-dev 当前不创建 Release：

    dotnet publish src/DesktopPet.App/DesktopPet.App.csproj -c Release -r win-x64 --self-contained true -o dist/DesktopPet-v0.0.1-win-x64

`dist/` 是本地构建输出，不提交到 Code 页面。正式版本将完整发布目录压缩为 ZIP，并单独上传到 GitHub Releases。

本地正式发布归档格式：

    releases/v版本号/DesktopPet-v版本号-win-x64.zip

当前正式归档仍为 `releases/v0.0.1/`；`releases/` 不提交到 Code 页面。
