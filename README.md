# FiatFighterz-Server

## Installation
The server can either be run locally or hosted on AWS. 
### local
1. Download this as a zip or clone from git. 
    * To run the server immedietly simpily run the file /Fiat-Fighterz-Server/RUN.LOCAL.ALL/Bin/Debug/netcoreapp3.0/Run.Local.All.exe
    * To run this in visual studio open the Titan-Project.sln file. You will also need the library, checkout the Library repository for
      instructions on installing this.
2. Open Visual studio, select the file to run as run.local.all and set the verision to Debug. On the client side make sure that it is run 
  in debug as well
### Adding servers / reset server 
The server on the client side and server side needs to point to a web address
![Screen Shot 2022-02-08 at 4 42 49 PM](https://user-images.githubusercontent.com/36010164/153080586-c2700a66-7c9c-4f17-90a2-e053fc5d25a2.png)

click on Public IPv4 DNS

![Screen Shot 2022-02-08 at 4 43 33 PM](https://user-images.githubusercontent.com/36010164/153080689-9d96fe96-b0bf-460f-aaad-13173b6e9f63.png)
this will give you the server name needed to reset the server adddress.


On the server these changes need to be made at WebServer.cs
   * private const string Prefix = "new server address";
There also needs to be a change made to ServerList.cs
   * Add a new webserver info for a new server, change the ip address of the broken one if an old server

On the Client these changes need to be made at WebClient.cs 
   * Web_Server_Url = "new server name"
   * Debug_Web_Server_Url = "new server address"
   
# known issues 
Failed to create CoreCLR, HRESULT: 0x80004005

The target process exited without raising a CoreCLR started event. Ensure that the target process is configured to use .NET Core. This may be expected if the target process did not run on .NET Core.
has exited with code 137 (0x89).

* need to update the .net sdk to incude the older versions such as 2.0
* https://dotnet.microsoft.com/en-us/download/dotnet/2.0


/Users//.nuget/packages/nethereum.autogen.contractapi/4.2.0/build/Nethereum.Autogen.ContractApi.targets(5,5): Error MSB3073: The command "dotnet ~/.nuget/packages/nethereum.autogen.contractapi/4.2.0/tools/Nethereum.Generator.Console.dll generate from-project -p "/Users/wewlad/GitHub/FiatFighterz-Server 2/WebServer/WebServer.csproj" -a "WebServer.dll"" exited with code 1. (MSB3073) (WebServer)

exited with code 137. (MSB3073) (WebServer)

* Completely uninstall nutget package related to the issue. 
* These can get broken in updates of visual studio. 
* I dont know what causes this specifically but it may get broken in nuget updates also.
