How to run?
Introduction
This guide explains how to load and run the OrLeviHomeTask.dll add-in in Autodesk Civil 3D. The add-in provides functionality accessible via the "DBD" command.

________________________________________
Step 1: Locate the Add-in File
Ensure you have the OrLeviHomeTask.dll file. Place it in a known directory on your computer for easy access.
________________________________________
Step 2: Open Autodesk Civil 3D
1.	Launch Civil 3D.
2.	Open or create a new drawing file.
________________________________________
Step 3: Load the Add-in
1.	Open the Command Line in Civil 3D.
(If the command line is not visible, press Ctrl+9 to display it.)
2.	Type the following command and press Enter:
 NETLOAD

3.	In the file browser that appears:
o	Navigate to the directory containing the OrLeviHomeTask.dll.
o	Select the file and click Open.
 

If loaded successfully, Civil 3D will display a confirmation message in the command line.
________________________________________

Step 4: Run the Add-in
1.	In the command line, type the following command and press Enter:
 
DBD
2.	Follow any prompts or instructions that appear. The add-in will execute its functionality.
________________________________________
Step 5: Troubleshooting
•	Error: "Cannot load assembly":
Ensure the .NET Framework version matches the version required by the add-in (.NET8 is the version compatible with Civil3d25)  and that the assembly is unblocked:
1.	Right-click the OrLeviHomeTask.dll file and select Properties.
2.	Check for an Unblock checkbox at the bottom of the General tab. If it exists, select it and click OK.
•	Command not found or unresponsive:
Double-check that the add-in was successfully loaded using the NETLOAD command. Retry if necessary.
•	General errors or crashes:
Contact the developer or consult the add-in documentation for further assistance.
________________________________________
Contact Information
For support or inquiries about the OrLeviHomeTask add-in, please contact me.

















