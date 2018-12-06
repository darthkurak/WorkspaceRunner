# WorkspaceRunner
Workspace runner based on Start/Stop powershell scripts - can be used to easily start or stop any, needed for work, resources. For example in Public Cloud. Has multiple mechanism to prevent user from leaving running workspace. For cost optimization. 

## Background
As Azure Developer with Big Data workflow im using a lot of resources. Many vm's, PaaS services like EventHub, StreamAnalytics, Azure Functions, Storages and so on. When I am ending my day-work it is needed to stop or lower to bare minimum all of my resources to not pay for them (or pay as less as i can). There are a lot of available options to do that, but none of them perfectly fit my needs:
- Removing whole Resource Group, and redeploy it when needed - I can't do that as it remove all my state of used resources (VM, EventHub, Storages)
- Use Azure Automation - I found it overcomplicated for my needs and missing integration with services like EventHubs, Stream Analytics and many other. Also, i need to pay for it. 
- Use Auto-shutdown on VM's - It resolve problem only partially - what with others services? And also, auto-shutdown happen only at desired time of the day. When i use resources after that time (for example, checking something after daily-work hours in home), they have to wait for next day
- Powershell scripts - write Azure CLI to do whatever what you want with your resources, save scripts, and launch it every time it needed. - Gotcha! 
This almost fit my needs - i wrote two scripts - Start and Stop, and start them using manually. But then i notice, that sometime i forget launching Stop script when leaving PC for longer period. And then i decide to wrote this App.

## How it can be used?
You will get the most of this application when you are Cloud developer working with many resources which can't be easly auto-shutdown or scale-down when not using. You can prepare two Powershell scripts which start (run, scale-up, create) or stop (stop, scale-down, remove) whatever reosources you need and set up this application to do as much as it can to prevent you from leaving running workspace without stoping it.

## Features
- Select two Powershell scripts: Start and Stop and run them from app
- Current State of workspace based on tracked actions: Stopped, Starting, Running, Stopping
- Auto-start on app launch
- Launch app on system startup
- Track mouse/keyboard activity and if detect idle for specified time - auto-run Stop script
- Prevent closing app or pc when in Running state (start script was launched)
- Prevent going auto-sleep when in Running state (to make idle time happen and Auto-stop workspace)
- Full log of latest execution of Start/Stop scripts
- Preserve State between two different application runs

## Known Issues
- Currently application trying to do as much as it can to prevent user from leaving with Running workspace. Obviously, It can't work when he manually put PC into a sleep (close lid, choose sleep from menu)
- Also, when application is used for executing commands over Internet (in case of starting/stoping resources in Public Cloud), obviously it will not work, when Internet connection is broken. 

## How-to-use
1) Write two scripts in powershell (.ps1): Start and Stop
2) Select them in application
3) Configure settings
4) Enjoy!

## Scripts examples:

#### Azure using Azure CLI (you have to be logged first!):**

1. Start.ps1
  ```
  Start-Job -ScriptBlock {az vm start -g ResourceGroup -n VmName}
  Start-Job -ScriptBlock {az vm start -g ResourceGroup -n VmName}
  Start-Job -ScriptBlock {az eventhubs namespace update --name EventHubNamespace --resource-group ResourceGroup --capacity 20}
  Get-Job | Wait-Job | Receive-Job
  ```
2. Stop.ps1
  ```
  Start-Job -ScriptBlock {az vm stop -g ResourceGroup -n VmName}
  Start-Job -ScriptBlock {az vm stop -g ResourceGroup -n VmName}
  Start-Job -ScriptBlock {az eventhubs namespace update --name EventHubNamespace --resource-group ResourceGroup --capacity 1}
  Get-Job | Wait-Job | Receive-Job
  ```

## Possible Features:
- Server side backend with execution of scripts (remove PC sleep and networking issues)
