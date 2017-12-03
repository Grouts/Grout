MSBuild.NodeTools
============
[![Gitter](https://badges.gitter.im/Join Chat.svg)](https://gitter.im/kmees/MSBuild.NodeTools?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
> Run various Node tools as a MSBuild task.

:warning: This is still a WIP, please report any issues. Thanks ! :warning:

Installation
------------

### NuGet

MSBuild.NodeTools is available as a NuGet Package with the same name. This
package contains all the available tools.

```
Install-Package MSBuild.NodeTools
```

It is also possible to install each tool seperately.

```
Install-Package MSBuild.Npm
Install-Package MSBuild.Bower
Install-Package MSBuild.Gulp
Install-Package MSBuild.Grunt
```

NuGet will automatically add the MSBuild targets to the `.csproj` file.

### Manually

Download the files in the `build/` folder and put them in your project folder.
Then open the `.csproj` file in an editor and do the following changes.

```xml
<Project ToolsVersion="12.0" DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
    <!-- Put this at the top of the Project node -->
    <Import Project="..\path\to\MSBuild.Node.props" Condition="Exists('..\path\to\MSBuild.Node.props')" />
    <!-- Import *.props file for the tool(s) that you need -->

    <!--
      other stuff goes here...
    -->

    <!-- Put this at the bottom of the Project node -->
    <Import Project="..\path\to\MSBuild.Node.targets" Condition="Exists('..\path\to\MSBuild.Node.targets')" />
    <!-- Import *.targets file for the tool(s) that you need -->
</Project>
```

Note that the `MSBuild.Node.props` and `MSBuild.Node.targets` is required by
all the tools and must always be included.

Configuration
-------------

There are various configuration properties you can overwrite in the `.csproj` file 
**after** importing `MSBuild.*.props` but **before** importing `MSBuild.*.targets`.

### MSBuild.Node

MSBuild.Node tries to find the global *NodeJS* by reading the `NODEJS` environment
variable first and then executing `where node` when it is unset.
It also tries to locate the global *npm* folder.  The global paths are stored in
the following properties which should **not** be overriden.

  * `GlobalNodePath`: Defaults to `$(NODEJS)` or `where node`
  * `GlobalNodeModulePath`: Defaults to `$(HOMEDRIVE)$(HOMEPATH)\AppData\Roaming\npm`

If no global *NodeJS* installation is available or if a local installation should 
be used instead, set these properties instead:

  * `LocalNodePath`: Path to the local *NodeJS* installation.
  * `LocalNodeModulePath`: Path to the local npm modules folder.

### MSBuild.Npm

  * `NpmFile`: Path to the npm file. Defaults to `$(MSBuildProjectDirectory)\package.json`.
  * `NpmCommand`: Npm command that should be run. Defaults to `install --loglevel error`.

### MSBuild.Bower

  * `BowerFile`: Path to the bower file. Defaults to `$(MSBuildProjectDirectory)\bower.json`.
  * `BowerCommand`: Bower command that should be run. Defaults to `install`.
  
  If no global *Git* installation is available or if a local installation should 
  be used instead, set these properties instead:
  * `LocalGitPath`: Path to the local *git* installation.

### MSBuild.Gulp

  * `GulpFile`: Path to gulpfile. Defaults to `$(MSBuildProjectDirectory)\gulpfile.[js|coffee]`.
  * `GulpWorkingDirectory`: Directory in which context the gulp task gets executed. Defaults to `$(MSBuildProjectDirectory)`.
  * `GulpTask`: **[deprecated]** See `GulpBuildTask`.
  * `GulpBuildTask`: Gulp task that gets executed on build. Defaults to `build-$(Configuration)`.
  * `GulpCleanTask`: Gulp task that gets executed on clean. Defaults to *unset*.

### MSBuild.Grunt

  * `GruntFile`: Path to gruntfile. Defaults to `$(MSBuildProjectDirectory)\gruntfile.[js|coffee]`.
  * `GruntWorkingDirectory`: Directory in which context the grunt task gets executed. Defaults to `$(MSBuildProjectDirectory)`.
  * `GruntTask`: **[deprecated]** See `GruntBuildTask`.
  * `GruntBuildTask`: Grunt task that gets executed on build. Defaults to `build-$(Configuration)`.
  * `GruntCleanTask`: Grunt task that gets executed on clean. Defaults to *unset*.

Release History
---------------

  * **v0.5.1**
    * Add `ConsoleToMsBuild` flag to *gulp* and *grunt* task (this time for real).
  * **v0.5.0**
    * Bugfix for npm and bower working directory.
    * Support for local git when using bower.
    * :warning: Remove quotes around `GruntBuildTask` and `GruntCleanTask`. In case that breaks some,
      please open an issue.
    * Add `ConsoleToMsBuild` flag to *gulp* and *grunt* task.
  * **v0.4.2** Performance Improvements  
    * Run npm targets before `BeforeBuild`/`BeforeClean` instead of `PrepareForBuild`.
    * Run bower targets before `BeforeBuild` instead of `PrepareForBuild`.
  * **v0.4.1** Bugfixes
  * **v0.4.0** MSBuild.Grunt and MSBuild.Gulp support `clean` target.
