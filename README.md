______________________________
Strp 0: Demo
______________________________
This is the demo for my app (YouTube)
https://youtu.be/kJrmuNnO2Bo



______________________________
Step 1: Clone the Repository
______________________________
Open a terminal on your computer.
Clone the GitHub repository containing the script:

git clone https://github.com/levir10/OrLeviHomeTask.git

Navigate to the cloned repository folder:

cd OrLeviHomeTask
______________________________
Step 2: Install .NET 8 SDK
______________________________
Ensure you have the .NET 8 SDK installed:

Download the .NET 8 SDK from the official Microsoft website:
https://dotnet.microsoft.com/
Follow the installation instructions for your operating system.
Verify the installation by running in the terminal:

dotnet --version

(It should display a version number starting with 8.)
______________________________
Step 3: Open the Project in Visual Studio 2022
______________________________
Launch Visual Studio 2022.
Click Open a Project or Solution on the startup screen.
Navigate to the cloned repository folder and select the project or solution file (e.g., YourProjectName.sln).
______________________________
Step 4: Restore Dependencies
______________________________
After opening the project in Visual Studio, open the Package Manager Console:
Go to Tools > NuGet Package Manager > Package Manager Console.
Run the following command to restore the project’s NuGet packages:

dotnet restore
______________________________
Step 5: Configure Build Settings
______________________________
In the Solution Explorer, right-click on the project and select Properties.
Under the Application tab, ensure that the Target Framework is set to .NET 8.0.
Save your changes and close the properties window.
______________________________
Step 6: Build the Project
______________________________
In Visual Studio, select Build > Build Solution or press Ctrl+Shift+B.
Verify that the build completes without errors. Address any issues if they occur.
______________________________
Step 7: Run the Script
______________________________
Set the project as the Startup Project:
Right-click on the project in the Solution Explorer and select Set as Startup Project.
Run the project:
Click Start (the green play button) in the toolbar or press F5.
Follow any prompts or instructions that appear in the console or application window.

______________________________
Step 8: Troubleshooting
______________________________
Error: “SDK not found”:
Ensure the .NET 8 SDK is correctly installed. Run dotnet --info in the terminal to verify.

Missing NuGet Packages:
Check the Output Window in Visual Studio for details and ensure the nuget.org source is enabled.

Runtime Errors:
Consult the repository’s README.md file or the code documentation for specific usage instructions.
