## Use Azure app service editor

1. make code change in the online editor
2. open the console window and run

```
build.cmd
```

## Use Visual Studio 

### Build and debug
1. download source code zip and extract source in local folder
2. open {PROJ_NAME}.sln in Visual Studio
3. build and run the bot
4. download and run [botframework-emulator](https://emulator.botframework.com/)
5. connect the emulator to http://localhost:3987

### Publish back

In Visual Studio, right click on {PROJ_NAME} and select 'Publish'

For first time publish after downloading source code
1. In the publish profiles tab, click 'Import'
2. Browse to 'PostDeployScripts' and pick '{SITE_NAME}.publishSettings'


## Use continuous integration

If you have setup continuous integration, then your bot will automatically deployed when new changes are pushed to the source repository.

## Add new languages

You can add new languages by using the resgen.exe tool of visual studio. see https://docs.microsoft.com/en-us/dotnet/framework/tools/resgen-exe-resource-file-generator ->
-> the section about strongly typed resources [lol](default.htm)
The multilingual App Toolkit doesn't work on this project since this is a VS 2017 project and
MAT only supports 2013 and 2015 versions.

## Add new json files
Add a json file to the project that has similar synta to the JsonDummyForBot.json. Right-click on the file, select properties and set Build action to
embedded ressource. Afterwards change method BuildJsonForm() in BasicLuisDialog to contain the name of the newly created manifest ressource.
